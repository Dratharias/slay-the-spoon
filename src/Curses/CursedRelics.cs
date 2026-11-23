using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>The cursed relics the Cursed Deck mode hands out: the game's 6 and the mod's 4.</summary>
public static class CursedRelics
{
	/// <summary>The vanilla cursed relics named in the spec, minus Neow's Bones: its nested reward set and its Niche curse draw desync co-op mid-run (bot queue 7 line 5, 1.11.1).</summary>
	public static IReadOnlyList<RelicModel> Vanilla() => new RelicModel[]
	{
		ModelDb.Relic<CallingBell>(),
		ModelDb.Relic<DistinguishedCape>(),
		ModelDb.Relic<CursedPearl>(),
		ModelDb.Relic<BloodSoakedRose>(),
		ModelDb.Relic<PreservedFog>(),
		ModelDb.Relic<DarkstonePeriapt>(),
	};

	/// <summary>The four relics the mod adds, kept out of every ordinary grab bag (Event rarity).</summary>
	public static IReadOnlyList<RelicModel> Custom() => new RelicModel[]
	{
		ModelDb.Relic<SpoonBloodPact>(),
		ModelDb.Relic<SpoonPinDoll>(),
		ModelDb.Relic<SpoonBlackCenser>(),
		ModelDb.Relic<SpoonReliquary>(),
	};

	public static IReadOnlyList<RelicModel> Pool => Vanilla().Concat(Custom()).ToList();

	/// <summary>
	/// Per-run draw state, never saved: a reload replays the rolls, and can shift them only within the reloaded floor.
	/// Draws are counted per bag key (player slot, -1 for the shared bag) so clients agree; relics drawn but not yet
	/// obtained are reserved until the floor changes.
	/// </summary>
	private sealed class DrawState
	{
		public int Floor = -1;
		public readonly Dictionary<int, int> Draws = new();
		public readonly Dictionary<int, HashSet<string>> Reserved = new();
	}

	private static readonly ConditionalWeakTable<IRunState, DrawState> States = new();

	/// <summary>
	/// The cursed draw: Cursed Deck curses chest, elite and merchant pulls 2 in 3; Overcursed curses merchant and event pulls
	/// 5 in 6 (<see cref="OvercursedRule"/>). Own seeded stream, vanilla RNG untouched; the relic is one nobody concerned holds.
	/// Returns null to let the vanilla pull go on.
	/// </summary>
	public static RelicModel? TryDraw(RelicGrabBag bag, Func<RelicModel, bool> filter, IRunState run, CursedRelicSource source)
	{
		bool overcursed = SpoonOvercursed.IsActive(run);
		if (!OvercursedRule.CursedSource(source, SpoonCursedDeck.IsActive(run), overcursed)) return null;
		bool fiveInSix = overcursed && source is CursedRelicSource.Merchant or CursedRelicSource.Event;
		var concerned = run.Players.Where(p => ReferenceEquals(p.RelicGrabBag, bag)).ToList();
		int key = concerned.Count == 1 ? run.GetPlayerSlotIndex(concerned[0]) : -1;
		if (concerned.Count == 0 && ReferenceEquals(run.SharedRelicGrabBag, bag)) concerned = run.Players.ToList();
		if (concerned.Count == 0) return null;

		DrawState state = States.GetOrCreateValue(run);
		int floor = run.TotalFloor;
		if (state.Floor != floor)
		{
			state.Floor = floor;
			state.Draws.Clear();
			state.Reserved.Clear();
		}
		int draw = state.Draws.GetValueOrDefault(key);
		state.Draws[key] = draw + 1;

		HashSet<string> excluded = concerned.SelectMany(p => p.Relics).Select(r => r.Id.Entry).ToHashSet();
		foreach (var (k, ids) in state.Reserved)
		{
			if (key == -1 || k == key || k == -1) excluded.UnionWith(ids);
		}
		Rng rng = new(run.Rng.Seed, CurseRule.RelicStream(floor, key, draw));
		if (!OvercursedRule.CursedRelicRoll(rng.NextInt(int.MaxValue), fiveInSix)) return null;
		List<RelicModel> left = Pool.Where(r => !excluded.Contains(r.Id.Entry) && r.IsAllowed(run) && filter(r)).ToList();
		string? id = CurseRule.PickCursedRelic(left.Select(r => r.Id.Entry).ToList(), new HashSet<string>(), rng.NextInt(int.MaxValue));
		if (id == null) return null;
		if (!state.Reserved.TryGetValue(key, out var set)) state.Reserved[key] = set = new HashSet<string>();
		set.Add(id);
		return left.First(r => r.Id.Entry == id);
	}
}

/// <summary>
/// Cursed Deck: the cursed draw applies inside the vanilla methods that generate a chest, elite or merchant relic; Overcursed
/// also curses random relic pulls in an ordinary "?" event room (<see cref="CursedScope.For"/>).
/// The scope is a thread-static depth counter (these methods are synchronous), so the rest-site Dig, Pael's Wing, Toy Box,
/// Large Capsule and (without Overcursed) event grants stay vanilla.
/// </summary>
public static class CursedScope
{
	[ThreadStatic] private static int _depth;
	[ThreadStatic] private static CursedRelicSource _source;

	public static bool Active => _depth > 0;

	public static CursedRelicSource Source => _source;

	public static void Enter(CursedRelicSource source)
	{
		if (_depth == 0) _source = source;
		_depth++;
	}

	public static void Exit() { if (_depth > 0) _depth--; }

	/// <summary>
	/// The pull's source: the open scope's, else an ordinary "?" event (Overcursed: the event's random relics), else none.
	/// Ancients and the Architect never count.
	/// </summary>
	public static CursedRelicSource? For(IRunState run)
	{
		if (Active) return _source;
		return run.CurrentRoom is EventRoom { CanonicalEvent: not (AncientEventModel or TheArchitect) } ? CursedRelicSource.Event : null;
	}
}

/// <summary>Chest: the treasure room's relic generation.</summary>
[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.BeginRelicPicking))]
public static class CursedChestScopePatch
{
	public static void Prefix() => CursedScope.Enter(CursedRelicSource.Chest);

	public static void Finalizer() => CursedScope.Exit();
}

/// <summary>Elite: the relic reward is populated while the current room is an elite fight.</summary>
[HarmonyPatch(typeof(RelicReward), nameof(RelicReward.Populate))]
public static class CursedEliteScopePatch
{
	public static void Prefix(RelicReward __instance, out bool __state)
	{
		__state = __instance.Player.RunState.CurrentRoom is { RoomType: RoomType.Elite };
		if (__state) CursedScope.Enter(CursedRelicSource.Elite);
	}

	public static void Finalizer(bool __state)
	{
		if (__state) CursedScope.Exit();
	}
}

/// <summary>Merchant: filling a relic slot (the purchase restock goes through the same method).</summary>
[HarmonyPatch(typeof(MerchantRelicEntry), "FillSlot")]
public static class CursedMerchantScopePatch
{
	public static void Prefix() => CursedScope.Enter(CursedRelicSource.Merchant);

	public static void Finalizer() => CursedScope.Exit();
}

/// <summary>Relic pulls from the front of a grab bag (chest, elite, event) may be cursed (<see cref="CursedRelics.TryDraw"/>).</summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromFront), typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState))]
public static class CursedRelicDrawPatch
{
	public static bool Prefix(RelicGrabBag __instance, Func<RelicModel, bool> filter, IRunState runState, ref RelicModel? __result)
	{
		if (CursedScope.For(runState) is not { } source || CursedRelics.TryDraw(__instance, filter, runState, source) is not { } relic) return true;
		__result = relic;
		return false;
	}
}

/// <summary>The merchant's relic pulls (from the back of a grab bag) may be cursed too.</summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromBack), typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState))]
public static class CursedRelicShopDrawPatch
{
	public static bool Prefix(RelicGrabBag __instance, Func<RelicModel, bool> filter, IRunState runState, ref RelicModel? __result)
	{
		if (CursedScope.For(runState) is not { } source || CursedRelics.TryDraw(__instance, filter, runState, source) is not { } relic) return true;
		__result = relic;
		return false;
	}
}

/// <summary>Event and Ancient relics have no merchant price (999999999): a cursed relic in a shop costs the Uncommon price.</summary>
[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.MerchantCost), MethodType.Getter)]
public static class CursedMerchantPricePatch
{
	public const int UncommonPrice = 225;

	public static void Postfix(RelicModel __instance, ref int __result)
	{
		if (__result < 999999999) return;
		if (!CursedRelics.Pool.Any(r => r.Id == __instance.Id)) return;
		if (__instance.Owner is { } owner && !SpoonCursedDeck.IsActive(owner.RunState) && !SpoonOvercursed.IsActive(owner.RunState)) return;
		__result = UncommonPrice;
	}
}

/// <summary>The four relics need a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class CursedRelicsPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Concat(CursedRelics.Custom()).ToList();
	}
}
