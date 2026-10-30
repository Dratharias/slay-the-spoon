using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Characters.Pools;

namespace SlaySpoon.DevConsole;

/// <summary>The native item commands search global pools, which intentionally exclude the Watcher's items.</summary>
public sealed class Sts1WatcherItemConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sts1watcheritem";
	public override string Args => "<relic|potion> <model-id:string>";
	public override string Description => "Grants a Watcher item for testing, including inactive former boss relics.";
	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (issuingPlayer == null) return new CmdResult(false, "A run is not in progress.");
		if (args.Length != 2) return new CmdResult(false, Args);
		string id = args[1].ToUpperInvariant();
		if (args[0].Equals("relic", StringComparison.OrdinalIgnoreCase))
		{
			RelicModel? relic = ModelDb.RelicPool<Sts1WatcherRelicPool>().AllRelics.FirstOrDefault(r => r.Id.Entry == id);
			if (relic == null) return new CmdResult(false, "Unknown Watcher relic.");
			if (issuingPlayer.GetRelicById(relic.Id) != null) return new CmdResult(false, "Relic already held.");
			return new CmdResult(RelicCmd.Obtain(relic.ToMutable(), issuingPlayer), true, "Granted " + id);
		}
		if (args[0].Equals("potion", StringComparison.OrdinalIgnoreCase))
		{
			PotionModel? potion = ModelDb.PotionPool<Sts1WatcherPotionPool>().AllPotions.FirstOrDefault(p => p.Id.Entry == id);
			if (potion == null) return new CmdResult(false, "Unknown Watcher potion.");
			return new CmdResult(PotionCmd.TryToProcure(potion.ToMutable(), issuingPlayer), true, "Requested " + id + " (requires a free potion slot).");
		}
		return new CmdResult(false, Args);
	}
}
