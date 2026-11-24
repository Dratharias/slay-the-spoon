namespace SlaySpoon.Core;

/// <summary>
/// The Witching Pact (owner, 2026-11-24; docs/superpowers/specs/2026-11-24-witching-pact-design.md): a Cursed Deck Neow
/// option, not offered with Overcursed. It opens a chest of <see cref="Size"/> boss relics; taking one makes the Witching
/// Hour replace Ascender's Bane (or join the deck without one), skipping refuses the pact.
/// </summary>
public static class WitchingPactRule
{
	public const int Size = 3;

	public static bool Offered(bool cursedDeck, bool overcursed) => cursedDeck && !overcursed;

	/// <summary>What sealing the pact does to the deck.</summary>
	public static WitchingPactDeal Deal(bool hasAscendersBane) =>
		hasAscendersBane ? WitchingPactDeal.ReplaceBane : WitchingPactDeal.AddCurse;

	public static string Stream(int slot) => $"witching_pact_{slot}";
}

public enum WitchingPactDeal
{
	ReplaceBane,
	AddCurse,
}
