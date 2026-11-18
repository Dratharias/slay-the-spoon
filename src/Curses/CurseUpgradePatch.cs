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
		_loading > 0 || SpoonCursedDeck.IsActive(OwnerRef(card)?.RunState ?? ForgeLevels.CurrentRun);

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

		/// <summary>In the mode a curse goes to +1 (never fused: the Forge only touches classes that keep the base getter).</summary>
		public static void Postfix(CardModel __instance, ref int __result)
		{
			if (__result == 0 && IsCurse(__instance) && Active(__instance)) __result = 1;
		}
	}

	/// <summary>A saved +1 curse replays its upgrade on load, before the run state exists: let it through.</summary>
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
	public static class LoadPatch
	{
		public static void Prefix(SerializableCard save, out bool __state)
		{
			__state = save.CurrentUpgradeLevel == 1 && CurseTable.For(save.Id!.Entry) != null;
			if (__state) _loading++;
		}

		public static Exception? Finalizer(Exception? __exception, bool __state)
		{
			if (__state) _loading--;
			return __exception;
		}
	}

	/// <summary>The row's card var goes from Amount to AmountPlus (Pain+ 2, Parasite+ 5, Bad Luck+ 20...), Normality's and Guilty's limits shift.</summary>
	[HarmonyPatch(typeof(CardModel), "OnUpgrade")]
	public static class VarPatch
	{
		public static void Postfix(CardModel __instance)
		{
			if (UpgradedRow(__instance) is not { } row) return;
			if (VarFor(row) is { } name && __instance.DynamicVars.TryGetValue(name, out var v) && row.AmountPlus != row.Amount)
				v.UpgradeValueBy(row.AmountPlus - row.Amount);
			if (__instance is Normality n && row.Self is { } limits)
			{
				if (n.DynamicVars.TryGetValue("CalculationBase", out var b)) b.UpgradeValueBy(limits[1] - limits[0]);
				NormalityPatch.SetMultiplier(n, limits[1]);
			}
			if (__instance is Guilty g && row.Self is { } gl && g.DynamicVars.TryGetValue("Combats", out var c))
				c.BaseValue = gl[1] - g.CombatsSeen;
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

		public static int Limit(Normality card) => UpgradedRow(card)?.Self is { } s ? s[1] : -1;
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
			if (UpgradedRow(__instance)?.Self is { } s && __instance.DynamicVars.TryGetValue("Combats", out var v))
				v.BaseValue = s[1] - __instance.CombatsSeen;
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
			if (UpgradedRow(card)?.Self is { } s && card.CombatsSeen >= s[1] && card.Pile?.Type == PileType.Deck)
				await MegaCrit.Sts2.Core.Commands.CardPileCmd.RemoveFromDeck(card);
		}
	}

	private static readonly AccessTools.FieldRef<Regret, int> CardsInHand = AccessTools.FieldRefAccess<Regret, int>("_cardsInHand");

	/// <summary>Regret+: the turn-end loss counts the table's upgraded HP per card in hand.</summary>
	[HarmonyPatch(typeof(Regret), nameof(Regret.BeforeSideTurnEnd))]
	public static class RegretPatch
	{
		public static void Postfix(Regret __instance, IEnumerable<Creature> participants)
		{
			if (UpgradedRow(__instance) is not { } row || __instance.Pile?.Type != PileType.Hand) return;
			if (!participants.Contains(__instance.Owner.Creature)) return;
			CardsInHand(__instance) = __instance.Pile.Cards.Count * row.AmountPlus;
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
			text.Add("Amount", __instance is Normality ? row.Self![1] : row.AmountPlus);
			__result = text;
		}
	}
}
