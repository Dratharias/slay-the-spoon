using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Relics.Watcher;

public sealed class Sts1HolyWater : Sts1WatcherRelic
{
	public override bool HasUponPickupEffect => true;
	public override RelicRarity Rarity => RelicRarity.Event;
	// Orobas fallback and the StS1 boss chest only: excluded from all random rewards, pools and shops.
	public override bool IsAllowed(IRunState runState) => false;
	public override bool IsAllowedInShops => false;
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(3) };
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
	public override async Task AfterObtained()
	{
		if (Owner.GetRelic<Sts1PureWater>() is { } pureWater)
			await RelicCmd.Remove(pureWater);
	}
}
