using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Sadistic Nature: vanilla Sleight of Flesh's power has the same rule, temporary debuffs excluded as StS1 excluded Shackled.</summary>
public sealed class Sts1SadisticNature : Sts1ColorlessCard
{
	private const string PowerKey = nameof(SleightOfFleshPower);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SleightOfFleshPower>(5m),
	};

	public Sts1SadisticNature()
		: base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<SleightOfFleshPower>(choiceContext, Owner.Creature, DynamicVars[PowerKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[PowerKey].UpgradeValueBy(2m);
	}
}
