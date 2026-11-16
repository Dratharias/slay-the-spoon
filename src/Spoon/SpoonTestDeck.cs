using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SlaySpoon.Cards.Sts1.Ironclad;

namespace SlaySpoon.Spoon;

/// <summary>The "Test: The Spoon" Ironclad deck (owner list, unupgraded). Metallicize is the StS1 port: StS2 has none.</summary>
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
}
