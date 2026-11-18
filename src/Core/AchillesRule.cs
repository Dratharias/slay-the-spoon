namespace SlaySpoon.Core;

/// <summary>Achilles' Heel: the numbers and pure rules (docs/superpowers/specs/2026-11-18-achilles-heel-design.md).</summary>
public static class AchillesRule
{
	/// <summary>Block needed for one point of the Block bonus.</summary>
	public const int BlockPerBonus = 5;

	/// <summary>Offer weight multiplier for Block cards while a player holds the relic.</summary>
	public const double BlockLean = 2.0;

	/// <summary>Relic ids (model id entries) every player starts with.</summary>
	public static readonly IReadOnlyList<string> StartRelics = ["ANCHOR", "ODDLY_SMOOTH_STONE", "GORGET"];

	/// <summary>Dexterity for a 0-based act index.</summary>
	public static int DexterityFor(int actIndex) => Math.Max(0, actIndex);

	/// <summary>One bonus point per <see cref="BlockPerBonus"/> Block.</summary>
	public static int BlockBonus(int block) => Math.Max(0, block) / BlockPerBonus;

	/// <summary>The heel kills only when a powered attack deals unblocked damage.</summary>
	public static bool HeelKills(bool poweredAttack, int unblocked) => poweredAttack && unblocked > 0;
}
