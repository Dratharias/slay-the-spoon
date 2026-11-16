using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using SlaySpoon.Cards.Sts1.Ironclad;

namespace SlaySpoon.Spoon;

/// <summary>
/// The "Test: The Spoon" Ironclad deck (owner list; every card upgraded since 2026-11-16, when the owner found the bare
/// deck too weak for a late-game boss) and its relics. Metallicize is the StS1 port: StS2 has none.
/// </summary>
public static class SpoonTestDeck
{
	public static IReadOnlyList<(CardModel Card, int Count)> Cards =>
	[
		(ModelDb.Card<BodySlam>(), 2),
		(ModelDb.Card<Barricade>(), 1),
		(ModelDb.Card<Entrench>(), 1),
		(ModelDb.Card<Impervious>(), 1),
		(ModelDb.Card<ShrugItOff>(), 2),
		(ModelDb.Card<FlameBarrier>(), 1),
		(ModelDb.Card<IronWave>(), 2),
		(ModelDb.Card<Sts1Metallicize>(), 1),
		(ModelDb.Card<Juggernaut>(), 1),
		(ModelDb.Card<DefendIronclad>(), 4),
		(ModelDb.Card<StrikeIronclad>(), 2),
		(ModelDb.Card<Bash>(), 1),
	];

	/// <summary>
	/// Block and Strength relics a run reaching the Spoon would plausibly hold. All act in combat only (no pickup effect),
	/// so adding them at creation without <c>AfterObtained</c> loses nothing.
	/// </summary>
	public static IReadOnlyList<RelicModel> Relics =>
	[
		ModelDb.Relic<Anchor>(),
		ModelDb.Relic<HornCleat>(),
		ModelDb.Relic<CaptainsWheel>(),
		ModelDb.Relic<Orichalcum>(),
		ModelDb.Relic<BronzeScales>(),
		ModelDb.Relic<Vajra>(),
		ModelDb.Relic<Lantern>(),
	];
}
