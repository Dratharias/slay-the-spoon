using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Relics.Watcher;

public sealed class Sts1VioletLotus : Sts1WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Event;
	// Orobas fallback and the StS1 boss chest only: excluded from all random rewards, pools and shops.
	public override bool IsAllowed(IRunState runState) => false;
	public override bool IsAllowedInShops => false;
}
