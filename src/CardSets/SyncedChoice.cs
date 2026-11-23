using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.CardSets;

/// <summary>
/// One index-valued player choice, synced the way the game syncs its card selection screens: the chooser's client
/// picks and sends the index, every other client (and a replay) waits for it.
/// </summary>
public static class SyncedChoice
{
	private static readonly Func<Player, bool> ShouldSelectLocalCard =
		AccessTools.MethodDelegate<Func<Player, bool>>(AccessTools.Method(typeof(CardSelectCmd), "ShouldSelectLocalCard"));

	/// <param name="pickLocally">Shows the screen on the chooser's client and returns the picked index.</param>
	public static async Task<int> Choose(PlayerChoiceContext context, Player player, Func<Task<int>> pickLocally)
	{
		PlayerChoiceSynchronizer sync = RunManager.Instance.PlayerChoiceSynchronizer;
		uint choiceId = sync.ReserveChoiceId(player);
		await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);
		int index;
		if (ShouldSelectLocalCard(player))
		{
			index = await pickLocally();
			sync.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndex(index));
		}
		else
		{
			index = (await sync.WaitForRemoteChoice(player, choiceId)).AsIndex();
		}
		await context.SignalPlayerChoiceEnded();
		return index;
	}

	/// <summary>Several indexes in one synced choice (the game's own index list), for a screen that settles many
	/// questions at once.</summary>
	public static async Task<List<int>> ChooseMany(PlayerChoiceContext context, Player player, Func<Task<List<int>>> pickLocally)
	{
		PlayerChoiceSynchronizer sync = RunManager.Instance.PlayerChoiceSynchronizer;
		uint choiceId = sync.ReserveChoiceId(player);
		await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);
		List<int> indexes;
		if (ShouldSelectLocalCard(player))
		{
			indexes = await pickLocally();
			sync.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(indexes));
		}
		else
		{
			indexes = (await sync.WaitForRemoteChoice(player, choiceId)).AsIndexes();
		}
		await context.SignalPlayerChoiceEnded();
		return indexes;
	}
}
