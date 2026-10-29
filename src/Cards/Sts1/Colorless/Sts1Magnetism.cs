using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Magnetism: a random colorless card at the start of each turn.</summary>
public sealed class Sts1Magnetism : Sts1ColorlessCard
{
	private const string PowerKey = nameof(Sts1MagnetismPower);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<Sts1MagnetismPower>(1m),
	};

	public Sts1Magnetism()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<Sts1MagnetismPower>(choiceContext, Owner.Creature, DynamicVars[PowerKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
