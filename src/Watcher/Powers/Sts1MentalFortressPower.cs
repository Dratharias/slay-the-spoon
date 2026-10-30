using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Mental Fortress: whenever the owner changes stance, gain Amount Block.</summary>
public sealed class Sts1MentalFortressPower : PowerModel, IStanceListener, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(UnmovablePower);

	public async Task AfterStanceChanged(PlayerChoiceContext choiceContext, Player player, Stance from, Stance to)
	{
		if (player != Owner.Player || from == to)
		{
			return;
		}
		Flash();
		await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
	}
}
