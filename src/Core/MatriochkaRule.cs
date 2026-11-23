namespace SlaySpoon.Core;

/// <summary>Pure rules of the Matriochka modifier (see docs/superpowers/specs/2026-11-14-matriochka-design.md).</summary>
public static class MatriochkaRule
{
	/// <summary>Card reward rerolls per player per act: the Custom Run row's −/+ value.</summary>
	public const int DefaultRerolls = 3;
	public const int MaxRerolls = 10;
	public static IReadOnlyList<int> AllRerolls { get; } = Enumerable.Range(0, MaxRerolls + 1).ToList();
	/// <summary>Card reward option id showing the rerolls left ("OPTION_SPOON_REROLL_3.name").</summary>
	public static string RerollOptionId(int left) => $"SPOON_REROLL_{Math.Clamp(left, 1, MaxRerolls)}";
	public static int StepRerolls(int rerolls, int direction) => Math.Clamp(rerolls + Math.Sign(direction), 0, MaxRerolls);

	/// <summary>0-based act index whose first event becomes the Doll Room.</summary>
	public const int DollRoomAct = 1;

	/// <summary>0-based act index holding the forced Reflections row.</summary>
	public const int ReflectionsAct = 2;

	/// <summary>
	/// Whether a non-combat room gets the floor's mandatory card reward: merchants, rest sites, treasure rooms and events,
	/// except Ancients and the victory room (the Architect after the last boss: the run is over, no final card, owner 2026-11-20).
	/// </summary>
	public static bool OffersFloorCard(bool shopRestOrTreasure, bool eventRoom, bool ancient, bool victoryRoom) =>
		!victoryRoom && (shopRestOrTreasure || (eventRoom && !ancient));

	/// <summary>
	/// Card rewards Matriochka puts back after vanilla Vintage turned a combat's card rewards into relics: one per card
	/// reward replaced, so the floor still has its card and the relic stays too (owner, 2026-11-23).
	/// </summary>
	public static int CardsAfterVintage(bool matriochka, int cardRewardsBefore, int cardRewardsAfter) =>
		matriochka ? Math.Max(0, cardRewardsBefore - cardRewardsAfter) : 0;

	/// <summary>Copies added with every card that enters the deck (vanilla Hoarder).</summary>
	public const int ExtraCopies = 2;

	/// <summary>Deck size where Matriochka stops adding cards (owner, 2026-11-23: a bot run reached 3 127 cards).</summary>
	public const int DeckLimit = 999;

	/// <summary>Whether a deck of <paramref name="deckCount"/> cards has reached the limit: no copies, no floor card,
	/// no combat card reward.</summary>
	public static bool DeckFull(int deckCount) => deckCount >= DeckLimit;

	/// <summary>Hoarder copies for a card that just entered a deck now holding <paramref name="deckCount"/> cards (the
	/// new card included): two, fewer near the limit, none past it.</summary>
	public static int CopiesFor(int deckCount) => Math.Clamp(DeckLimit - deckCount, 0, ExtraCopies);

	/// <summary>
	/// The map row just before the last row made only of rest sites, or null when there is no such row or it is the
	/// first row. <paramref name="restSiteRows"/> lists, row by row, whether each point of the row is a rest site.
	/// </summary>
	public static int? ReflectionsRow(IReadOnlyList<IReadOnlyList<bool>> restSiteRows)
	{
		for (int row = restSiteRows.Count - 1; row >= 0; row--)
		{
			var points = restSiteRows[row];
			if (points.Count > 0 && points.All(p => p))
				return row > 0 ? row - 1 : null;
		}
		return null;
	}
}

/// <summary>
/// Matriochka's card reward rerolls: each player has the row's value per act. Saved as
/// <c>[act index, used by player 0, used by player 1, ...]</c>; a new act starts everyone at zero.
/// </summary>
public sealed class MatriochkaRerolls
{
	private int _act;
	private int[] _used = Array.Empty<int>();

	public int Remaining(int player, int act, int perAct) =>
		act != _act || player < 0 || player >= _used.Length ? perAct : Math.Max(0, perAct - _used[player]);

	/// <summary>Spends one reroll; false when the player has none left this act.</summary>
	public bool Use(int player, int act, int perAct, int playerCount)
	{
		if (player < 0 || Remaining(player, act, perAct) <= 0)
			return false;
		if (act != _act)
		{
			_act = act;
			_used = Array.Empty<int>();
		}
		if (_used.Length < Math.Max(playerCount, player + 1))
			Array.Resize(ref _used, Math.Max(playerCount, player + 1));
		_used[player]++;
		return true;
	}

	public int[] Encode() => _used.Length == 0 ? Array.Empty<int>() : new[] { _act }.Concat(_used).ToArray();

	public static MatriochkaRerolls Decode(int[]? data)
	{
		var rerolls = new MatriochkaRerolls();
		if (data is { Length: > 1 })
		{
			rerolls._act = data[0];
			rerolls._used = data.Skip(1).Select(u => Math.Max(u, 0)).ToArray();
		}
		return rerolls;
	}
}
