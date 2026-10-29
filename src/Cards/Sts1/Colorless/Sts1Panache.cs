using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Panache: copies share one 5-card countdown and add up their damage.</summary>
public sealed class Sts1Panache : Sts1ColorlessCard
{
	private const string PowerKey = nameof(Sts1PanachePower);

	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.Panache);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<Sts1PanachePower>(10m),
	};

	public Sts1Panache()
		: base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<Sts1PanachePower>(choiceContext, Owner.Creature, DynamicVars[PowerKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[PowerKey].UpgradeValueBy(4m);
	}
}
