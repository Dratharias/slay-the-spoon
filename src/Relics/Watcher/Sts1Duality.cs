using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Relics.Watcher;

public sealed class Sts1Duality : Sts1WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack || CombatManager.Instance.IsOverOrEnding) return;
		Flash();
		await PowerCmd.Apply<Sts1DualityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
	}
}
