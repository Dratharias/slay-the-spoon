using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Ironclad;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>Flex's Strength, lost at the end of the turn (vanilla temporary Strength, like Flex Potion).</summary>
public sealed class Sts1FlexPower : TemporaryStrengthPower, IBorrowedIcon
{
	public override AbstractModel OriginModel => ModelDb.Card<Sts1Flex>();

	public Type IconSource => typeof(FlexPotionPower);
}
