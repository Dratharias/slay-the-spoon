using MegaCrit.Sts2.Core.Entities.Players;

namespace SlaySpoon.Watcher;

/// <summary>Called after Regard, including an empty pile or a choice to discard nothing.</summary>
public interface IScryListener
{
	Task AfterScry(Player player, int discarded);
}
