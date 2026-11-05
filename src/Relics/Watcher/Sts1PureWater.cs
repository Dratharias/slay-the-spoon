using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Relics.Watcher;

public sealed class Sts1PureWater : Sts1WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Starter;
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(1) };
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Sts1Miracle>() };
	public override async Task BeforeCombatStart()
	{
		Flash();
		for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
		{
			CardModel card = Owner.Creature.CombatState!.CreateCard<Sts1Miracle>(Owner);
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
		}
	}
}
