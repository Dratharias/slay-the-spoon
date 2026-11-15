using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Config;

namespace SlaySpoon.Midas;

/// <summary>
/// King Midas rule 2 for card picks outside combat (events, relics such as Massive Scroll or Lead Paperweight): the
/// screen never opens and nothing is gained. Neow is kept. Runs first, so the mod's version-choice and Folie à deux
/// patches see an empty pick.
/// </summary>
public static class MidasCardPicks
{
	public static bool Blocks(Player player) =>
		SpoonRoiMidas.IsActive(player.RunState) && !CombatManager.Instance.IsInProgress && !SpoonRoiMidas.AtAncient(player.RunState);
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGridForRewards))]
[HarmonyPriority(Priority.First)]
public static class MidasGridPickPatch
{
	public static bool Prefix(Player player, ref Task<IEnumerable<CardModel>> __result)
	{
		if (!MidasCardPicks.Blocks(player)) return true;
		__result = Task.FromResult<IEnumerable<CardModel>>([]);
		return false;
	}
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen))]
[HarmonyPriority(Priority.First)]
public static class MidasChooseACardPickPatch
{
	public static bool Prefix(Player player, ref Task<CardModel?> __result)
	{
		if (!MidasCardPicks.Blocks(player)) return true;
		__result = Task.FromResult<CardModel?>(null);
		return false;
	}
}
