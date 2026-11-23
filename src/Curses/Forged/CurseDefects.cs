using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Core;

namespace SlaySpoon.Curses.Forged;

/// <summary>
/// A forged curse's defects (<see cref="CurseDefect"/> flags), stored on the card through a BaseLib saved field: it rides
/// in the card's saved properties, so it survives save/reload and reaches every co-op client with the card.
/// </summary>
public static class CurseDefects
{
	// BaseLib is a runtime dependency only (the build has no copy of it), so its field type is reached by reflection.
	private static Func<CardModel, string?>? _get;
	private static Action<CardModel, string?>? _set;

	/// <summary>Called from the mod's initializer: builds a BaseLib <c>SavedSpireField&lt;CardModel, string&gt;</c> (it registers itself). A string, null by default, because BaseLib saves every non-null value: an int field put a 0 on every card of every run.</summary>
	public static void Register()
	{
		try
		{
			var open = Type.GetType("BaseLib.Utils.SavedSpireField`2, BaseLib", throwOnError: true)!;
			var type = open.MakeGenericType(typeof(CardModel), typeof(string));
			var ctor = type.GetConstructor([typeof(Func<string>), typeof(string)])
				?? throw new MissingMethodException(type.FullName, ".ctor(Func<string>, string)");
			object field = ctor.Invoke([(Func<string?>)(() => null), "SpoonCurseDefects"]);
			type.GetMethod("CopyOnClone")!.Invoke(field, [null]);
			var get = type.GetMethod("Get", [typeof(CardModel)])!;
			var set = type.GetMethod("Set", [typeof(CardModel), typeof(string)])!;
			_get = card => get.Invoke(field, [card]) as string;
			_set = (card, v) => set.Invoke(field, [card, v]);
			SpoonLog.Info("forged curses: defect field registered");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forged curses: could not register the defect field, forged curses get no defects: {e.Message}");
		}
	}

	public static CurseDefect Get(CardModel card) => _get is { } g ? ForgedCurseRule.DecodeDefects(g(card)) : CurseDefect.None;

	public static void Set(CardModel card, CurseDefect defects) => _set?.Invoke(card, ForgedCurseRule.EncodeDefects(defects));

	/// <summary>
	/// After a fusion that left a forged curse: keep the kept copy's defects, add the sacrificed copy's, draw the rest on
	/// the mod's own stream (fusion is a synced rest site action, so every client draws the same).
	/// </summary>
	public static void AfterFusion(CardModel result, CurseDefect kept, CurseDefect sacrificed, int deckIndex)
	{
		if (!ForgedCurses.Applies(result) || result.Owner?.RunState is not { } run) return;
		var rng = new Rng(run.Rng.Seed, ForgedCurseRule.DefectStream(run.TotalFloor, deckIndex, 0));
		var merged = ForgedCurseRule.Merge(kept, sacrificed, result.CurrentUpgradeLevel, n => rng.NextInt(n));
		Set(result, merged);
		SpoonLog.Info($"forge: curse defects {kept} + {sacrificed} -> {merged} ({result.Id.Entry}+{result.CurrentUpgradeLevel})");
	}

	/// <summary>Cards the owner played this turn before this one (the curse itself is not started yet when checked).</summary>
	public static int OtherCardsPlayedThisTurn(CardModel card) =>
		card.CombatState is { } combat && card.Owner is { } owner
			? CombatManager.Instance.History.CardPlaysStarted.Count(e => e.HappenedThisTurn(combat) && e.CardPlay.Player == owner && e.CardPlay.Card != card)
			: 0;

	/// <summary>Whether a defect forbids playing the curse right now (even with the Unholy Grimoire).</summary>
	public static bool BlockedNow(CardModel card)
	{
		if (card.IsCanonical || card.Type != CardType.Curse || card.CombatState is not { } combat) return false;
		var defects = Get(card);
		if (defects == CurseDefect.None || !ForgedCurses.Applies(card)) return false;
		return ForgedCurseRule.BlocksPlay(defects, combat.RoundNumber, OtherCardsPlayedThisTurn(card));
	}
}
