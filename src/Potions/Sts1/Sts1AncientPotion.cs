using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Potions.Sts1;

/// <summary>
/// StS1's Ancient Potion (uncommon, gain 1 Artifact), which StS2 has no counterpart for. Offered to every character in
/// StS1-only runs (<see cref="Sts1PotionOptionsPatch"/>). Like StS2's Dexterity Potion it may target an ally in co-op.
/// </summary>
public sealed class Sts1AncientPotion : PotionModel, IBorrowedIcon
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AnyPlayer;
	public Type IconSource => typeof(LiquidBronze);
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<ArtifactPower>(1m) };
	public override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<ArtifactPower>() };

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		AssertValidForTargetedPotion(target);
		await PowerCmd.Apply<ArtifactPower>(choiceContext, target!, DynamicVars[nameof(ArtifactPower)].BaseValue, Owner.Creature, null);
	}
}
