using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Defect;

/// <summary>StS1's weaker Hyperbeam (26 damage) replaces the StS2 card; same beam animation.</summary>
public sealed class Sts1Hyperbeam : Sts1DefectCard
{
	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(Hyperbeam);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<FocusPower>() };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(26m, ValueProp.Move),
		new PowerVar<FocusPower>(3m),
	};

	public Sts1Hyperbeam()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!)
			.WithAttackerAnim("Cast", 0.5f)
			.BeforeDamage(PlayBeam)
			.Execute(choiceContext);
		await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, -DynamicVars["FocusPower"].BaseValue, Owner.Creature, this);
	}

	private async Task PlayBeam()
	{
		List<Creature> enemies = CombatState!.Enemies.Where(e => e.IsAlive).ToList();
		if (enemies.Count == 0)
		{
			return;
		}
		NHyperbeamVfx? beam = NHyperbeamVfx.Create(Owner.Creature, enemies.Last());
		if (beam != null)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(beam);
			await Cmd.Wait(0.5f);
		}
		foreach (Creature enemy in enemies)
		{
			NHyperbeamImpactVfx? impact = NHyperbeamImpactVfx.Create(Owner.Creature, enemy);
			if (impact != null)
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(impact);
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(8m);
	}
}
