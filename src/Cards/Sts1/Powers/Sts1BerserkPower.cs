using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>At the start of the turn, before the hand is drawn, gain Amount energy.</summary>
public sealed class Sts1BerserkPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(EnergyNextTurnPower);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.ForEnergy(this) };

	public override async Task AfterEnergyReset(Player player)
	{
		if (player == Owner.Player)
		{
			Flash();
			await PlayerCmd.GainEnergy(Amount, player);
		}
	}
}
