using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// Spoon mode rest option (owner, 2026-11-18): lose <see cref="SpoonRule.DigestMaxHpLoss"/> max HP, then heal to full.
/// Spends the rest action like Rest or Feast; disabled while it would take the last max HP.
/// </summary>
public sealed class SpoonDigestRestSiteOption(Player owner) : RestSiteOption(owner)
{
	public override string OptionId => "SPOON_DIGEST";
	public override IEnumerable<string> AssetPaths => [SpoonFeastRestSiteOption.IconImage];

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			description.Add("MaxHp", SpoonRule.DigestMaxHpLoss);
			return description;
		}
	}
	public override bool IsEnabled => SpoonRule.CanDigest(Owner.Creature.MaxHp);

	public override async Task<bool> OnSelect()
	{
		var creature = Owner.Creature;
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, SpoonRule.DigestMaxHpLoss, isFromCard: false);
		await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp);
		return true;
	}
}
