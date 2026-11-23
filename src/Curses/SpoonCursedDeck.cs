using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// Cursed Deck (Custom Run, docs/superpowers/specs/2026-11-18-cursed-deck-design.md): every player starts with the
/// Unholy Grimoire and one random curse, drawn on the mod's own stream so vanilla RNG stays untouched.
/// </summary>
public sealed class SpoonCursedDeck : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonCursedDeck);

	protected override void AfterRunCreated(RunState runState)
	{
		var relic = ModelDb.Relic<SpoonUnholyGrimoire>();
		for (int slot = 0; slot < runState.Players.Count; slot++)
		{
			Player player = runState.Players[slot];
			player.RelicGrabBag.Remove(relic);
			if (player.Relics.All(r => r.Id != relic.Id)) player.AddRelicInternal(relic.ToMutable());
			Rng rng = new(runState.Rng.Seed, CurseRule.StartStream(slot));
			player.Deck.AddInternal(CursedDeckCurses.Create(player, rng));
		}
		runState.SharedRelicGrabBag.Remove(relic);
	}

	/// <summary>Forged curses' Spiteful defect (with The Forge): one more in-hand run before the vanilla one.</summary>
	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants) =>
		side == CombatSide.Player ? Forged.SpitefulTurnEnd.Run(choiceContext, participants) : Task.CompletedTask;
}
