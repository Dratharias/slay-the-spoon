using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Midas;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Config;

/// <summary>
/// King Midas (Custom Run, docs/superpowers/specs/2026-11-14-roi-midas-design.md): every gold relic from the start,
/// no card rewards, gold events only, and rest site actions bought with gold (repeatable, the price rising at each fire).
/// Balance (1.8.4): gold melts on every floor and a player left with none dies (<see cref="MidasMelt"/>; since 1.8.32 a
/// ruined co-op player comes straight back with 1 gold and 1 HP for the next room); merchant
/// prices rise with each purchase; the merchant and the rest site refuse what would ruin the buyer.
/// </summary>
public sealed class SpoonRoiMidas : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonRoiMidas);

	public static SpoonRoiMidas? Of(IRunState? run) => run?.Modifiers.OfType<SpoonRoiMidas>().FirstOrDefault();

	/// <summary>2 when vanilla Midas is ticked too (<see cref="MidasRule.Scale"/>).</summary>
	public static int Scale(IRunState? run) =>
		MidasRule.Scale(run?.Modifiers.OfType<MegaCrit.Sts2.Core.Models.Modifiers.Midas>().Any() == true);

	/// <summary>Actions each player paid for at the current rest site, by net id. Not saved: a reload resets the price.</summary>
	private Dictionary<ulong, int> _paidHere = new();

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_paidHere = new Dictionary<ulong, int>();
		_boughtHere = new Dictionary<ulong, int>();
	}

	public int PaidHere(Player player) => _paidHere.GetValueOrDefault(player.NetId);

	public void Paid(Player player) => _paidHere[player.NetId] = PaidHere(player) + 1;

	/// <summary>Items each player bought at the current merchant, by net id. Not saved: a reload resets the rise.</summary>
	private Dictionary<ulong, int> _boughtHere = new();

	// Rule 1.
	protected override void AfterRunCreated(RunState runState)
	{
		// Pickup effects (gold, Cursed Pearl's Greed) come from vanilla: RunManager.FinalizeStartingRelics runs
		// AfterObtained on every relic the players hold when the run starts, these included.
		foreach (Player player in runState.Players)
		{
			foreach (RelicModel canonical in MidasStart.Relics())
			{
				player.RelicGrabBag.Remove(canonical);
				if (player.Relics.Any(r => r.Id == canonical.Id)) continue;
				player.AddRelicInternal(canonical.ToMutable());
			}
		}
		foreach (RelicModel canonical in MidasStart.Relics())
			runState.SharedRelicGrabBag.Remove(canonical);
	}

	// Rule 2. Late, so card rewards other models add (Prayer Wheel, Amethyst-like relics) go too. Neow keeps its own.
	public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (AtAncient(RunState)) return false;
		return rewards.RemoveAll(r => r is CardReward or SpecialCardReward) > 0;
	}

	internal static bool AtAncient(IRunState run) => run.CurrentMapPoint?.PointType == MapPointType.Ancient;

	// Rule 3.
	public override EventModel ModifyNextEvent(EventModel currentEvent) => MidasEvents.Draw(RunState, currentEvent);

	// Rule 4 and the paid rest site.
	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is RestSiteRoom) _paidHere.Clear();
		if (room is MerchantRoom) _boughtHere.Clear();
		return Task.CompletedTask;
	}

	// Each purchase at this merchant raises the price of what is left.
	public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost) =>
		cost * MidasRule.PriceFactor(_boughtHere.GetValueOrDefault(player.NetId), Scale(RunState));

	public override Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
	{
		if (goldSpent > 0) _boughtHere[player.NetId] = _boughtHere.GetValueOrDefault(player.NetId) + 1;
		// Deferred: the Maw Bank breaks in its own AfterItemPurchased, which may run after this one.
		Godot.Callable.From(Midas.MidasBankReopen.NotifyChanged).CallDeferred();
		return Task.CompletedTask;
	}

	/// <summary>
	/// A paid action does not close the fire: the options come back (<c>MidasRestChoosePatch</c>), so the vanilla
	/// synchronizer must not mark the player's rest site completed.
	/// </summary>
	public override bool ShouldDisableRemainingRestSiteOptions(Player player) => false;

	/// <summary>Purchases made by <paramref name="player"/> at the current merchant (King Midas price rise).</summary>
	public int BoughtHere(Player player) => _boughtHere.GetValueOrDefault(player.NetId);
}

/// <summary>The King Midas starting relics, in grant order.</summary>
internal static class MidasStart
{
	public static IEnumerable<RelicModel> Relics() =>
	[
		ModelDb.Relic<BowlerHat>(), ModelDb.Relic<AmethystAubergine>(), ModelDb.Relic<LuckyFysh>(), ModelDb.Relic<MawBank>(),
		ModelDb.Relic<Sts1GoldenIdol>(), ModelDb.Relic<OldCoin>(), ModelDb.Relic<GoldenPearl>(), ModelDb.Relic<SignetRing>(),
		ModelDb.Relic<CursedPearl>(), ModelDb.Relic<MembershipCard>(), ModelDb.Relic<TheCourier>(),
	];
}
