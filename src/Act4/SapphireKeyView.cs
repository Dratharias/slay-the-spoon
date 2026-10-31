using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// The Sapphire key sitting in the opened chest, right of the relics: the gem itself is clickable, with the game's
/// reward-alternative button under it for the controller.
/// </summary>
internal sealed class SapphireKeyView
{
	private const float GemSize = 132f;
	// From the treasure room's center: right of the single-player relic holder (-102..102) and the chest.
	private static readonly Vector2 Offset = new(330f, -70f);

	private readonly Control _holder;
	private readonly TextureButton _gem;
	private readonly NCardRewardAlternativeButton _button;
	private readonly Func<bool> _take;
	private bool _taken;

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
		_gem.MouseEntered += () => _gem.Scale = Vector2.One * 1.12f;
		_gem.MouseExited += () => _gem.Scale = Vector2.One;
		_gem.Pressed += Take;
		_holder.AddChild(_gem);

		var buttonHolder = new Control { Position = new Vector2(0, GemSize * 0.75f), Scale = Vector2.One * 0.85f,
			MouseFilter = Control.MouseFilterEnum.Ignore };
		_button = NCardRewardAlternativeButton.Create(
			new LocString("merchant_room", "SPOON_SAPPHIRE_SWAP").GetFormattedText(), MegaInput.viewDrawPile);
		buttonHolder.AddChild(_button);
		_holder.AddChild(buttonHolder);
		_button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => Take()));

		room.AddChild(_holder);
		void Place() => _holder.Position = room.Size / 2f + Offset;
		room.Resized += Place;
		Place();

		// A slow bob, like a relic waiting in the chest.
		var bob = _gem.CreateTween().SetLoops();
		bob.TweenProperty(_gem, "position:y", -GemSize / 2f - 8f, 1.2).SetTrans(Tween.TransitionType.Sine);
		bob.TweenProperty(_gem, "position:y", -GemSize / 2f + 8f, 1.2).SetTrans(Tween.TransitionType.Sine);
	}

	public static SapphireKeyView Add(Control room, Func<bool> take) => new(room, take);

	/// <summary>Shows the key while a swap is on offer; <paramref name="tip"/> names the relic it gives back.</summary>
	public void Show(bool shown, string tip)
	{
		_holder.Visible = shown && !_taken;
		_gem.TooltipText = tip;
		_button.TooltipText = tip;
	}

	private void Take()
	{
		if (_taken || !_holder.Visible || !_take()) return;
		_taken = true;
		_holder.Visible = false;
	}
}
