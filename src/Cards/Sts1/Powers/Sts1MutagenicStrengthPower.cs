using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Relics;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>Mutagenic Strength's Strength, lost at the end of the first turn (vanilla temporary Strength).</summary>
public sealed class Sts1MutagenicStrengthPower : TemporaryStrengthPower, IBorrowedIcon
{
	public override AbstractModel OriginModel => ModelDb.Relic<Sts1MutagenicStrength>();

	public Type IconSource => typeof(FlexPotionPower);
}
