using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Ui;

namespace SlaySpoon.Config;

/// <summary>
/// Starting Chest (Custom Run): a Neow option, like Draft's, opens a chest of <see cref="StartingChestRule.Size"/>
/// common, uncommon and rare relics from the player's own grab bag, and the player keeps one through the vanilla
/// synchronized "Choose a Relic" screen. The starter relic stays. The chest only reads the bag: the kept relic leaves
/// it (and the shared bag), the others stay where they were. Rolls use a per-player stream named after this modifier,
/// so vanilla seeds are untouched.
/// </summary>
public sealed class SpoonStartingChest : ModifierModel
{
	private delegate List<RelicModel>? DequeGetter(RelicGrabBag bag, RelicRarity rarity, IRunState run, Func<RelicModel, bool> filter);

	private static readonly DequeGetter AvailableDeque =
		AccessTools.MethodDelegate<DequeGetter>(AccessTools.Method(typeof(RelicGrabBag), "GetAvailableDeque"));

	public override LocString NeowOptionTitle => new("modifiers", Id.Entry + ".neow_title");
	public override LocString NeowOptionDescription => new("modifiers", Id.Entry + ".neow_description");

	public override Func<Task>? GenerateNeowOption(EventModel eventModel) =>
		eventModel.Owner is { } owner ? () => OpenChest(owner) : null;

	private async Task OpenChest(Player player)
	{
		var rng = new Rng(player, Id);
		List<RelicModel> relics = StartingChestRule.Draw<RelicModel>(StartingChestRule.Size, shown => Next(player, rng, shown));
		SpoonLog.Info($"starting chest: player {player.NetId} sees {string.Join(", ", relics.Select(r => r.Id.Entry))}");
		if (relics.Count == 0) return;
		if (LocalContext.IsMe(player)) await ChestOpeningOverlay.Play(player.RunState.Act);
		RelicModel? chosen = await RelicSelectCmd.FromChooseARelicScreen(player, relics);
		if (chosen == null) return;
		player.RelicGrabBag.Remove(chosen);
		player.RunState.SharedRelicGrabBag.Remove(chosen);
		await RelicCmd.Obtain(chosen.ToMutable(), player);
	}

	private static RelicModel? Next(Player player, Rng rng, IReadOnlyCollection<RelicModel> shown)
	{
		RelicRarity rarity = RelicFactory.RollRarity(rng);
		bool Allowed(RelicModel r) => !shown.Any(s => s.Id == r.Id) && r.IsAllowedAtNeow(player);
		return AvailableDeque(player.RelicGrabBag, rarity, player.RunState, Allowed)?.FirstOrDefault(Allowed);
	}
}
