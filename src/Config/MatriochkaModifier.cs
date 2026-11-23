using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Matriochka;

namespace SlaySpoon.Config;

/// <summary>
/// Matriochka (Custom Run, docs/superpowers/specs/2026-11-14-matriochka-design.md): vanilla Hoarder, an unskippable
/// card reward on every floor, Dolly's Mirror on the act 3 Reflections row, right before Shatter's Bad Luck (owner, 2026-11-19), the Doll Room (Bing Bong only) as the first act 2
/// event, and a whole act 3 row of Reflections (Touch a Mirror only: Shatter is forbidden, owner 2026-11-23) before the final rest sites.
/// The deck stops growing at <see cref="MatriochkaRule.DeckLimit"/> cards: no more copies, floor cards or combat card rewards. The row's −/+ value is the
/// number of card reward rerolls each player gets per act (one sealed class per value, like the percent rows).
/// </summary>
public abstract class SpoonMatriochka : SpoonPercentModifier
{
	public const string LocKey = "SPOON_MATRIOCHKA";

	protected override string FamilyKey => LocKey;

	public override int Step(int direction) => MatriochkaRule.StepRerolls(Percent, direction);

	/// <summary>Rerolls per player per act.</summary>
	public int RerollsPerAct => Percent;

	private HashSet<CardModel> _cardsToSkip = new();
	private MatriochkaRerolls _rerolls = new();

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_cardsToSkip = new HashSet<CardModel>();
		_rerolls = new MatriochkaRerolls();
	}

	// Rerolls used this act, under a vanilla saved-property name (as SpoonCampfireGift does).
	[SavedProperty]
	public int[] FurCoatCoordCols
	{
		get => _rerolls.Encode();
		set => _rerolls = MatriochkaRerolls.Decode(value);
	}

	/// <summary>The vanilla reroll button, while the reward's owner has rerolls left this act. Picking it runs on
	/// every client (the choice index is synced), so the count stays the same everywhere.</summary>
	public override bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
	{
		if (cardReward.Player != player || cardReward.CanReroll || alternatives.Count >= 2)
			return false;
		int index = player.RunState.Players.ToList().IndexOf(player);
		int left = _rerolls.Remaining(index, RunState.CurrentActIndex, RerollsPerAct);
		if (left <= 0)
			return false;
		alternatives.Add(new CardRewardAlternative(MatriochkaRule.RerollOptionId(left), () =>
		{
			if (_rerolls.Use(index, RunState.CurrentActIndex, RerollsPerAct, RunState.Players.Count))
				cardReward.Reroll();
			return Task.CompletedTask;
		}, PostAlternateCardRewardAction.DoNothing));
		return true;
	}

	public static SpoonMatriochka? Of(IRunState? run) => run?.Modifiers.OfType<SpoonMatriochka>().FirstOrDefault();

	// Rule 1: vanilla Hoarder's body; when Hoarder is also ticked it already does this.
	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (oldPileType != PileType.None || RunState.Modifiers.Any(m => m is Hoarder))
			return;
		CardPile? pile = card.Pile;
		if (pile == null || pile.Type != PileType.Deck || clonedBy != null || _cardsToSkip.Remove(card))
			return;
		int copies = MatriochkaRule.CopiesFor(card.Owner.Deck.Cards.Count);
		for (int i = 0; i < copies; i++)
		{
			CardModel copy = card.Owner.RunState.CloneCard(card);
			_cardsToSkip.Add(copy);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(copy, PileType.Deck, CardPilePosition.Bottom, this));
		}
	}

	public override bool ShouldAllowMerchantCardRemoval(Player player) => false;

	// Rule 2 (combat rooms): the room's card rewards cannot be skipped.
	public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (room is not CombatRoom)
			return false;
		// Rule 8: a full deck (999 cards) gets no card reward at all.
		if (MatriochkaRule.DeckFull(player.Deck.Cards.Count))
			return rewards.RemoveAll(r => r is CardReward) > 0;
		bool changed = false;
		foreach (CardReward reward in rewards.OfType<CardReward>())
			changed |= MatriochkaRewards.MakeMandatory(reward);
		return changed;
	}

	// Rules 2 (other rooms) and 3.
	public override Task AfterRoomEntered(AbstractRoom room)
	{
		bool ancient = RunState.CurrentMapPoint?.PointType == MapPointType.Ancient;
		bool offers = MatriochkaRule.OffersFloorCard(room is MerchantRoom or RestSiteRoom or TreasureRoom, room is EventRoom, ancient, room.IsVictoryRoom);
		// Rule 3: Dolly's Mirror when the act 3 Reflections room opens, before its Shatter.
		bool mirror = room is EventRoom && RunState.CurrentActIndex == MatriochkaRule.ReflectionsAct && OnReflectionsRow();
		if (!offers && !mirror)
			return Task.CompletedTask;
		// Rule 7: with Folie à deux, the floor's rewards go through one chest.
		if (SpoonFolieADeux.IsActive(RunState))
		{
			TaskHelper.RunSafely(Folie.FolieRewards.OfferFloors(RunState, mirror, offers));
			return Task.CompletedTask;
		}
		foreach (Player player in RunState.Players)
			TaskHelper.RunSafely(MatriochkaRewards.OfferFloor(player, mirror, offers));
		return Task.CompletedTask;
	}


	// Rules 4 and 5.
	public override EventModel ModifyNextEvent(EventModel currentEvent)
	{
		if (RunState.CurrentActIndex == MatriochkaRule.ReflectionsAct && OnReflectionsRow())
			return ModelDb.Event<Reflections>();
		if (RunState.CurrentActIndex == MatriochkaRule.DollRoomAct && !RunState.VisitedEventIds.Contains(ModelDb.Event<DollRoom>().Id)
			&& !EventVisitedThisAct())
			return ModelDb.Event<DollRoom>();
		return currentEvent;
	}

	private bool EventVisitedThisAct()
	{
		var history = RunState.MapPointHistory;
		if (history.Count <= RunState.CurrentActIndex)
			return false;
		return history[RunState.CurrentActIndex].Any(e => e.MapPointType != MapPointType.Ancient && e.HasRoomOfType(RoomType.Event));
	}

	public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
	{
		if (RunState.CurrentActIndex != MatriochkaRule.ReflectionsAct || !OnReflectionsRow())
			return roomTypes;
		return new HashSet<RoomType> { RoomType.Event };
	}

	public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
	{
		if (actIndex != MatriochkaRule.ReflectionsAct || ReflectionsRow(map) is not int row)
			return map;
		foreach (MapPoint point in map.GetPointsInRow(row))
		{
			if (point.CanBeModified)
				point.PointType = MapPointType.Unknown;
		}
		return map;
	}

	private bool OnReflectionsRow()
	{
		MapCoord? coord = RunState.CurrentMapCoord;
		return coord.HasValue && ReflectionsRow(RunState.Map) == coord.Value.row;
	}

	private static int? ReflectionsRow(ActMap map)
	{
		var rows = Enumerable.Range(0, map.GetRowCount())
			.Select(r => (IReadOnlyList<bool>)map.GetPointsInRow(r).Select(p => p.PointType == MapPointType.RestSite).ToList())
			.ToList();
		return MatriochkaRule.ReflectionsRow(rows);
	}
}

public sealed class SpoonMatriochka0 : SpoonMatriochka { }
public sealed class SpoonMatriochka1 : SpoonMatriochka { }
public sealed class SpoonMatriochka2 : SpoonMatriochka { }
public sealed class SpoonMatriochka3 : SpoonMatriochka { }
public sealed class SpoonMatriochka4 : SpoonMatriochka { }
public sealed class SpoonMatriochka5 : SpoonMatriochka { }
public sealed class SpoonMatriochka6 : SpoonMatriochka { }
public sealed class SpoonMatriochka7 : SpoonMatriochka { }
public sealed class SpoonMatriochka8 : SpoonMatriochka { }
public sealed class SpoonMatriochka9 : SpoonMatriochka { }
public sealed class SpoonMatriochka10 : SpoonMatriochka { }
