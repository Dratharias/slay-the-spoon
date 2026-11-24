using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// Cursed Deck (Custom Run, docs/superpowers/specs/2026-11-18-cursed-deck-design.md): every player starts with the
/// Unholy Grimoire and one random curse, drawn on the mod's own stream so vanilla RNG stays untouched. Without Overcursed,
/// Neow offers the Witching Pact (<see cref="WitchingPact"/>).
/// </summary>
public sealed class SpoonCursedDeck : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonCursedDeck);

	public override LocString NeowOptionTitle => new("modifiers", Id.Entry + ".neow_title");
	public override LocString NeowOptionDescription => new("modifiers", Id.Entry + ".neow_description");

	/// <summary>Whether Neow shows the Witching Pact page (not with Overcursed, which already deals the Witching Hour).</summary>
	internal bool OffersNeowOption => WitchingPactRule.Offered(true, SpoonOvercursed.IsActive(RunState));

	public override Func<Task>? GenerateNeowOption(EventModel eventModel) =>
		OffersNeowOption && eventModel.Owner is { } owner ? () => WitchingPact.Open(owner) : null;

	protected override void AfterRunCreated(RunState runState)
	{
		var relic = ModelDb.Relic<SpoonUnholyGrimoire>();
		for (int slot = 0; slot < runState.Players.Count; slot++)
		{
			Player player = runState.Players[slot];
			player.RelicGrabBag.Remove(relic);
			if (player.Relics.All(r => r.Id != relic.Id)) player.AddRelicInternal(relic.ToMutable());
			Rng rng = new(runState.Rng.Seed, CurseRule.StartStream(slot));
			CardModel curse = CursedDeckCurses.Create(player, rng);
			player.Deck.AddInternal(curse);
			// Overcursed: the start curse comes in three too.
			if (SpoonOvercursed.IsActive(runState))
				for (int i = 0; i < OvercursedRule.ExtraCopies; i++) player.Deck.AddInternal(runState.CloneCard(curse));
		}
		runState.SharedRelicGrabBag.Remove(relic);
	}

	/// <summary>Forged curses' Spiteful defect (with The Forge): one more in-hand run before the vanilla one.</summary>
	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants) =>
		side == CombatSide.Player ? Forged.SpitefulTurnEnd.Run(choiceContext, participants) : Task.CompletedTask;
}
