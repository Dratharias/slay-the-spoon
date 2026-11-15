using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Folie;

/// <summary>
/// A chest over per-player sources that every player reaches on their own (Draft rounds, the Sealed Deck pick): every
/// client runs every player's source, so each joins here with their stake; the chest opens once every living player
/// has joined, with the bidders in slot order, and each joiner gets back the stake they were awarded (or null).
/// A failed chest gives every player their own stake back.
/// </summary>
public static class FolieRendezvous
{
	private sealed class Meeting
	{
		public readonly Dictionary<Player, (IReadOnlyList<CardModel> Cards, object Tag)> Joined = new();
		public readonly TaskCompletionSource<Dictionary<Player, (Player Owner, object Tag)?>> Done =
			new(TaskCreationOptions.RunContinuationsAsynchronously);
	}

	private static readonly Dictionary<string, Meeting> Meetings = new();

	/// <returns>The awarded stake's owner and tag; null when the player took nothing.</returns>
	public static async Task<(Player Owner, object Tag)?> Join(IRunState run, string key, Player player,
		IReadOnlyList<CardModel> cards, object tag, bool consolation)
	{
		if (!Meetings.TryGetValue(key, out var meeting))
		{
			meeting = new Meeting();
			Meetings[key] = meeting;
		}
		meeting.Joined[player] = (cards, tag);
		var living = run.Players.Where(p => !p.Creature.IsDead).ToList();
		if (living.All(meeting.Joined.ContainsKey))
		{
			Meetings.Remove(key);
			meeting.Done.TrySetResult(await Resolve(run, key, living, meeting));
		}
		return (await meeting.Done.Task).GetValueOrDefault(player);
	}

	private static async Task<Dictionary<Player, (Player Owner, object Tag)?>> Resolve(IRunState run, string key,
		List<Player> bidders, Meeting meeting)
	{
		var own = bidders.ToDictionary(p => p, p => ((Player, object)?)(p, meeting.Joined[p].Tag));
		if (bidders.Count < 2) return own;
		try
		{
			var stakes = bidders.Select(p => new FolieStake(p, meeting.Joined[p].Cards)).ToList();
			FolieOutcome outcome = await FolieChest.Run(run, key, bidders, stakes, consolation: true);
			return bidders.ToDictionary(p => p, p =>
			{
				int index = bidders.IndexOf(p);
				FolieAward? award = outcome.Awards.Where(a => a.Player == index).Select(a => (FolieAward?)a).FirstOrDefault();
				return award is { } a ? ((Player, object)?)(bidders[a.Stake], meeting.Joined[bidders[a.Stake]].Tag) : null;
			});
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: {key} chest failed, every player keeps their own: {e}");
			return own;
		}
	}

	/// <summary>Moves cards to <paramref name="player"/> (a card's owner cannot be reassigned in place).</summary>
	public static void GiveCards(IRunState run, IEnumerable<CardModel> cards, Player player)
	{
		foreach (CardModel card in cards)
		{
			if (card.Owner == player) continue;
			run.RemoveCard(card);
			run.AddCard(card, player);
		}
	}
}
