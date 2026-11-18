using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace SlaySpoon.Curses;

/// <summary>The cursed relics the Cursed Deck mode hands out. Task 7 extends this into the full pool.</summary>
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
}
