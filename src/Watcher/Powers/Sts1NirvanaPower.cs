using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Nirvana: whenever the owner Scries, gain Amount Block.</summary>
public sealed class Sts1NirvanaPower : PowerModel, IScryListener, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(ToricToughnessPower);

	public async Task AfterScry(Player player, int discarded)
	{
		if (player != Owner.Player)
		{
			return;
		}
		Flash();
		await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
	}
}
