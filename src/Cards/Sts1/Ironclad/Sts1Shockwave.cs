using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Ironclad;

/// <summary>StS1 Shockwave, same as StS2's; kept because StS2's is a colorless card outside the Ironclad pool.</summary>
public sealed class Sts1Shockwave : Sts1IroncladCard
{
	private const string PowerKey = "Power";

	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(Shockwave);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>(),
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar(PowerKey, 3m),
	};

	public Sts1Shockwave()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_flying_slash");
		int amount = DynamicVars[PowerKey].IntValue;
		foreach (Creature enemy in CombatState!.HittableEnemies)
		{
			await PowerCmd.Apply<WeakPower>(choiceContext, enemy, amount, Owner.Creature, this);
			await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, amount, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars[PowerKey].UpgradeValueBy(2m);
	}
}
