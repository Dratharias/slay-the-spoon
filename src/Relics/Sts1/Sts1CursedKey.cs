using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;

namespace SlaySpoon.Relics.Sts1;

/// <summary>StS2 has no boss chests (Ancients replace them), so every treasure room counts as StS1's non-boss chest.</summary>
public sealed class Sts1CursedKey : Sts1EnergyBossRelic
{
	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is not TreasureRoom || Owner.Creature.IsDead) return;
		// As SereTalon picks its curses.
		var curses = ModelDb.CardPool<CurseCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
			.Where(c => c.CanBeGeneratedByModifiers).OrderBy(c => c.Id).ToList();
		if (curses.Count == 0) return;
		Flash();
		var curse = Owner.RunState.Rng.Niche.NextItem(curses);
		if (curse == null) return;
		var card = Owner.RunState.CreateCard(curse, Owner);
		CardCmd.PreviewCardPileAdd(new List<CardPileAddResult> { await CardPileCmd.Add(card, PileType.Deck) }, 2f);
	}
}
