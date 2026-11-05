using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Fasting's drawback: at the start of each turn the owner loses Amount energy, right after the recharge.</summary>
public sealed class Sts1FastingPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(NoEnergyGainPower);

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}
		Flash();
		await PlayerCmd.LoseEnergy(Amount, player);
	}
}
