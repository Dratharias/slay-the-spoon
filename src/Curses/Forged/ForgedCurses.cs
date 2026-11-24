using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Forge;

namespace SlaySpoon.Curses.Forged;

/// <summary>
/// Forged curses (spec docs/superpowers/specs/2026-11-23-forged-curses-design.md): with The Forge and Cursed Deck both
/// active, curses fuse up to +9. Either mode alone keeps its own rules.
/// </summary>
public static class ForgedCurses
{
	/// <summary>Set while a curse saved above +1 loads: only the combo makes one, so the save proves the combo.</summary>
	[ThreadStatic] internal static int Loading;

	private static readonly AccessTools.FieldRef<CardModel, Player?> OwnerRef = AccessTools.FieldRefAccess<CardModel, Player?>("_owner");

	public static bool IsActive(IRunState? run) => SpoonForge.IsActive(run) && SpoonCursedDeck.IsActive(run);

	/// <summary>A curse in a Forge + Cursed Deck run (the owner's run, else the current one).</summary>
	public static bool Applies(CardModel card) => card.Type == CardType.Curse
		&& (Loading > 0 || IsActive(OwnerRef(card)?.RunState ?? ForgeLevels.CurrentRun));

	/// <summary>
	/// Fusion and Duplicate: a forged curse the game lets its owner lose (Eternal curses such as Ascender's Bane never fuse),
	/// plus the Witching Hour, which climbs to +9 while staying Eternal (owner, 2026-11-24).
	/// </summary>
	public static bool Fusable(CardModel card) => Applies(card) && (card.IsRemovable || card is SpoonWitchingHour);
}
