using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Cards.Sts1.Curses;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Events;

/// <summary>
/// StS1 Necronomicon (Cursed Tome): the first Attack each turn that costs 2 or more (an X cost paid with 2 or more) is
/// played twice. Obtaining it adds a Necronomicurse; losing it removes one.
/// </summary>
public sealed class Sts1Necronomicon : Sts1EventRelic
{
	private bool _usedThisTurn;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Cost", Sts1EventMath.NecronomiconCost) };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Sts1Necronomicurse>() };

	private bool UsedThisTurn
	{
		get => _usedThisTurn;
		set
		{
			AssertMutable();
			_usedThisTurn = value;
		}
	}

	public override async Task AfterObtained()
	{
		await CardPileCmd.AddCurseToDeck<Sts1Necronomicurse>(Owner);
	}

	public override async Task AfterRemoved()
	{
		if (Owner.Deck.Cards.FirstOrDefault(c => c is Sts1Necronomicurse) is { } curse)
		{
			await CardPileCmd.RemoveFromDeck(curse);
		}
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom) Ready();
		return Task.CompletedTask;
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == Owner) Ready();
		return Task.CompletedTask;
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (UsedThisTurn || card.Owner != Owner || card.Type != CardType.Attack) return playCount;
		int paid = card.EnergyCost.CostsX ? card.EnergyCost.CapturedXValue : card.EnergyCost.GetResolved();
		return Sts1EventMath.NecronomiconTriggers(paid) ? playCount + 1 : playCount;
	}

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		UsedThisTurn = true;
		Flash();
		Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		UsedThisTurn = false;
		Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	private void Ready()
	{
		UsedThisTurn = false;
		Status = RelicStatus.Active;
	}
}
