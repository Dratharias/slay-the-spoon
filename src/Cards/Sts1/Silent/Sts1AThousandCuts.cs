using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Cards.Sts1.Silent;

public sealed class Sts1AThousandCuts : Sts1SilentCard
{
	private const string PowerKey = nameof(Sts1AThousandCutsPower);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<Sts1AThousandCutsPower>(1m),
	};

	public Sts1AThousandCuts()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<Sts1AThousandCutsPower>(choiceContext, Owner.Creature, DynamicVars[PowerKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[PowerKey].UpgradeValueBy(1m);
	}
}
