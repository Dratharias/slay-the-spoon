using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Blood;

namespace SlaySpoon.Config;

/// <summary>
/// Blood (Custom Run, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): no gold; every gold price is paid in
/// current HP (<see cref="BloodGold"/>), and every player starts with Blood Fang.
/// </summary>
public sealed class SpoonBlood : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonBlood);

	protected override void AfterRunCreated(RunState runState)
	{
		var fang = ModelDb.Relic<SpoonBloodFang>();
		foreach (Player player in runState.Players)
		{
			BloodGold.Mirror(player);
			player.RelicGrabBag.Remove(fang);
			if (player.Relics.All(r => r.Id != fang.Id)) player.AddRelicInternal(fang.ToMutable());
		}
		runState.SharedRelicGrabBag.Remove(fang);
	}

	/// <summary>The real gold field follows the spendable HP (<see cref="BloodGold.Mirror"/>).</summary>
	protected override void AfterRunLoaded(RunState runState)
	{
		foreach (Player player in runState.Players) BloodGold.Mirror(player);
	}

	public override decimal ModifyGoldGained(Player player, decimal amount) => 0m;

	public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room) =>
		rewards.RemoveAll(r => r is GoldReward) > 0;
}
