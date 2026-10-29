using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1Fission : Sts1DefectCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { EnergyHoverTip, HoverTipFactory.Static(StaticHoverTip.Evoke) };

	public Sts1Fission()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		var queue = Owner.PlayerCombatState!.OrbQueue;
		int count = queue.Orbs.Count;
		for (int i = 0; i < count; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding)
			{
				break;
			}
			if (IsUpgraded)
			{
				await OrbCmd.EvokeNext(choiceContext, Owner);
			}
			else if (queue.Orbs.FirstOrDefault() is { } orb)
			{
				queue.Remove(orb);
				NCombatRoom.Instance?.GetCreatureNode(Owner.Creature)?.OrbManager?.EvokeOrbAnim(orb);
				orb.RemoveInternal();
			}
		}
		await PlayerCmd.GainEnergy(count, Owner);
		await CardPileCmd.Draw(choiceContext, count, Owner);
	}
}
