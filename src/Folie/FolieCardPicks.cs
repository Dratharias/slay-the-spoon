using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Folie;

/// <summary>
/// Folie à deux on card picks outside combat that are not card rewards: event screens (Brain Leech, Room Full of
/// Cheese) and relic pickups (Massive Scroll, Hefty Tablet, Lead Paperweight, Sea Glass). The owner picks as vanilla;
/// what they picked is the stake of a one-head chest every living player bids on. The cards then change owner, so the
/// source's own "add to deck" puts them in the winner's deck; an unclaimed pick is lost (the source gets nothing).
/// Combat picks (potions, Discovery, Toolbox...) go to the hand and stay vanilla.
/// </summary>
public static class FolieCardPicks
{
	/// <summary>Set while the mod's own Sealed Deck screen opens: that pick goes through its own chest.</summary>
	internal static bool Bypass;

	internal static bool Applies(Player player) =>
		!Bypass && SpoonFolieADeux.IsActive(player.RunState) && !CombatManager.Instance.IsInProgress
		&& player.RunState.Players.Count(p => !p.Creature.IsDead) >= 2 && !player.Creature.IsDead;

	/// <returns>Who takes the picked cards: the owner, another player, or null (lost).</returns>
	private static async Task<Player?> Bid(Player owner, IReadOnlyList<CardModel> cards)
	{
		IRunState run = owner.RunState;
		var living = run.Players.Where(p => !p.Creature.IsDead).ToList();
		try
		{
			FolieOutcome outcome = await FolieChest.Run(run, FolieCustomRewards.NextKey(run, owner, "pick"), living,
				[new FolieStake(owner, cards)], consolation: false);
			Player? winner = outcome.Awards.Select(a => (FolieAward?)a).FirstOrDefault() is { } a ? living[a.Player] : null;
			if (winner != null && winner != owner) FolieRendezvous.GiveCards(run, cards, winner);
			return winner;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: card pick chest failed, the pick stays with its owner: {e}");
			return owner;
		}
	}

	internal static async Task<IEnumerable<CardModel>> Many(Task<IEnumerable<CardModel>> pick, Player owner)
	{
		var cards = (await pick).ToList();
		if (cards.Count == 0) return cards;
		return await Bid(owner, cards) == null ? [] : cards;
	}

	internal static async Task<CardModel?> One(Task<CardModel?> pick, Player owner)
	{
		CardModel? card = await pick;
		if (card == null) return null;
		return await Bid(owner, [card]) == null ? null : card;
	}
}

/// <summary>Runs after the mod's version-choice postfix, so the stake is the version the owner kept.</summary>
[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGridForRewards))]
[HarmonyPriority(Priority.Last)]
public static class FolieGridPickPatch
{
	public static void Postfix(Player player, ref Task<IEnumerable<CardModel>> __result)
	{
		if (FolieCardPicks.Applies(player)) __result = FolieCardPicks.Many(__result, player);
	}
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen))]
[HarmonyPriority(Priority.Last)]
public static class FolieChooseACardPickPatch
{
	public static void Postfix(Player player, ref Task<CardModel?> __result)
	{
		if (FolieCardPicks.Applies(player)) __result = FolieCardPicks.One(__result, player);
	}
}
