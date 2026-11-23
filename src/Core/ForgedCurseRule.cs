namespace SlaySpoon.Core;

/// <summary>Defects a forged curse picks up at +3, +6 and +9 (spec docs/superpowers/specs/2026-11-23-forged-curses-design.md).</summary>
[Flags]
public enum CurseDefect
{
	None = 0,
	Odd = 1,
	Even = 2,
	Late = 4,
	Heavy = 8,
	Demanding = 16,
	Bloody = 32,
	Spiteful = 64,
	Contagious = 128,
	Ravenous = 256,
	Echo = 512,
}

/// <summary>Numbers and defect rules of forged curses (The Forge + Cursed Deck, owner 2026-11-23).</summary>
public static class ForgedCurseRule
{
	public const int MaxLevel = 9;

	/// <summary>Turn from which a Late curse can be played.</summary>
	public const int LateTurn = 3;

	/// <summary>Other cards a Demanding curse needs played first this turn.</summary>
	public const int DemandingCards = 2;

	/// <summary>Extra energy a Heavy curse costs.</summary>
	public const int HeavyCost = 2;

	public static readonly IReadOnlyList<CurseDefect> All =
	[
		CurseDefect.Odd, CurseDefect.Even, CurseDefect.Late, CurseDefect.Heavy, CurseDefect.Demanding,
		CurseDefect.Bloody, CurseDefect.Spiteful, CurseDefect.Contagious, CurseDefect.Ravenous, CurseDefect.Echo,
	];

	/// <summary>Each level repeats the +0 → +1 step: amount + (amountPlus − amount) × level.</summary>
	public static int AmountAt(CurseRow row, int level) => row.Amount + (row.AmountPlus - row.Amount) * Math.Max(level, 0);

	/// <summary>Grimoire damage on exhaust: 5, or for a curse with no played effect 12 at +1 then +3.5 per level, rounded down.</summary>
	public static int GrimoireAt(CurseRow? row, int level)
	{
		bool noEffect = row == null || row.Played == "None";
		if (!noEffect || level < 1) return CurseRule.GrimoireDamage;
		return CurseRule.GrimoireNoEffectPlus + 7 * (level - 1) / 2;
	}

	/// <summary>Normality: cards allowed per turn, 3 + level.</summary>
	public static int NormalityLimitAt(int level) => 3 + Math.Max(level, 0);

	/// <summary>Guilty: combats before it leaves the deck: 5 at +0, 3 at +1, one fewer per level, at least 1.</summary>
	public static int GuiltyCombatsAt(int level) => level <= 0 ? 5 : Math.Max(1, 3 - (level - 1));

	public static int DefectCount(int level) => level >= 9 ? 3 : level >= 6 ? 2 : level >= 3 ? 1 : 0;

	/// <summary>Odd and Even never sit on the same card; no defect twice.</summary>
	public static bool Compatible(CurseDefect set, CurseDefect add)
	{
		if ((set & add) != 0) return false;
		if (add == CurseDefect.Odd && set.HasFlag(CurseDefect.Even)) return false;
		if (add == CurseDefect.Even && set.HasFlag(CurseDefect.Odd)) return false;
		return true;
	}

	private static int CountOf(CurseDefect set) => All.Count(d => set.HasFlag(d));

	/// <summary>
	/// The fused curse's defects: the kept copy's (never removed), then the sacrificed copy's in table order, then draws
	/// among the compatible rest until <see cref="DefectCount"/> is reached. <paramref name="roll"/>(n) returns 0..n−1.
	/// </summary>
	public static CurseDefect Merge(CurseDefect kept, CurseDefect sacrificed, int level, Func<int, int> roll)
	{
		int count = DefectCount(level);
		var set = kept;
		foreach (var d in All)
			if (CountOf(set) < count && sacrificed.HasFlag(d) && Compatible(set, d)) set |= d;
		while (CountOf(set) < count)
		{
			var left = All.Where(d => Compatible(set, d)).ToList();
			if (left.Count == 0) break;
			set |= left[Math.Clamp(roll(left.Count), 0, left.Count - 1)];
		}
		return set;
	}

	/// <summary>Odd, Even, Late and Demanding make the curse unplayable, even with the Grimoire. Rounds count from 1.</summary>
	public static bool BlocksPlay(CurseDefect set, int round, int otherCardsPlayedThisTurn) =>
		set.HasFlag(CurseDefect.Odd) && round % 2 == 1
		|| set.HasFlag(CurseDefect.Even) && round % 2 == 0
		|| set.HasFlag(CurseDefect.Late) && round < LateTurn
		|| set.HasFlag(CurseDefect.Demanding) && otherCardsPlayedThisTurn < DemandingCards;

	public static int CostWithDefects(CurseDefect set, int cost, bool playable) =>
		playable && set.HasFlag(CurseDefect.Heavy) ? cost + HeavyCost : cost;

	public static int EchoAmount(int played) => Math.Max(played, 0) / 2;

	public static int BloodyHp(int level) => Math.Max(level, 0);

	public static int SpitefulTimes(CurseDefect set) => set.HasFlag(CurseDefect.Spiteful) ? 2 : 1;

	/// <summary>A curse's max level: 9 inside an open Forge scope (fusion, reload) in the combo, else 1 (Cursed Deck's +1).</summary>
	public static int MaxLevelFor(bool combo, bool inScope, int vanillaMax) =>
		combo && inScope ? MaxLevel : Math.Max(vanillaMax, 1);

	/// <summary>Level a saved curse reloads to: up to 9 in the combo, else at most 1.</summary>
	public static int LoadLevel(int saved, bool comboActive) => Math.Clamp(saved, 0, comboActive ? MaxLevel : 1);

	public static string DefectStream(int floor, int deckIndex, int draw) => $"forged_curse_defect_{floor}_{deckIndex}_{draw}";

	/// <summary>Stream for Contagious and Ravenous draws: floor, combat round and the play's index in the combat history.</summary>
	public static string PlayStream(int floor, int round, int playIndex) => $"forged_curse_play_{floor}_{round}_{playIndex}";
}
