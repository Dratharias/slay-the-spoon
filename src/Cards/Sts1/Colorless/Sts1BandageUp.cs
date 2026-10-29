using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace SlaySpoon.Cards.Sts1.Colorless;

public sealed class Sts1BandageUp : Sts1ColorlessCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new HealVar(4m),
	};

	public Sts1BandageUp()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Heal.UpgradeValueBy(2m);
	}
}
