using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Silent;

/// <summary>StS1 Wraith Form: Dexterity is lost at the end of the turn, where StS2's loses it at the start.</summary>
public sealed class Sts1WraithForm : Sts1SilentCard
{
	private const string IntangibleKey = nameof(IntangiblePower);

	private const string DexterityLossKey = nameof(Sts1WraithFormPower);

	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(WraithForm);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<IntangiblePower>(2m),
		new PowerVar<Sts1WraithFormPower>(1m),
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.FromPower<IntangiblePower>(),
		HoverTipFactory.FromPower<DexterityPower>(),
	};

	public Sts1WraithForm()
		: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, DynamicVars[IntangibleKey].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<Sts1WraithFormPower>(choiceContext, Owner.Creature, DynamicVars[DexterityLossKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[IntangibleKey].UpgradeValueBy(1m);
	}
}
