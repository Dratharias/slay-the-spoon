using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Tantrum: several hits, enter Wrath, and the card shuffles itself into the draw pile instead of the discard.</summary>
public sealed class Sts1Tantrum : Sts1WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<Sts1WrathStancePower>() };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(3m, ValueProp.Move),
		new DynamicVar("Magic", 3m),
	};

	public Sts1Tantrum()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars["Magic"].IntValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
		await StanceCmd.Enter(Owner, Stance.Wrath, this, choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}

	// Played normally it would go to the discard pile; StS1 shuffles it into the draw pile instead. Exhaust is left alone.
	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		if (card == this && pileType == PileType.Discard)
		{
			return (PileType.Draw, CardPilePosition.Random);
		}
		return (pileType, position);
	}
}
