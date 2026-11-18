using System.Text.Json;

namespace SlaySpoon.Core;

/// <summary>Price bands of the Forge's Duplicate service (spec decision 4). Every special rarity prices as Rare.</summary>
public enum ForgeRarity
{
	Common,
	Uncommon,
	Rare,
}

/// <summary>
/// One row of <c>data/forge/curves.json</c>: how a card behaves past +1.
/// <see cref="Cap"/> is the growth cap: up to it, each level replays the +1 var deltas (or <see cref="PerLevel"/>) and
/// lowers the cost by <see cref="CostPerLevel"/> (floor 0). Past it, fusion still goes to +9, but only the vars in
/// <see cref="Beyond"/> keep growing, and the <see cref="ForgeBonus"/> ladder applies.
/// </summary>
public sealed record ForgeCurve(
	int? Cap,
	bool Fusable,
	IReadOnlyDictionary<string, decimal>? PerLevel,
	int CostPerLevel,
	string? Note,
	IReadOnlyList<string>? Beyond = null,
	IReadOnlyDictionary<string, decimal>? PowerBonus = null,
	IReadOnlyDictionary<string, decimal>? Play = null);

/// <summary>The fixed bonuses every fusable card gets at high levels (owner, 2026-11-18).</summary>
[Flags]
public enum ForgeBonus
{
	None = 0,
	Retain = 1,
	CostDown = 2,
	Innate = 4,
}

/// <summary>The Forge (spec docs/superpowers/specs/2026-11-18-forge-design.md): fusion levels, caps and prices.</summary>
public static class ForgeRule
{
	public const int MaxLevel = 9;

	public const int DuplicateIncrease = 25;

	public const int RetainLevel = 6;
	public const int CostDownLevel = 7;
	public const int InnateLevel = 9;

	/// <summary>The ladder bonus a card gains on reaching <paramref name="level"/>: Retain at +6, cost −1 at +7, Innate at +9.</summary>
	public static ForgeBonus BonusAt(int level) => level switch
	{
		RetainLevel => ForgeBonus.Retain,
		CostDownLevel => ForgeBonus.CostDown,
		InnateLevel => ForgeBonus.Innate,
		_ => ForgeBonus.None,
	};

	/// <summary>Every ladder bonus a card at <paramref name="level"/> holds.</summary>
	public static ForgeBonus BonusesUpTo(int level)
	{
		var all = ForgeBonus.None;
		for (int l = 2; l <= Math.Min(level, MaxLevel); l++) all |= BonusAt(l);
		return all;
	}

	/// <summary>The play bonus kinds the runtime knows (data/forge/curves.json "play").</summary>
	public static readonly IReadOnlySet<string> PlayKinds = new HashSet<string>(StringComparer.Ordinal)
	{
		"Block", "Damage", "Draw", "Energy", "Strength", "Dexterity", "Vigor", "Poison", "Weak", "Vulnerable",
	};

	/// <summary>A per-level bonus at <paramref name="level"/>: one step per level past +1, up to the card's growth cap.</summary>
	public static decimal PerLevelBonus(decimal perLevel, int level, int cap = MaxLevel) =>
		level <= 1 ? 0 : perLevel * (Math.Clamp(Math.Min(level, cap), 1, MaxLevel) - 1);

	private static Dictionary<string, decimal>? Amounts(JsonElement e, string name)
	{
		if (!e.TryGetProperty(name, out var obj) || obj.ValueKind != JsonValueKind.Object) return null;
		var d = new Dictionary<string, decimal>(StringComparer.Ordinal);
		foreach (var p in obj.EnumerateObject()) d[p.Name] = p.Value.GetDecimal();
		return d.Count > 0 ? d : null;
	}

	/// <summary>Levels add up: kept + sacrificed + 1, clamped to the cap.</summary>
	public static int FusedLevel(int kept, int sacrificed, int cap) =>
		Math.Min(Math.Max(kept, 0) + Math.Max(sacrificed, 0) + 1, Math.Clamp(cap, 1, MaxLevel));

	/// <summary>A card can be fused into while it is fusable and below its cap (the result clamps to the cap).</summary>
	public static bool CanFuseInto(int keptLevel, int cap, bool fusable) => fusable && keptLevel < Math.Clamp(cap, 1, MaxLevel);

	/// <summary>25 / 50 / 75 by rarity, +25 at Ascension 6+ (Inflation), +25 per earlier purchase.</summary>
	public static int DuplicatePrice(ForgeRarity rarity, bool inflation, int purchases)
	{
		int band = rarity switch
		{
			ForgeRarity.Common => 25,
			ForgeRarity.Uncommon => 50,
			_ => 75,
		};
		return band + (inflation ? 25 : 0) + DuplicateIncrease * Math.Max(purchases, 0);
	}

	/// <summary>
	/// A card whose +1 only lowers its cost keeps lowering it by the same step at each level. Its cap is the level where
	/// the cost reaches 0: a 3-cost card at 2 after +1 reaches 0 at +3. A card already at 0 after +1 caps at 1.
	/// </summary>
	public static int CostCap(int costAfterFirst, int costDelta)
	{
		if (costDelta >= 0 || costAfterFirst <= 0) return 1;
		int step = -costDelta;
		int extra = (costAfterFirst + step - 1) / step;
		return Math.Min(1 + extra, MaxLevel);
	}

	/// <summary>The cost at <paramref name="level"/> for a card with a per-level cost step (floor 0).</summary>
	public static int CostAt(int costAfterFirst, int costPerLevel, int level) =>
		level <= 1 ? costAfterFirst : Math.Max(costAfterFirst + costPerLevel * (level - 1), 0);

	/// <summary>Parses the curve table. Keys starting with "_" are comments.</summary>
	public static IReadOnlyDictionary<string, ForgeCurve> ParseCurves(string json)
	{
		var result = new Dictionary<string, ForgeCurve>(StringComparer.Ordinal);
		using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
		foreach (var row in doc.RootElement.EnumerateObject())
		{
			if (row.Name.StartsWith('_')) continue;
			var e = row.Value;
			int? cap = null;
			if (e.TryGetProperty("cap", out var c))
			{
				int v = c.GetInt32();
				if (v < 1 || v > MaxLevel) throw new FormatException($"{row.Name}: cap {v} outside 1-{MaxLevel}");
				cap = v;
			}
			bool fusable = !e.TryGetProperty("fusable", out var f) || f.GetBoolean();
			Dictionary<string, decimal>? perLevel = null;
			if (e.TryGetProperty("perLevel", out var p))
			{
				perLevel = new Dictionary<string, decimal>(StringComparer.Ordinal);
				foreach (var v in p.EnumerateObject()) perLevel[v.Name] = v.Value.GetDecimal();
			}
			int cost = e.TryGetProperty("costPerLevel", out var cp) ? cp.GetInt32() : 0;
			if (cost > 0) throw new FormatException($"{row.Name}: costPerLevel must not raise the cost");
			string? note = e.TryGetProperty("note", out var n) ? n.GetString() : null;
			var powerBonus = Amounts(e, "powerBonus");
			var play = Amounts(e, "play");
			List<string>? beyond = null;
			if (e.TryGetProperty("beyond", out var b))
				beyond = b.EnumerateArray().Select(v => v.GetString() ?? "").Where(v => v.Length > 0).ToList();
			result[row.Name] = new ForgeCurve(cap, fusable, perLevel, cost, note, beyond, powerBonus, play);
		}
		return result;
	}
}
