using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Relics.Watcher;

public sealed class Sts1TeardropLocket : Sts1WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1CalmStancePower>() };
	public override async Task BeforeCombatStart()
	{
		Flash();
		// BeforeCombatStart gives no choice context; build the hook's own so a listener (Rushdown's draw) can open a choice.
		var context = new HookPlayerChoiceContext(this, LocalContext.NetId!.Value, Owner.Creature.CombatState!, GameActionType.Combat);
		await context.AssignTaskAndWaitForPauseOrCompletion(StanceCmd.Enter(Owner, Stance.Calm, null, context));
	}
}
