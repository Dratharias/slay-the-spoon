using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Achilles;

/// <summary>Which cards count as Block cards for the Achilles' Heel offer lean.</summary>
public static class AchillesBlock
{
	public static bool IsBlockCard(CardModel card) => card.GainsBlock || card.DynamicVars.Values.Any(v => v is BlockVar);
}
