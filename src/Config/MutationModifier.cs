using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Mutation (Custom Run): when acts 2 and later start, every deck card that is not upgraded is transformed at random
/// (<see cref="MutationRule"/>). The last mutated act is saved so a reload never mutates twice.
/// </summary>
public sealed class SpoonMutation : ModifierModel
{
	private int _lastMutated = -1;

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_lastMutated = -1;
	}

	[SavedProperty]
	public int SpoonMutationAct
	{
		get => _lastMutated;
		set => _lastMutated = value;
	}

	public override async Task AfterActEntered()
	{
		int act = RunState.CurrentActIndex;
		if (!MutationRule.Mutates(act, _lastMutated)) return;
		_lastMutated = act;
		var players = RunState.Players;
		for (int slot = 0; slot < players.Count; slot++)
		{
			var rng = new Rng(RunState.Rng.Seed, MutationRule.Stream(act, slot));
			var cards = PileType.Deck.GetPile(players[slot]).Cards.Where(c => !c.IsUpgraded && c.IsTransformable).ToList();
			if (cards.Count == 0) continue;
			try
			{
				await CardCmd.Transform(cards.Select(c => new CardTransformation(c)), rng);
				SpoonLog.Info($"mutation: act {act + 1}, player {slot}: {cards.Count} cards transformed");
			}
			catch (Exception e)
			{
				SpoonLog.Error($"mutation: act {act + 1}, player {slot} failed: {e.Message}");
			}
		}
	}
}
