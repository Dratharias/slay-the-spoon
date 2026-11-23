using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Cards;
using SlaySpoon.Core;
using SlaySpoon.Curses.Forged;
using SlaySpoon.Forge;

namespace SlaySpoon.Curses;

/// <summary>
/// Cursed Deck (spec "Upgraded curses"): the game caps every curse at +0; in the mode a curse goes to +1. The numbers
/// come from the curse table (<see cref="CurseRow"/>): the card var of a row with <c>SelfVar</c> (and Pain's and Parasite's
/// own vars) goes from <c>Amount</c> to <c>AmountPlus</c>; Regret, Normality and Guilty have player-side patches below.
/// Outside the mode every patch returns early.
/// </summary>
public static class CurseUpgrade
{
	[ThreadStatic] private static int _loading;

	private static readonly AccessTools.FieldRef<CardModel, Player?> OwnerRef = AccessTools.FieldRefAccess<CardModel, Player?>("_owner");

	public static bool IsCurse(CardModel card) => card.Type == CardType.Curse;

	/// <summary>Cursed Deck run (or a saved +1 curse being loaded, which proves one), checked without needing an owner.</summary>
	public static bool Active(CardModel card) =>
		_loading > 0 || ForgedCurses.Loading > 0 || SpoonCursedDeck.IsActive(OwnerRef(card)?.RunState ?? ForgeLevels.CurrentRun);

	/// <summary>A mutable curse of the mode that is upgraded and has a table row.</summary>
	public static CurseRow? UpgradedRow(CardModel card) =>
		!card.IsCanonical && IsCurse(card) && card.IsUpgraded && Active(card) ? CurseTable.For(card.Id.Entry) : null;

	/// <summary>The card var the row raises on upgrade: the row's own, or Pain's and Parasite's.</summary>
	public static string? VarFor(CurseRow row) => row.SelfVar ?? row.Id switch
	{
		"STS1_PAIN" => "HpLoss",
		"STS1_PARASITE" => "MaxHp",
		_ => null,
	};

	/// <summary>
	/// Every concrete game card class that declares its own MaxUpgradeLevel (every vanilla curse sets 0; so do a few
	/// non-curses, which <see cref="MaxLevelPatch"/> leaves alone by checking the card type), plus the mod's curse base.
	/// </summary>
	public static IEnumerable<MethodBase> OverridingGetters()
	{
		foreach (var type in typeof(CardModel).Assembly.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(CardModel))) continue;
			var getter = AccessTools.DeclaredPropertyGetter(type, nameof(CardModel.MaxUpgradeLevel));
			if (getter != null) yield return getter;
		}
		yield return AccessTools.DeclaredPropertyGetter(typeof(Sts1CurseCard), nameof(CardModel.MaxUpgradeLevel));
	}

	[HarmonyPatch]
	public static class MaxLevelPatch
	{
		public static IEnumerable<MethodBase> TargetMethods() => OverridingGetters();

		/// <summary>
		/// In the mode a curse goes to +1. With The Forge too, fusion and reloads (an open Forge scope) reach +9, and a forged
		/// curse's max follows its level so its title reads "+N" and ordinary upgrades still stop.
		/// </summary>
		public static void Postfix(CardModel __instance, ref int __result)
		{
			if (__result != 0 || !IsCurse(__instance) || !Active(__instance)) return;
			bool combo = ForgedCurses.Applies(__instance);
			__result = Math.Max(ForgedCurseRule.MaxLevelFor(combo, ForgeLevels.InScope, __result), combo ? __instance.CurrentUpgradeLevel : 0);
		}
	}

	/// <summary>
	/// A saved upgraded curse replays its upgrades on load, before the run state exists: let it through. A level above +1
	/// proves Forge + Cursed Deck (the Forge's own load patch opens the scope); it reloads to at most +9.
	/// </summary>
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
	public static class LoadPatch
	{
		public static void Prefix(SerializableCard save, out int __state)
		{
			__state = 0;
			if (save.CurrentUpgradeLevel < 1 || CurseTable.For(save.Id!.Entry) == null) return;
			_loading++;
			__state = 1;
			if (save.CurrentUpgradeLevel <= 1) return;
			save.CurrentUpgradeLevel = ForgedCurseRule.LoadLevel(save.CurrentUpgradeLevel, comboActive: true);
			ForgedCurses.Loading++;
			__state = 2;
		}

		public static Exception? Finalizer(Exception? __exception, int __state)
		{
			if (__state >= 1) _loading--;
			if (__state == 2) ForgedCurses.Loading--;
			return __exception;
		}
	}

	/// <summary>
	/// The row's card var follows the level: each level repeats the +0 → +1 step (Pain+ 2, Parasite+ 5, Bad Luck+ 20, Bad
	/// Luck+9 76...); Normality's and Guilty's limits shift. Set from the canonical value, so replaying it is harmless.
	/// </summary>
	[HarmonyPatch(typeof(CardModel), "OnUpgrade")]
	public static class VarPatch
	{
		public static void Postfix(CardModel __instance)
		{
			if (UpgradedRow(__instance) is not { } row) return;
			int level = __instance.CurrentUpgradeLevel;
			if (VarFor(row) is { } name && __instance.DynamicVars.TryGetValue(name, out var v) && row.AmountPlus != row.Amount)
			{
				decimal start = __instance.CanonicalInstance.DynamicVars.TryGetValue(name, out var cv) ? cv.BaseValue : row.Amount;
				SetTo(v, start + (row.AmountPlus - row.Amount) * level);
			}
			if (__instance is Normality n && row.Self != null)
			{
				int limit = ForgedCurseRule.NormalityLimitAt(level);
				if (n.DynamicVars.TryGetValue("CalculationBase", out var b)) SetTo(b, limit);
				NormalityPatch.SetMultiplier(n, limit);
			}
			if (__instance is Guilty g && row.Self != null && g.DynamicVars.TryGetValue("Combats", out var c))
				c.BaseValue = ForgedCurseRule.GuiltyCombatsAt(level) - g.CombatsSeen;
		}

		private static void SetTo(DynamicVar v, decimal target)
		{
			if (target != v.BaseValue) v.UpgradeValueBy(target - v.BaseValue);
		}
	}

	public static class NormalityPatch
	{
		private static readonly FieldInfo Calc = AccessTools.Field(typeof(CalculatedVar), "_multiplierCalc");
		private static readonly Func<Normality, int> Played =
			AccessTools.MethodDelegate<Func<Normality, int>>(AccessTools.PropertyGetter(typeof(Normality), "CardsPlayedThisTurn"));

		public static int PlayedThisTurn(Normality card) => Played(card);

		/// <summary>The "cards left" counter is capped at the limit; the vanilla lambda hardcodes 3.</summary>
		public static void SetMultiplier(Normality card, int limit)
		{
			if (card.DynamicVars.TryGetValue("CalculatedCards", out var v) && v is CalculatedVar calc)
				Calc.SetValue(calc, (Func<CardModel, Creature?, decimal>)((c, _) => Math.Min(limit, PlayedThisTurn((Normality)c))));
		}

		public static int Limit(Normality card) => UpgradedRow(card)?.Self != null ? ForgedCurseRule.NormalityLimitAt(card.CurrentUpgradeLevel) : -1;
	}

	/// <summary>Normality+ allows the table's upgraded number of cards (the private getter it replaces can be inlined, so the callers are patched).</summary>
	[HarmonyPatch(typeof(Normality), nameof(Normality.ShouldPlay))]
	public static class NormalityShouldPlayPatch
	{
		public static void Postfix(Normality __instance, CardModel card, ref bool __result)
		{
			int limit = NormalityPatch.Limit(__instance);
			if (limit < 0 || card.Owner != __instance.Owner || __instance.Pile?.Type != PileType.Hand) return;
			__result = NormalityPatch.PlayedThisTurn(__instance) < limit;
		}
	}

	[HarmonyPatch(typeof(Normality), "ShouldGlowRedInternal", MethodType.Getter)]
	public static class NormalityGlowPatch
	{
		public static void Postfix(Normality __instance, ref bool __result)
		{
			int limit = NormalityPatch.Limit(__instance);
			if (limit >= 0) __result = NormalityPatch.PlayedThisTurn(__instance) >= limit;
		}
	}

	/// <summary>Guilty+ shows and leaves after the table's upgraded number of combats.</summary>
	[HarmonyPatch(typeof(Guilty), nameof(Guilty.CombatsSeen), MethodType.Setter)]
	public static class GuiltyCountPatch
	{
		public static void Postfix(Guilty __instance)
		{
			if (UpgradedRow(__instance)?.Self != null && __instance.DynamicVars.TryGetValue("Combats", out var v))
				v.BaseValue = ForgedCurseRule.GuiltyCombatsAt(__instance.CurrentUpgradeLevel) - __instance.CombatsSeen;
		}
	}

	[HarmonyPatch(typeof(Guilty), nameof(Guilty.AfterCombatEnd))]
	public static class GuiltyLeavePatch
	{
		public static void Postfix(Guilty __instance, CombatRoom _, ref Task __result) =>
			__result = Finish(__result, __instance);

		private static async Task Finish(Task original, Guilty card)
		{
			await original;
			if (UpgradedRow(card)?.Self != null && card.CombatsSeen >= ForgedCurseRule.GuiltyCombatsAt(card.CurrentUpgradeLevel)
				&& card.Pile?.Type == PileType.Deck)
				await MegaCrit.Sts2.Core.Commands.CardPileCmd.RemoveFromDeck(card);
		}
	}

	private static readonly AccessTools.FieldRef<Regret, int> CardsInHand = AccessTools.FieldRefAccess<Regret, int>("_cardsInHand");

	/// <summary>Regret+: the turn-end loss counts the level's HP per card in hand.</summary>
	[HarmonyPatch(typeof(Regret), nameof(Regret.BeforeSideTurnEnd))]
	public static class RegretPatch
	{
		public static void Postfix(Regret __instance, IEnumerable<Creature> participants)
		{
			if (UpgradedRow(__instance) is not { } row || __instance.Pile?.Type != PileType.Hand) return;
			if (!participants.Contains(__instance.Owner.Creature)) return;
			CardsInHand(__instance) = __instance.Pile.Cards.Count * ForgedCurseRule.AmountAt(row, __instance.CurrentUpgradeLevel);
		}
	}

	/// <summary>Regret+ and Normality+ have no var for their number: an own text gets it as an argument.</summary>
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.Description), MethodType.Getter)]
	public static class DescriptionPatch
	{
		public static void Postfix(CardModel __instance, ref LocString __result)
		{
			if (__instance is not (Regret or Normality) || UpgradedRow(__instance) is not { } row) return;
			var text = new LocString("cards", "SPOON_CURSED_" + row.Id + "_PLUS.description");
			int level = __instance.CurrentUpgradeLevel;
			text.Add("Amount", __instance is Normality ? ForgedCurseRule.NormalityLimitAt(level) : ForgedCurseRule.AmountAt(row, level));
			__result = text;
		}
	}
}
