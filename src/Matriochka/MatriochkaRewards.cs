using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Matriochka;

/// <summary>The rewards Matriochka hands out outside combat.</summary>
public static class MatriochkaRewards
{
	private static readonly AccessTools.FieldRef<CardReward, bool> CanSkip =
		AccessTools.FieldRefAccess<CardReward, bool>("<CanSkip>k__BackingField");

	/// <summary>Removes the reward's Skip option. Returns false when it was already mandatory.</summary>
	public static bool MakeMandatory(CardReward reward)
	{
		if (!reward.CanSkip)
			return false;
		CanSkip(reward) = false;
		return true;
	}

	/// <summary>Dolly's Mirror (act 3 Reflections row, unless already held), then an unskippable card reward of the player's character pool.</summary>
	public static async Task OfferFloor(Player player, bool mirror, bool cards)
	{
		if (player.Creature.IsDead)
			return;
		if (mirror && !player.Relics.Any(r => r is DollysMirror))
			await RelicCmd.Obtain(ModelDb.Relic<DollysMirror>().ToMutable(), player);
		if (!cards)
			return;
		var options = CardCreationOptions.ForNonCombatWithDefaultOdds([player.Character.CardPool]);
		var reward = new CardReward(options, 3, player) { CanSkip = false };
		await new RewardsSet(player).WithCustomRewards([reward]).WithSkippingDisallowed().Offer();
	}
}
