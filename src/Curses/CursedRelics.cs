using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>The cursed relics the Cursed Deck mode hands out: the game's 7 and the mod's 4.</summary>
public static class CursedRelics
{
	/// <summary>The vanilla cursed relics named in the spec.</summary>
	public static IReadOnlyList<RelicModel> Vanilla() => new RelicModel[]
	{
		ModelDb.Relic<CallingBell>(),
		ModelDb.Relic<DistinguishedCape>(),
		ModelDb.Relic<NeowsBones>(),
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
	/// The cursed draw: in a Cursed Deck run, 2 pulls in 3 (own seeded stream, vanilla RNG untouched) give a cursed relic
	/// nobody concerned holds. Returns null to let the vanilla pull go on.
	/// </summary>
	public static RelicModel? TryDraw(RelicGrabBag bag, Func<RelicModel, bool> filter, IRunState run)
	{
		if (!SpoonCursedDeck.IsActive(run)) return null;
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
		if (!CurseRule.CursedRelicRoll(rng.NextInt(int.MaxValue))) return null;
		List<RelicModel> left = Pool.Where(r => !excluded.Contains(r.Id.Entry) && r.IsAllowed(run) && filter(r)).ToList();
		string? id = CurseRule.PickCursedRelic(left.Select(r => r.Id.Entry).ToList(), new HashSet<string>(), rng.NextInt(int.MaxValue));
		if (id == null) return null;
		if (!state.Reserved.TryGetValue(key, out var set)) state.Reserved[key] = set = new HashSet<string>();
		set.Add(id);
		return left.First(r => r.Id.Entry == id);
	}
}

/// <summary>
/// Cursed Deck: the cursed draw applies only inside the vanilla methods that generate a chest, elite or merchant relic.
/// The scope is a thread-static depth counter (these methods are synchronous), so event grants, the rest-site Dig,
/// Pael's Wing, Toy Box and Large Capsule stay vanilla.
/// </summary>
public static class CursedScope
{
	[ThreadStatic] private static int _depth;

	public static bool Active => _depth > 0;

	public static void Enter() => _depth++;

	public static void Exit() { if (_depth > 0) _depth--; }
}

/// <summary>Chest: the treasure room's relic generation.</summary>
[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.BeginRelicPicking))]
public static class CursedChestScopePatch
{
	public static void Prefix() => CursedScope.Enter();

	public static void Finalizer() => CursedScope.Exit();
}

/// <summary>Elite: the relic reward is populated while the current room is an elite fight.</summary>
[HarmonyPatch(typeof(RelicReward), nameof(RelicReward.Populate))]
public static class CursedEliteScopePatch
{
	public static void Prefix(RelicReward __instance, out bool __state)
	{
		__state = __instance.Player.RunState.CurrentRoom is { RoomType: RoomType.Elite };
		if (__state) CursedScope.Enter();
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
	public static void Prefix() => CursedScope.Enter();

	public static void Finalizer() => CursedScope.Exit();
}

/// <summary>Cursed Deck: relic pulls from the front of a grab bag (chest, elite) are cursed 2 times in 3.</summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromFront), typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState))]
public static class CursedRelicDrawPatch
{
	public static bool Prefix(RelicGrabBag __instance, Func<RelicModel, bool> filter, IRunState runState, ref RelicModel? __result)
	{
		if (!CursedScope.Active || CursedRelics.TryDraw(__instance, filter, runState) is not { } relic) return true;
		__result = relic;
		return false;
	}
}

/// <summary>Cursed Deck: the merchant's relic pulls (from the back of a grab bag) are cursed 2 times in 3 too.</summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromBack), typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState))]
public static class CursedRelicShopDrawPatch
{
	public static bool Prefix(RelicGrabBag __instance, Func<RelicModel, bool> filter, IRunState runState, ref RelicModel? __result)
	{
		if (!CursedScope.Active || CursedRelics.TryDraw(__instance, filter, runState) is not { } relic) return true;
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
		if (__instance.Owner is { } owner && !SpoonCursedDeck.IsActive(owner.RunState)) return;
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
