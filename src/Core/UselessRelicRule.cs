namespace SlaySpoon.Core;

/// <summary>
/// Useless Relics (Custom Run): which relics are spent for the fight once they have flashed. A hand list of vanilla
/// relics whose combat effect happens once per fight (start of combat, first turn, first time something happens);
/// the game has no "used up" flag.
/// </summary>
public static class UselessRelicRule
{
	public const int Damage = 10;

	public static readonly IReadOnlySet<string> OncePerCombat = new HashSet<string>(StringComparer.Ordinal)
	{
		"AKABEKO", "ANCHOR", "BAG_OF_MARBLES", "BAG_OF_PREPARATION", "BIG_HAT", "BLOOD_VIAL", "BOOMING_CONCH",
		"BURNING_STICKS", "CENTENNIAL_PUZZLE", "CRACKED_CORE", "DELICATE_FROND", "DIVINE_DESTINY", "FAKE_ANCHOR",
		"FAKE_BLOOD_VIAL", "FENCING_MANUAL", "FESTIVE_POPPER", "FUNERARY_MASK", "GAMBLING_CHIP", "LANTERN", "NINJA_SCROLL",
		"ORANGE_DOUGH", "PANTOGRAPH", "PETRIFIED_TOAD", "POWER_CELL", "RADIANT_PEARL", "RED_MASK", "ROYAL_POISON",
		"RUINED_HELMET", "SYMBIOTIC_VIRUS", "THROWING_AXE", "TOOLBOX", "UNSETTLING_LAMP", "VAMBRACE", "VERY_HOT_COCOA",
		"VEXING_PUZZLEBOX",
	};

	/// <summary>Whether a relic can be thrown: on the list, flashed this combat, not melted and not thrown yet.</summary>
	public static bool CanThrow(string relicId, bool flashedThisCombat, bool melted, bool thrown) =>
		OncePerCombat.Contains(relicId) && flashedThisCombat && !melted && !thrown;
}
