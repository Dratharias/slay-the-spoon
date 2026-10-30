using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SlaySpoon.Watcher;

namespace SlaySpoon.DevConsole;

/// <summary>Exercise Scry and its synchronized choice before the card port is complete.</summary>
public sealed class Sts1ScryConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sts1scry";
	public override string Args => "<amount:int>";
	public override string Description => "Inspect the top cards and choose which to discard.";
	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (issuingPlayer?.PlayerCombatState == null || !CombatManager.Instance.IsInProgress)
			return new CmdResult(false, "An active combat is required.");
		if (args.Length != 1 || !int.TryParse(args[0], out int amount) || amount < 0)
			return new CmdResult(false, "Expected a non-negative number of cards.");
		return new CmdResult(Run(issuingPlayer, amount), true, $"Scry {amount}.");
	}

	private static async Task Run(Player player, int amount)
	{
		var context = new HookPlayerChoiceContext(player, LocalContext.NetId!.Value, GameActionType.Combat);
		await context.AssignTaskAndWaitForPauseOrCompletion(ScryCmd.Scry(player, amount, context));
	}
}
