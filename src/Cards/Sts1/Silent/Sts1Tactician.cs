using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Silent;

/// <summary>StS1 Tactician: Unplayable, gains energy when discarded by a card; StS2's is a Sly card that costs 3.</summary>
public sealed class Sts1Tactician : Sts1SilentCard
{
	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(Tactician);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(1),
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { EnergyHoverTip };

	public Sts1Tactician()
		: base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
	{
	}

	// The game raises this hook for discards made by cards and effects, never for the end-of-turn hand discard.
	public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
	{
		if (card == this)
		{
			await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
