using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Events;

/// <summary>StS1's Golden Idol, from the Golden Idol event: combat gold rewards are 25% larger.</summary>
public sealed class Sts1GoldenIdol : Sts1EventRelic
{
	private static readonly AccessTools.FieldRef<GoldReward, bool> StolenBack =
		AccessTools.FieldRefAccess<GoldReward, bool>("_wasGoldStolenBack");

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Percent", 25m) };

	/// <summary>
	/// Late, so gold other relics added is boosted too, as StS1 boosted every gold reward of the room. Gold a thief
	/// stole and gives back stays as it was.
	/// </summary>
	public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (player != Owner || room == null || !room.RoomType.IsCombatRoom())
		{
			return false;
		}
		bool changed = false;
		for (int i = 0; i < rewards.Count; i++)
		{
			if (rewards[i] is GoldReward { IsPopulated: true } gold && !StolenBack(gold))
			{
				rewards[i] = new GoldReward(Sts1EventMath.GoldenIdolGold(gold.Amount), player);
				changed = true;
			}
		}
		return changed;
	}

	public override Task AfterModifyingRewards()
	{
		Flash();
		return Task.CompletedTask;
	}
}
