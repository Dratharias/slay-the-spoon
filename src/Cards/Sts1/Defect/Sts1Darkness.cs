using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Defect;

/// <summary>StS1's weaker Darkness: only the upgrade triggers the Dark orbs' passive, once.</summary>
public sealed class Sts1Darkness : Sts1DefectCard
{
	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(Darkness);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<DarkOrb>(),
	};

	public Sts1Darkness()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		await OrbCmd.Channel<DarkOrb>(choiceContext, Owner);
		if (!IsUpgraded)
		{
			return;
		}
		foreach (OrbModel orb in Owner.PlayerCombatState!.OrbQueue.Orbs.Where(o => o is DarkOrb).ToList())
		{
			await OrbCmd.Passive(choiceContext, orb, null);
		}
	}
}
