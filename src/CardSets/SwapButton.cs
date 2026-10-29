using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using SlaySpoon.Cards;

namespace SlaySpoon.CardSets;

/// <summary>The card reward screen's alternative button, reused as a "switch version" button under one card.</summary>
public static class SwapButton
{
	private static readonly AccessTools.FieldRef<NCardRewardAlternativeButton, MegaLabel> ButtonLabel =
		AccessTools.FieldRefAccess<NCardRewardAlternativeButton, MegaLabel>("_label");

	private static readonly AccessTools.FieldRef<NCardRewardAlternativeButton, string?> ButtonText =
		AccessTools.FieldRefAccess<NCardRewardAlternativeButton, string?>("_optionName");

	/// <summary>
	/// Adds a button centered on <paramref name="center"/> in <paramref name="parent"/>'s coordinates. The button sits
	/// in a zero-size holder: its center anchors then resolve to the holder's origin whatever the parent's size, and
	/// the holder carries the scale (the button resets its own scale on hover).
	/// </summary>
	public static NCardRewardAlternativeButton Add(Control parent, Vector2 center, float scale, CardModel shown,
		Action onPressed)
	{
		var holder = new Control { Position = center, Scale = Vector2.One * scale, MouseFilter = Control.MouseFilterEnum.Ignore };
		NCardRewardAlternativeButton button = NCardRewardAlternativeButton.Create(Label(shown), MegaInput.viewDrawPile);
		holder.AddChild(button);
		parent.AddChildSafely(holder);
		button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => onPressed()));
		return button;
	}

	/// <summary>Shows or hides the button with its holder.</summary>
	public static void SetVisible(NCardRewardAlternativeButton button, bool visible)
	{
		if (button.GetParent() is Control holder)
		{
			holder.Visible = visible;
		}
	}

	/// <summary>Relabels the button for the version now shown (the label names the version it switches to).</summary>
	public static void Relabel(NCardRewardAlternativeButton button, CardModel shown)
	{
		string text = Label(shown);
		ButtonText(button) = text;
		if (button.IsNodeReady())
		{
			ButtonLabel(button).SetTextAutoSize(text);
		}
	}

	private static string Label(CardModel shown)
	{
		return new LocString("card_reward_ui",
			shown is Sts1Card ? "SPOON_SWAP_SHORT_TO_STS2.name" : "SPOON_SWAP_SHORT_TO_STS1.name").GetFormattedText();
	}

	/// <summary>Runs <paramref name="act"/> once <paramref name="node"/> is ready (now if it already is).</summary>
	public static void WhenReady(Node node, Action act)
	{
		if (node.IsNodeReady())
		{
			act();
		}
		else
		{
			node.Connect(Node.SignalName.Ready, Callable.From(act), (uint)GodotObject.ConnectFlags.OneShot);
		}
	}
}
