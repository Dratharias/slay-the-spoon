using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
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

	/// <summary>Draws made on the current floor; not saved, so a reload replays the same rolls.</summary>
	private static int _floor = -1;
	private static int _draws;

	private static int NextDraw(int floor)
	{
		if (floor != _floor) { _floor = floor; _draws = 0; }
		return _draws++;
	}

	/// <summary>
	/// The cursed draw: in a Cursed Deck run, 2 pulls in 3 (own seeded stream, vanilla RNG untouched) give a cursed relic
	/// nobody concerned holds. Returns null to let the vanilla pull go on.
	/// </summary>
	public static RelicModel? TryDraw(RelicGrabBag bag, Func<RelicModel, bool> filter, IRunState run)
	{
		if (!SpoonCursedDeck.IsActive(run)) return null;
		var concerned = run.Players.Where(p => ReferenceEquals(p.RelicGrabBag, bag)).ToList();
		if (concerned.Count == 0 && ReferenceEquals(run.SharedRelicGrabBag, bag)) concerned = run.Players.ToList();
		if (concerned.Count == 0) return null;
		HashSet<string> held = concerned.SelectMany(p => p.Relics).Select(r => r.Id.Entry).ToHashSet();
		Rng rng = new(run.Rng.Seed, CurseRule.RelicStream(run.TotalFloor, NextDraw(run.TotalFloor)));
		if (!CurseRule.CursedRelicRoll(rng.NextInt(int.MaxValue))) return null;
		List<RelicModel> left = Pool.Where(r => !held.Contains(r.Id.Entry) && r.IsAllowed(run) && filter(r)).ToList();
		string? id = CurseRule.PickCursedRelic(left.Select(r => r.Id.Entry).ToList(), new HashSet<string>(), rng.NextInt(int.MaxValue));
		return id == null ? null : left.First(r => r.Id.Entry == id);
	}
}

/// <summary>Cursed Deck: relic pulls from the front of a grab bag are cursed 2 times in 3.</summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromFront), typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState))]
public static class CursedRelicDrawPatch
{
	public static bool Prefix(RelicGrabBag __instance, Func<RelicModel, bool> filter, IRunState runState, ref RelicModel? __result)
	{
		if (CursedRelics.TryDraw(__instance, filter, runState) is not { } relic) return true;
		__result = relic;
		return false;
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
