using System.Text.Json;

namespace SlaySpoon.Core;

/// <summary>One row of data/curses/curses.json (only the fields the code reads).</summary>
public sealed record CurseRow(string Id, string Played, int Amount, int AmountPlus, string? SelfVar, int[]? Self, int CostPlus);

public enum CurseEventReward { CursedRelic, Gold, Heal }

/// <summary>Numbers and rules of the curse deck relics and the Cursed Deck mode (owner idea, 2026-11-18).</summary>
public static class CurseRule
{
	/// <summary>Unholy Grimoire: damage to every enemy per curse exhausted.</summary>
	public const int GrimoireDamage = 5;

	/// <summary>Grimoire damage of an upgraded curse that has no played effect.</summary>
	public const int GrimoireNoEffectPlus = 12;

	/// <summary>Energy cost of a curse made playable by the Grimoire when the vanilla card is unplayable.</summary>
	public const int PlayCost = 1;

	/// <summary>Blood Pact: Block gained whenever the holder draws a curse.</summary>
	public const int BloodPactBlock = 4;

	/// <summary>Pin Doll: one Strength per this many curses in the deck.</summary>
	public const int PinDollCursesPerStrength = 3;

	/// <summary>Reliquary: gold per curse in the deck after each combat.</summary>
	public const int ReliquaryGoldPerCurse = 3;

	/// <summary>Pin Doll: Strength per full group of curses.</summary>
	public const int PinDollStrengthPerGroup = 1;

	public static int PinDollStrength(int curses) => Math.Max(0, curses) / PinDollCursesPerStrength * PinDollStrengthPerGroup;

	public static int ReliquaryGold(int curses) => Math.Max(0, curses) * ReliquaryGoldPerCurse;

	public const int EventGold = 75;
	public const int EventHealPercent = 25;

	/// <summary>Parses the curse table. Keys starting with "_" are comments.</summary>
	public static IReadOnlyDictionary<string, CurseRow> ParseTable(string json)
	{
		var result = new Dictionary<string, CurseRow>(StringComparer.Ordinal);
		using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
		foreach (var row in doc.RootElement.EnumerateObject())
		{
			if (row.Name.StartsWith('_')) continue;
			var e = row.Value;
			string played = e.TryGetProperty("played", out var p) ? p.GetString() ?? "None" : "None";
			int amount = e.TryGetProperty("amount", out var a) ? a.GetInt32() : 0;
			int plus = e.TryGetProperty("amountPlus", out var ap) ? ap.GetInt32() : amount;
			string? selfVar = e.TryGetProperty("selfVar", out var sv) ? sv.GetString() : null;
			int[]? self = null;
			if (e.TryGetProperty("self", out var s))
			{
				self = s.EnumerateArray().Select(x => x.GetInt32()).ToArray();
				if (self.Length != 2) throw new FormatException($"{row.Name}: self needs [vanilla, upgraded]");
			}
			int cost = e.TryGetProperty("costPlus", out var c) ? c.GetInt32() : 0;
			result[row.Name] = new CurseRow(row.Name, played, amount, plus, selfVar, self, cost);
		}
		return result;
	}

	/// <summary>Damage to every enemy when the Grimoire exhausts the curse; a curse without a row counts as having no effect.</summary>
	public static int GrimoireDamageFor(CurseRow? row, bool upgraded) => GrimoireDamageFor(row, upgraded ? 1 : 0);

	/// <summary>Grimoire damage at any level (forged curses go past +1, <see cref="ForgedCurseRule.GrimoireAt"/>).</summary>
	public static int GrimoireDamageFor(CurseRow? row, int level) => ForgedCurseRule.GrimoireAt(row, level);

	public static int CostFor(CurseRow? row, int vanillaCost, bool upgraded) =>
		upgraded ? row?.CostPlus ?? 0 : vanillaCost >= 0 ? vanillaCost : PlayCost;

	public static int EnemyAmount(CurseRow row, bool upgraded) => EnemyAmount(row, upgraded ? 1 : 0);

	public static int EnemyAmount(CurseRow row, int level) => ForgedCurseRule.AmountAt(row, level);

	public static bool IsPlayable(bool modeActive, bool holdsGrimoire) => modeActive && holdsGrimoire;

	/// <summary>
	/// Energy cost shown and paid for a playable curse, or null to keep the vanilla value. A vanilla-unplayable curse
	/// (canonical −1) costs <see cref="CostFor"/>; an already-playable one (Enthralled, Spore Mind, Pride) keeps its
	/// vanilla cost, modifiers included, until upgraded, then shifts by the row's upgraded cost.
	/// </summary>
	public static int? PlayedCost(CurseRow? row, int canonical, bool upgraded, int vanillaWithModifiers)
	{
		if (canonical < 0) return CostFor(row, canonical, upgraded);
		if (!upgraded) return null;
		return Math.Max(0, vanillaWithModifiers + CostFor(row, canonical, true) - canonical);
	}

	/// <summary>
	/// The amount of the row's played effect on the chosen enemy: per card left in the hand (Regret, counted after the
	/// curse left it), per card played this turn (Pain, itself included), flat otherwise; 0 for "None".
	/// </summary>
	public static int PlayedAmount(CurseRow row, bool upgraded, int handCards, int cardsPlayedThisTurn) =>
		PlayedAmount(row, upgraded ? 1 : 0, handCards, cardsPlayedThisTurn);

	public static int PlayedAmount(CurseRow row, int level, int handCards, int cardsPlayedThisTurn) => row.Played switch
	{
		"None" => 0,
		"Stun" => WitchingHourRule.StunTurnsAt(level),
		"HpLossPerHandCard" => EnemyAmount(row, level) * Math.Max(0, handCards),
		"HpLossPerCardPlayed" => EnemyAmount(row, level) * Math.Max(0, cardsPlayedThisTurn),
		_ => EnemyAmount(row, level),
	};

	/// <summary>2 rolls in 3 give a cursed relic.</summary>
	public static bool CursedRelicRoll(int roll) => roll % 3 != 0;

	public static string? PickCursedRelic(IReadOnlyList<string> pool, ISet<string> held, int roll)
	{
		var left = pool.Where(r => !held.Contains(r)).ToList();
		return left.Count == 0 ? null : left[(int)((uint)roll % (uint)left.Count)];
	}

	public static CurseEventReward EventReward(int roll, bool cursedRelicLeft) => (roll % 3) switch
	{
		0 => cursedRelicLeft ? CurseEventReward.CursedRelic : CurseEventReward.Gold,
		1 => CurseEventReward.Gold,
		_ => CurseEventReward.Heal,
	};

	/// <summary>Where the Embrace option goes: before the first LEAVE or PROCEED option, else last.</summary>
	public static int EmbraceIndex(IReadOnlyList<string> optionKeys)
	{
		for (int i = 0; i < optionKeys.Count; i++)
		{
			if (optionKeys[i].EndsWith("LEAVE", StringComparison.Ordinal) || optionKeys[i].EndsWith("PROCEED", StringComparison.Ordinal)) return i;
		}
		return optionKeys.Count;
	}

	/// <summary>The heal reward: 25 % of max HP, rounded down, at least 1.</summary>
	public static int EventHeal(int maxHp) => Math.Max(1, maxHp * EventHealPercent / 100);

	/// <summary>Stream of one cursed relic draw; bagKey is the owner's player slot, or -1 for the shared bag.</summary>
	public static string RelicStream(int floor, int bagKey, int draw) => $"cursed_deck_relic_{floor}_{bagKey}_{draw}";

	public static string RelicStream(int floor, int draw) => RelicStream(floor, -1, draw);
	public static string EventStream(int floor, int slot) => $"cursed_deck_event_{floor}_{slot}";
	public static string StartStream(int slot) => $"cursed_deck_start_{slot}";

	/// <summary>A playable curse picks an enemy only when it has a played effect.</summary>
	public static bool NeedsTarget(CurseRow? row) => row != null && row.Played != "None";
}
