using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// The Sapphire key sitting in the opened chest as one more slot beside the relics, shown to every player from the start.
/// Every player still choosing can vote for it: the gem is then clickable, with the game's reward-alternative button
/// under it for the controller; it is dimmed only for players who are done or while a round plays, and glows once the
/// local player voted for it.
/// </summary>
internal sealed class SapphireKeyView
{
	private const float GemSize = 88f;

	private readonly Control _holder;
	private readonly TextureButton _gem;
	private readonly Control _buttonHolder;
	private readonly NCardRewardAlternativeButton _button;
	private readonly Func<bool> _take;

	private SapphireKeyView(Control room, Func<bool> take)
	{
		_take = take;
		_holder = new Control { MouseFilter = Control.MouseFilterEnum.Ignore, Visible = false };
		_gem = new TextureButton
		{
			TextureNormal = new AtlasTexture
			{
				Atlas = PreloadManager.Cache.GetTexture2D(KeySprites.Path(Act4Key.Sapphire, large: true)),
				Region = new Rect2(70, 70, 116, 116),
			},
			IgnoreTextureSize = true,
			StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
			Size = new Vector2(GemSize, GemSize),
			Position = new Vector2(-GemSize / 2f, -GemSize / 2f),
			PivotOffset = new Vector2(GemSize / 2f, GemSize / 2f),
			FocusMode = Control.FocusModeEnum.None,
		};
		_gem.MouseEntered += () => _gem.Scale = Vector2.One * (_gem.Disabled ? 1f : 1.12f);
		_gem.MouseExited += () => _gem.Scale = Vector2.One;
		_gem.Pressed += Take;
		_holder.AddChild(_gem);

		var buttonHolder = new Control { Position = new Vector2(0, GemSize * 0.75f), Scale = Vector2.One * 0.85f,
			MouseFilter = Control.MouseFilterEnum.Ignore };
		_buttonHolder = buttonHolder;
		_button = NCardRewardAlternativeButton.Create(
			new LocString("merchant_room", "SPOON_SAPPHIRE_KEY").GetFormattedText(), MegaInput.viewDrawPile);
		buttonHolder.AddChild(_button);
		_holder.AddChild(buttonHolder);
		_button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => Take()));

		room.AddChild(_holder);
		// The reward button scene carries screen-relative offsets; reset them inside this slot.
		_button.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
		_button.Position = new Vector2(-_button.Size.X / 2f, 0);

		// A slow bob, like a relic waiting in the chest.
		var bob = _gem.CreateTween().SetLoops();
		bob.TweenProperty(_gem, "position:y", -GemSize / 2f - 8f, 1.2).SetTrans(Tween.TransitionType.Sine);
		bob.TweenProperty(_gem, "position:y", -GemSize / 2f + 8f, 1.2).SetTrans(Tween.TransitionType.Sine);
	}

	public static SapphireKeyView Add(Control room, Func<bool> take) => new(room, take);

	/// <summary>Puts the key's center at <paramref name="globalCenter"/>, the slot it takes beside the relics.</summary>
	public void Place(Vector2 globalCenter)
	{
		if (GodotObject.IsInstanceValid(_holder)) _holder.GlobalPosition = globalCenter;
	}

	/// <summary>
	/// Shows the key while it is in the chest; only <paramref name="offered"/> makes it clickable, and
	/// <paramref name="picked"/> marks the local vote.
	/// </summary>
	public void Show(bool shown, bool offered, bool picked, string tip)
	{
		if (!GodotObject.IsInstanceValid(_holder)) return;
		_holder.Visible = shown;
		_gem.Disabled = !offered;
		_gem.Modulate = picked ? new Color(1.25f, 1.25f, 1.4f) : offered ? Colors.White : new Color(0.6f, 0.6f, 0.6f, 0.75f);
		if (!offered) _gem.Scale = Vector2.One;
		_buttonHolder.Visible = offered;
		_gem.TooltipText = tip;
		_button.TooltipText = tip;
	}

	private void Take()
	{
		if (_holder.Visible && !_gem.Disabled) _take();
	}
}
