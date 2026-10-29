using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1Recycle : Sts1DefectCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { EnergyHoverTip, HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

	public Sts1Recycle()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel? chosen = (await CardSelectCmd.FromHand(choiceContext, Owner,
			new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), null, this)).FirstOrDefault();
		if (chosen == null)
		{
			return;
		}
		int energy = chosen.EnergyCost.GetAmountToSpend();
		await CardCmd.Exhaust(choiceContext, chosen);
		await PlayerCmd.GainEnergy(energy, Owner);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
