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

/// <summary>
/// StS1 Deva Form: at the start of the owner's turn gain energy, then raise the next gain by Amount. Each further Deva Form
/// raises both Amount and the current gain by 1, as StS1 does.
/// </summary>
public sealed class Sts1DevaFormPower : PowerModel, IBorrowedIcon
{
	private const string GainKey = "EnergyGain";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(EnergyNextTurnPower);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar(GainKey, 1m),
	};

	/// <summary>A second Deva Form: the energy the next turn gives goes up by 1 on top of the stacked Amount.</summary>
	public void RaiseGain()
	{
		AssertMutable();
		DynamicVars[GainKey].BaseValue++;
	}

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}
		Flash();
		int gain = DynamicVars[GainKey].IntValue;
		await PlayerCmd.GainEnergy(gain, player);
		DynamicVars[GainKey].BaseValue = WatcherPowerMath.DevaNextGain(gain, Amount);
	}
}
