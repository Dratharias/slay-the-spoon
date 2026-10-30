using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using SlaySpoon.Core;
using SlaySpoon.Watcher;

namespace SlaySpoon.DevConsole;

/// <summary>Networked stance command for combat play-testing.</summary>
public sealed class Sts1StanceConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sts1stance";
	public override string Args => "<calm|wrath|divinity|neutral>";
	public override string Description => "Changes your stance during combat.";
	public override bool IsNetworked => true;
	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (issuingPlayer?.PlayerCombatState == null || !CombatManager.Instance.IsInProgress)
			return new CmdResult(false, "An active combat is required.");
		if (args.Length != 1 || !Enum.TryParse<Stance>(args[0], true, out var stance) || !Enum.IsDefined(stance))
			return new CmdResult(false, "Expected calm, wrath, divinity or neutral.");
		return new CmdResult(Run(issuingPlayer, stance), true, $"Entered {stance}.");
	}

	private static async Task Run(Player player, Stance stance)
	{
		var context = new HookPlayerChoiceContext(player, LocalContext.NetId!.Value, GameActionType.Combat);
		await context.AssignTaskAndWaitForPauseOrCompletion(StanceCmd.Enter(player, stance, null, context));
	}
}
