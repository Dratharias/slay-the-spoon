using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// The Sapphire key sitting in the opened chest, right of the relics left, for the player it is offered to: the gem itself
/// is clickable, with the game's reward-alternative button under it for the controller.
/// </summary>
internal sealed class SapphireKeyView
{
	private const float GemSize = 132f;
	// From the room's center when no relic holder is given.
	private static readonly Vector2 Offset = new(330f, -70f);
	// From the right edge of the relic it sits beside.
	private const float Gap = 120f;

	private readonly Control _room;
	private readonly Control _holder;
	private readonly TextureButton _gem;
	private readonly NCardRewardAlternativeButton _button;
	private readonly Func<bool> _take;
	private bool _taken;

	private SapphireKeyView(Control room, Func<bool> take)
	{
		_take = take;
		_room = room;
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
			new LocString("merchant_room", "SPOON_SAPPHIRE_KEY").GetFormattedText(), MegaInput.viewDrawPile);
		buttonHolder.AddChild(_button);
		_holder.AddChild(buttonHolder);
		_button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => Take()));

		room.AddChild(_holder);

		// A slow bob, like a relic waiting in the chest.
		var bob = _gem.CreateTween().SetLoops();
		bob.TweenProperty(_gem, "position:y", -GemSize / 2f - 8f, 1.2).SetTrans(Tween.TransitionType.Sine);
		bob.TweenProperty(_gem, "position:y", -GemSize / 2f + 8f, 1.2).SetTrans(Tween.TransitionType.Sine);
	}

	public static SapphireKeyView Add(Control room, Func<bool> take) => new(room, take);

	/// <summary>Shows the key while it is on offer, right of <paramref name="beside"/> (a relic holder) when given.</summary>
	public void Show(bool shown, string tip, Control? beside)
	{
		if (!GodotObject.IsInstanceValid(_holder)) return;
		_holder.Visible = shown && !_taken;
		if (_holder.Visible)
		{
			if (beside != null && GodotObject.IsInstanceValid(beside))
			{
				var rect = beside.GetGlobalRect();
				_holder.GlobalPosition = new Vector2(rect.End.X + Gap, rect.GetCenter().Y);
			}
			else _holder.Position = _room.Size / 2f + Offset;
		}
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
