using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Characters.Pools;

namespace SlaySpoon.Cards;

/// <summary>
/// A Watcher StS1 card. StS2 has no Watcher, so the logic pool and the visual pool are both ours (purple frame
/// and Necrobinder energy colours, see <see cref="Sts1WatcherCardPool"/>).
/// </summary>
public abstract class Sts1WatcherCard : Sts1Card
{
	protected Sts1WatcherCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}

	public override CardPoolModel Pool => ModelDb.CardPool<Sts1WatcherCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<Sts1WatcherCardPool>();

	protected override string PortraitFolder => "watcher";

	/// <summary>Flurry of Blows and Weave: back to the hand from the discard pile when the trigger is the owner's.</summary>
	protected async Task ReturnFromDiscard(Player player)
	{
		if (player == Owner && Pile?.Type == PileType.Discard)
		{
			await CardPileCmd.Add(this, PileType.Hand);
		}
	}

	/// <summary>Sands of Time and Windmill Strike: true when the owner kept this very card through a flush.</summary>
	protected bool RetainedBy(Player player, IReadOnlyCollection<CardModel> retainedCards) =>
		player == Owner && retainedCards.Contains(this);
}
