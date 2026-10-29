using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1Recursion : Sts1DefectCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.Static(StaticHoverTip.Evoke),
		HoverTipFactory.Static(StaticHoverTip.Channeling),
	};

	public Sts1Recursion()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		var first = Owner.PlayerCombatState!.OrbQueue.Orbs.FirstOrDefault();
		if (first == null)
		{
			return;
		}
		// Clone before Evoke marks the old orb removed; retain Dark's accumulated damage.
		var replacement = (OrbModel)first.MutableClone();
		await OrbCmd.EvokeNext(choiceContext, Owner);
		await OrbCmd.Channel(choiceContext, replacement, Owner);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
