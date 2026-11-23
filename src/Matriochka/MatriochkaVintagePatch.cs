using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Matriochka;

/// <summary>
/// Vanilla Vintage turns a normal combat's card rewards into relic rewards, which left Matriochka floors without their
/// card. With Matriochka, every replaced card reward comes back, unskippable, next to its relic. A postfix on Vintage
/// itself, so the order of the two modifiers' late reward hooks does not matter.
/// </summary>
[HarmonyPatch(typeof(Vintage), nameof(Vintage.TryModifyRewardsLate))]
public static class MatriochkaVintagePatch
{
	public static void Prefix(List<Reward> rewards, out int __state) => __state = rewards.OfType<CardReward>().Count();

	public static void Postfix(Player player, List<Reward> rewards, AbstractRoom? room, int __state)
	{
		int count = MatriochkaRule.CardsAfterVintage(SpoonMatriochka.Of(player.RunState) != null, __state, rewards.OfType<CardReward>().Count());
		if (count == 0 || room == null)
			return;
		for (int i = 0; i < count; i++)
		{
			var options = CardCreationOptions.ForRoom(player, room.RoomType).WithFlags(CardCreationFlags.IsFromCombat);
			rewards.Add(new CardReward(options, 3, player) { CanSkip = false });
		}
		SpoonLog.Info($"matriochka: Vintage replaced {count} card reward(s) for player {player.NetId}, card put back");
	}
}
