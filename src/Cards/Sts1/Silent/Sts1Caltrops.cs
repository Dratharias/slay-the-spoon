using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Silent;

/// <summary>StS1 Caltrops, same as StS2's; kept because StS2's is an Event card outside the Silent pool.</summary>
public sealed class Sts1Caltrops : Sts1SilentCard
{
	private const string PowerKey = nameof(ThornsPower);

	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(Caltrops);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ThornsPower>(3m),
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<ThornsPower>() };

	public Sts1Caltrops()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<ThornsPower>(choiceContext, Owner.Creature, DynamicVars[PowerKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[PowerKey].UpgradeValueBy(2m);
	}
}
