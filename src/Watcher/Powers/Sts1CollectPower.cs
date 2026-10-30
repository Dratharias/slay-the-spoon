using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Cards.Sts1.WatcherTokens;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Collect: at the start of each of the next Amount turns, an upgraded Miracle goes into the owner's hand.</summary>
public sealed class Sts1CollectPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(ClarityPower);

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}
		Flash();
		Sts1Miracle miracle = CombatState.CreateCard<Sts1Miracle>(player);
		CardCmd.Upgrade(miracle);
		await CardPileCmd.AddGeneratedCardToCombat(miracle, PileType.Hand, player);
		await PowerCmd.Decrement(this);
	}
}
