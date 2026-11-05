using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace SlaySpoon.Relics.Sts1;

/// <summary>StS1's "Gain [E] at the start of your turn" boss relics, as StS2's Ectoplasm and Sozu do it.</summary>
public abstract class Sts1EnergyBossRelic : Sts1BossRelic
{
	protected virtual IEnumerable<DynamicVar> ExtraVars => Array.Empty<DynamicVar>();
	protected override IEnumerable<DynamicVar> CanonicalVars => ExtraVars.Prepend(new EnergyVar(1));
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.ForEnergy(this) };
	protected virtual bool GivesEnergy(Player player) => true;
	public override decimal ModifyMaxEnergy(Player player, decimal amount) =>
		player == Owner && GivesEnergy(player) ? amount + DynamicVars.Energy.IntValue : amount;
}
