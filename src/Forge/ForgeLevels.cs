using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>
/// Upgrade levels past +1 (spec "Upgrade levels"). Only cards that keep <see cref="CardModel"/>'s own
/// <c>MaxUpgradeLevel</c> are touched: curses (0) and StS1 Searing Blow (99) keep theirs.
/// <list type="bullet">
/// <item>In a Forge run, every fusable card's max is +9 (<see cref="Limit"/>); ordinary upgrades still stop at +1 (<see cref="Gate"/>).</item>
/// <item>Up to its growth cap (<see cref="Cap"/>) a card grows fully; past it only the row's <c>beyond</c> vars grow, and the
/// ladder adds Retain at +6, cost −1 at +7 and Innate at +9 (<see cref="ForgeRule.BonusAt"/>).</item>
/// <item>A saved or received card above +1 always loads, whatever the run state: the level itself proves a Forge run.</item>
/// <item>Each level past +1 replays the card's <c>OnUpgrade</c> but keeps its cost, star cost and keywords, so only the
/// var deltas repeat; a table row can replace the deltas or lower the cost per level.</item>
/// </list>
/// </summary>
public static class ForgeLevels
{
	[ThreadStatic] private static int _openScopes;
	[ThreadStatic] private static bool _probing;

	private static readonly AccessTools.FieldRef<CardModel, Player?> OwnerRef = AccessTools.FieldRefAccess<CardModel, Player?>("_owner");
	private static readonly AccessTools.FieldRef<CardModel, int> StarCostRef = AccessTools.FieldRefAccess<CardModel, int>("_baseStarCost");
	private static readonly AccessTools.FieldRef<CardEnergyCost, int> CostBaseRef = AccessTools.FieldRefAccess<CardEnergyCost, int>("_base");
	private static readonly AccessTools.FieldRef<CardModel, Action?> UpgradedRef = AccessTools.FieldRefAccess<CardModel, Action?>("Upgraded");
	private static readonly Action<CardModel> OnUpgrade = AccessTools.MethodDelegate<Action<CardModel>>(AccessTools.Method(typeof(CardModel), "OnUpgrade"));
	private static readonly MethodInfo LevelSetter = AccessTools.PropertySetter(typeof(CardModel), nameof(CardModel.CurrentUpgradeLevel));
	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	private static readonly Dictionary<Type, bool> BaseMax = new();
	private static readonly Dictionary<string, Probe> Probes = new(StringComparer.Ordinal);

	/// <summary>What a card's vanilla +1 does, read once from a fresh copy.</summary>
	private readonly record struct Probe(bool VarsChange, int CostAfter, int CostDelta);

	/// <summary>Lets levels past the gate be set: fusion, and loading a saved card above +1.</summary>
	public static IDisposable Open() => new Scope();

	private sealed class Scope : IDisposable
	{
		private bool _done;
		public Scope() => _openScopes++;
		public void Dispose()
		{
			if (_done) return;
			_done = true;
			_openScopes--;
		}
	}

	public static bool UsesBaseMax(CardModel card)
	{
		var type = card.GetType();
		lock (BaseMax)
		{
			if (!BaseMax.TryGetValue(type, out bool own))
			{
				own = AccessTools.PropertyGetter(type, nameof(CardModel.MaxUpgradeLevel)).DeclaringType == typeof(CardModel);
				BaseMax[type] = own;
			}
			return own;
		}
	}

	public static IRunState? CurrentRun => RunManager.Instance is { } manager ? State(manager) : null;

	public static bool InForgeRun(CardModel card) => SpoonForge.IsActive(OwnerRef(card)?.RunState ?? CurrentRun);

	/// <summary>The highest level fusion can reach: +9 for fusable cards, else +1.</summary>
	public static int Limit(CardModel card) => Fusable(card) ? ForgeRule.MaxLevel : 1;

	/// <summary>The growth cap: the table's cap, a cost-only card's cost cap, 1 when +1 changes no number, else 9.</summary>
	public static int Cap(CardModel card)
	{
		var row = ForgeCurves.For(card.Id.Entry);
		if (row is { Fusable: false }) return 1;
		if (row?.Cap is int cap) return cap;
		if (row?.PerLevel is { Count: > 0 }) return ForgeRule.MaxLevel;
		var probe = ProbeOf(card);
		if (row is { CostPerLevel: < 0 } && probe.CostAfter >= 0) return ForgeRule.CostCap(probe.CostAfter, row.CostPerLevel);
		if (probe.VarsChange) return ForgeRule.MaxLevel;
		return probe.CostDelta < 0 ? ForgeRule.CostCap(probe.CostAfter, probe.CostDelta) : 1;
	}

	public static bool Fusable(CardModel card) => UsesBaseMax(card) && card.Type is not (CardType.Curse or CardType.Status)
		&& ForgeCurves.For(card.Id.Entry) is not { Fusable: false };

	private static Probe ProbeOf(CardModel card)
	{
		string id = card.Id.Entry;
		lock (Probes)
		{
			if (Probes.TryGetValue(id, out var cached)) return cached;
		}
		Probe probe;
		_probing = true;
		try
		{
			var fresh = ModelDb.GetById<CardModel>(card.Id).ToMutable();
			var before = fresh.DynamicVars.Values.ToDictionary(v => v.Name, v => v.BaseValue);
			bool costsX = fresh.EnergyCost.CostsX;
			int cost = fresh.EnergyCost.Canonical;
			fresh.UpgradeInternal();
			bool vars = fresh.DynamicVars.Values.Any(v => !before.TryGetValue(v.Name, out var b) || b != v.BaseValue);
			int after = costsX ? -1 : CostBaseRef(fresh.EnergyCost);
			probe = new Probe(vars, after, costsX ? 0 : after - cost);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forge: could not probe {id}: {e.Message}");
			probe = new Probe(false, -1, 0);
		}
		finally
		{
			_probing = false;
		}
		lock (Probes) Probes[id] = probe;
		return probe;
	}

	/// <summary>Raises a card to <paramref name="level"/> through the same per-level step a reload replays.</summary>
	public static void RaiseTo(CardModel card, int level)
	{
		using (Open())
		{
			while (card.CurrentUpgradeLevel < level)
			{
				card.UpgradeInternal();
				card.FinalizeUpgradeInternal();
			}
		}
	}

	private static List<CardKeyword> Local(CardModel card) => card.GetKeywordsWithSources(KeywordSources.Local).ToList();

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel), MethodType.Getter)]
	public static class MaxPatch
	{
		public static void Postfix(CardModel __instance, ref int __result)
		{
			// The base getter only runs for classes that do not override it.
			if (__result != 1 || _probing) return;
			if (_openScopes > 0) __result = ForgeRule.MaxLevel;
			else if (InForgeRun(__instance)) __result = Math.Max(Limit(__instance), __instance.CurrentUpgradeLevel);
		}
	}

	/// <summary>Decision 3: only fusion goes past +1. Every ordinary upgrade path asks <c>IsUpgradable</c>.</summary>
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsUpgradable), MethodType.Getter)]
	public static class Gate
	{
		public static void Postfix(CardModel __instance, ref bool __result)
		{
			if (__result && _openScopes == 0 && __instance.CurrentUpgradeLevel >= 1 && UsesBaseMax(__instance)) __result = false;
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
	public static class LoadPatch
	{
		public static void Prefix(SerializableCard save, out IDisposable? __state)
		{
			__state = save.CurrentUpgradeLevel > 1 ? Open() : null;
		}

		public static Exception? Finalizer(Exception? __exception, IDisposable? __state)
		{
			__state?.Dispose();
			return __exception;
		}
	}

	/// <summary>Levels past +1: replay the +1 var deltas (or the table's), keep cost and keywords.</summary>
	[HarmonyPatch(typeof(CardModel), nameof(CardModel.UpgradeInternal))]
	public static class StepPatch
	{
		public static bool Prefix(CardModel __instance)
		{
			var card = __instance;
			if (card.CurrentUpgradeLevel < 1 || !UsesBaseMax(card)) return true;

			int level = card.CurrentUpgradeLevel + 1;
			LevelSetter.Invoke(card, [level]);
			var row = ForgeCurves.For(card.Id.Entry);
			bool pastCap = level > Cap(card);
			var vars = card.DynamicVars.Values.ToDictionary(v => v.Name, v => v.BaseValue);
			int cost = CostBaseRef(card.EnergyCost);
			bool costsX = card.EnergyCost.CostsX;
			int stars = StarCostRef(card);
			var keywords = Local(card);

			OnUpgrade(card);

			if (row?.PerLevel is { Count: > 0 } perLevel)
			{
				foreach (var v in card.DynamicVars.Values)
					if (vars.TryGetValue(v.Name, out var b)) v.BaseValue = b;
				foreach (var (name, delta) in perLevel)
					if (card.DynamicVars.TryGetValue(name, out var v)) v.UpgradeValueBy(delta);
			}
			if (pastCap)
			{
				// Past the growth cap only the safe vars keep growing.
				var keep = row?.Beyond ?? [];
				foreach (var v in card.DynamicVars.Values)
					if (!keep.Contains(v.Name) && vars.TryGetValue(v.Name, out var b)) v.BaseValue = b;
			}
			StarCostRef(card) = stars;
			var now = Local(card);
			foreach (var k in now.Where(k => !keywords.Contains(k))) card.RemoveKeyword(k);
			foreach (var k in keywords.Where(k => !now.Contains(k))) card.AddKeyword(k);
			if (!costsX)
			{
				// Cost-only cards keep lowering their cost (their cap stops at 0); every other card keeps its +1 cost.
				var probe = ProbeOf(card);
				bool costOnly = row?.PerLevel is not { Count: > 0 } && !probe.VarsChange && probe.CostDelta < 0;
				int next = pastCap ? cost
					: row is { CostPerLevel: < 0 } ? Math.Max(cost + row.CostPerLevel, 0)
					: costOnly ? Math.Max(CostBaseRef(card.EnergyCost), 0) : cost;
				if (ForgeRule.BonusAt(level) == ForgeBonus.CostDown) next = Math.Max(next - 1, 0);
				if (CostBaseRef(card.EnergyCost) != next)
				{
					if (next < cost) card.EnergyCost.UpgradeBy(next - CostBaseRef(card.EnergyCost));
					else card.EnergyCost.SetCustomBaseCost(next);
				}
			}
			var bonus = ForgeRule.BonusAt(level);
			if (bonus == ForgeBonus.Retain && !card.Keywords.Contains(CardKeyword.Retain)) card.AddKeyword(CardKeyword.Retain);
			if (bonus == ForgeBonus.Innate && !card.Keywords.Contains(CardKeyword.Innate)) card.AddKeyword(CardKeyword.Innate);
			card.DynamicVars.RecalculateForUpgradeOrEnchant();
			UpgradedRef(card)?.Invoke();
			return false;
		}
	}
}
