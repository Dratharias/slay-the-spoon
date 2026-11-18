using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.Cards;

/// <summary>A StS1 curse handed out by a StS1 relic only; its hidden pool keeps it out of every random curse.</summary>
public abstract class Sts1CurseCard : Sts1Card
{
	protected Sts1CurseCard(int cost = -1, TargetType target = TargetType.None) : base(cost, CardType.Curse, CardRarity.Curse, target) { }

	public override CardPoolModel Pool => ModelDb.CardPool<Sts1CurseCardPool>();
	public override CardPoolModel VisualCardPool => ModelDb.CardPool<CurseCardPool>();
	public override bool CanBeGeneratedByModifiers => false;
	public override bool CanBeGeneratedInCombat => false;
	public override int MaxUpgradeLevel => 0;
	protected override string PortraitFolder => "curses";
}
