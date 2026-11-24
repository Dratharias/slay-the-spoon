using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Core;
using SlaySpoon.Relics;
using SlaySpoon.Ui;

namespace SlaySpoon.Curses;

/// <summary>
/// The Witching Pact (<see cref="WitchingPactRule"/>), Cursed Deck's Neow option without Overcursed: a chest of
/// <see cref="WitchingPactRule.Size"/> boss relics from the boss chest pool on the vanilla synchronized "Choose a Relic"
/// screen. Taking a relic makes the Witching Hour replace Ascender's Bane (or join the deck); skipping refuses the pact.
/// The chest rolls on a per-player stream from the run seed, so vanilla seeds are untouched.
/// </summary>
public static class WitchingPact
{
	public static async Task Open(Player player)
	{
		var run = player.RunState;
		var rng = new Rng(run.Rng.Seed, WitchingPactRule.Stream(run.GetPlayerSlotIndex(player)));
		List<RelicModel> candidates = BossChest.Candidates(run, [player]);
		List<RelicModel> relics = Enumerable.Range(0, candidates.Count).ToList().StableShuffle(rng)
			.Take(WitchingPactRule.Size).Select(i => candidates[i]).ToList();
		SpoonLog.Info($"witching pact: player {player.NetId} sees {string.Join(", ", relics.Select(r => r.Id.Entry))}");
		if (relics.Count == 0) return;
		if (LocalContext.IsMe(player)) await ChestOpeningOverlay.Play(run.Act);
		RelicModel? chosen = await RelicSelectCmd.FromChooseARelicScreen(player, relics);
		if (chosen == null)
		{
			SpoonLog.Info($"witching pact: player {player.NetId} refuses");
			return;
		}
		var bane = player.Deck.Cards.LastOrDefault(c => c is AscendersBane);
		var deal = WitchingPactRule.Deal(bane != null);
		if (bane != null) await CardPileCmd.RemoveFromDeck(bane);
		var curse = run.CreateCard<SpoonWitchingHour>(player);
		var added = await CardPileCmd.Add(curse, PileType.Deck);
		if (LocalContext.IsMe(player)) CardCmd.PreviewCardPileAdd(added);
		await RelicCmd.Obtain(chosen.ToMutable(), player);
		SpoonLog.Info($"witching pact: player {player.NetId} takes {chosen.Id.Entry} ({deal})");
	}
}
