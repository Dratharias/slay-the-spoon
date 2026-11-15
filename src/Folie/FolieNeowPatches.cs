using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;

namespace SlaySpoon.Folie;

/// <summary>
/// Folie à deux over the deck-clearing Neow options. Draft: each of the ten rounds is a chest over every player's
/// three cards, with consolation (every player takes one stake), then the player picks from what they were awarded.
/// Sealed Deck: each player picks their ten cards as vanilla, then one chest with consolation hands out the decks.
/// </summary>
public static class FolieNeow
{
	private static readonly Func<CardModel, CardModel, int> CompareSealed =
		AccessTools.MethodDelegate<Func<CardModel, CardModel, int>>(AccessTools.Method(typeof(SealedDeck), "CompareCards"));

	public static async Task Draft(Player player)
	{
		IRunState run = player.RunState;
		var options = new CardCreationOptions([player.Character.CardPool], CardCreationSource.Other, CardRarityOddsType.RegularEncounter)
			.WithFlags(CardCreationFlags.NoUpgradeRoll);
		for (int i = 0; i < 10; i++)
		{
			var reward = new CardReward(options, 3, player) { CanSkip = false };
			reward.Populate();
			var award = await FolieRendezvous.Join(run, $"draft_{i}", player, reward.Cards.ToList(), reward, consolation: true);
			if (award is not { Tag: CardReward won }) continue;
			CardReward mine = won.Player == player ? won : FolieRewards.Rebuild(run, won, player);
			await mine.SelectUnsynchronized();
		}
		RemovePandora(run);
	}

	public static async Task Sealed(Player player)
	{
		IRunState run = player.RunState;
		var options = new CardCreationOptions([player.Character.CardPool], CardCreationSource.Other, CardRarityOddsType.RegularEncounter)
			.WithFlags(CardCreationFlags.NoUpgradeRoll | CardCreationFlags.ForceRarityOddsChange | CardCreationFlags.IsCardReward);
		var source = CardFactory.CreateForReward(player, 30, options).ToList();
		var prefs = new CardSelectorPrefs(new LocString("modifiers", "SEALED_DECK.selectionPrompt"), 10)
		{
			Cancelable = false,
			RequireManualConfirmation = true,
			Comparison = (a, b) => CompareSealed(a, b),
		};
		Task<IEnumerable<CardModel>> pick;
		FolieCardPicks.Bypass = true;
		try
		{
			pick = CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(), source, player, prefs);
		}
		finally
		{
			FolieCardPicks.Bypass = false;
		}
		var picked = (await pick).ToList();
		var award = await FolieRendezvous.Join(run, "sealed", player, picked, picked, consolation: true);
		if (award is { Tag: List<CardModel> deck })
		{
			FolieRendezvous.GiveCards(run, deck, player);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(deck, PileType.Deck), 1.2f, CardPreviewStyle.GridLayout);
		}
		RemovePandora(run);
	}

	private static void RemovePandora(IRunState run)
	{
		foreach (Player p in run.Players) p.RelicGrabBag.Remove<PandorasBox>();
		run.SharedRelicGrabBag.Remove<PandorasBox>();
	}
}

[HarmonyPatch(typeof(Draft), "OfferRewards")]
public static class FolieDraftPatch
{
	public static bool Prefix(Player player, ref Task __result)
	{
		if (!SpoonFolieADeux.IsActive(player.RunState) || player.RunState.Players.Count < 2) return true;
		__result = FolieNeow.Draft(player);
		return false;
	}
}

[HarmonyPatch(typeof(SealedDeck), "ChooseCards")]
public static class FolieSealedPatch
{
	public static bool Prefix(Player player, ref Task __result)
	{
		if (!SpoonFolieADeux.IsActive(player.RunState) || player.RunState.Players.Count < 2) return true;
		__result = FolieNeow.Sealed(player);
		return false;
	}
}
