using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Silent;

/// <summary>StS1's weaker Grand Finale (50 / 60 damage) replaces the StS2 card.</summary>
public sealed class Sts1GrandFinale : Sts1SilentCard
{
	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(GrandFinale);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(50m, ValueProp.Move),
	};

	protected override bool IsPlayable => PileType.Draw.GetPile(Owner).Cards.Count == 0;

	public override bool HasCantPlayMessage => true;

	protected override bool ShouldGlowGoldInternal => IsPlayable;

	public Sts1GrandFinale()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		NGrandFinaleVfx? vfx = NGrandFinaleVfx.Create(Owner.Creature);
		if (vfx != null)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			await Cmd.Wait(NGrandFinaleVfx.totalAnticipationDuration);
		}
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState!)
			.WithHitVfxNode(NGrandFinaleImpactVfx.Create)
			.WithHitFx(null, null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(10m);
	}
}
