using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Events;

namespace SlaySpoon.DevConsole;

/// <summary>
/// Console command <c>sts1event</c>: the vanilla <c>event</c> command only knows the acts' events, which leave out the
/// StS1 events. Same behaviour, StS1 events only; it skips their eligibility rule, so play-testers can open any of them.
/// </summary>
public sealed class Sts1EventConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sts1event";

	public override string Args => "<id:string>";

	public override string Description => "Jumps a player to a Slay the Spire 1 event, e.g. 'sts1event STS1_NEST'.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No event name specified.");
		}
		if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		string eventName = args[0].ToUpperInvariant();
		Sts1Event? model = Sts1Event.All.FirstOrDefault(e => e.Id.Entry == eventName);
		if (model == null)
		{
			return new CmdResult(success: false, "Event '" + eventName + "' not found");
		}
		issuingPlayer.RunState.AppendToMapPointHistory(MapPointType.Unknown, RoomType.Event, model.Id);
		return new CmdResult(RunManager.Instance.EnterRoom(new EventRoom(model)), success: true, "Jumped to event: '" + model.Id.Entry + "'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			return CompleteArgument(Sts1Event.All.Select(e => e.Id.Entry), Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return base.GetArgumentCompletions(player, args);
	}
}
