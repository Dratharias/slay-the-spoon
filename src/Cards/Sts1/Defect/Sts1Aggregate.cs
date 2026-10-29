using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1Aggregate : Sts1DefectCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("CardsPerEnergy", 4m),
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { EnergyHoverTip };

	public Sts1Aggregate()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayerCmd.GainEnergy(PileType.Draw.GetPile(Owner).Cards.Count / DynamicVars["CardsPerEnergy"].IntValue, Owner);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["CardsPerEnergy"].UpgradeValueBy(-1m);
	}
}
