using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Spoon;

namespace SlaySpoon.Config;

/// <summary>
/// Spoon (Custom Run, docs/superpowers/specs/2026-11-14-spoon-design.md): grow your max HP with Spoonful; at 300 the Spoon
/// awaits after act 3. Every player starts with the 12 max HP relics.
/// </summary>
public sealed class SpoonSpoon : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonSpoon);

	public static SpoonSpoon? Of(IRunState? run) => run?.Modifiers.OfType<SpoonSpoon>().FirstOrDefault();

	// Every player starts with one Spoonful (Eternal: it can be neither removed nor transformed).
	protected override void AfterRunCreated(RunState runState)
	{
		foreach (Player player in runState.Players)
			player.Deck.AddInternal(runState.CreateCard<SpoonSpoonful>(player));

		// The 12 max HP relics from the start (owner, 2026-11-16), like King Midas's gold relics: taken out of every grab bag,
		// their pickup max HP comes from vanilla: RunManager.FinalizeStartingRelics runs AfterObtained on every relic held
		// at the start, these included (Lee's Waffle heals to full).
		var relics = SpoonRule.StartRelics
			.Select(r => ModelDb.AllRelics.FirstOrDefault(m => m.Id.Entry == r.Entry))
			.OfType<RelicModel>().ToList();
		foreach (Player player in runState.Players)
		{
			foreach (RelicModel canonical in relics)
			{
				player.RelicGrabBag.Remove(canonical);
				if (player.Relics.Any(r => r.Id == canonical.Id)) continue;
				player.AddRelicInternal(canonical.ToMutable());
			}
		}
		foreach (RelicModel canonical in relics)
			runState.SharedRelicGrabBag.Remove(canonical);
	}

	// Before vanilla sets the win time and saves the run, on every co-op peer at the same point.
	public override Task AfterCombatVictory(CombatRoom room)
	{
		try
		{
			SpoonRouting.EnsureSpoonAct(RunState, room);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spoon: Spoon act routing failed: {e}");
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// Every ordinary monster steals Appetite on hit (owner, 2026-11-16): each hit that reaches a player, even fully
	/// blocked, moves <see cref="SpoonRule.StolenOnHits"/> Appetite from that player to the attacker. The Spoon keeps its
	/// own per-move rule (<see cref="SpoonMonster"/>). Runs on every peer from the same attack results.
	/// </summary>
	public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		await base.AfterAttack(choiceContext, command);
		Creature? attacker = command.Attacker;
		if (attacker?.Monster == null || attacker.Monster is SpoonMonster || !attacker.IsAlive) return;
		if (attacker.CombatState?.Enemies.Contains(attacker) != true) return;
		int gained = 0;
		foreach (var hits in command.Results.SelectMany(hit => hit).Where(r => r.Receiver.IsPlayer).GroupBy(r => r.Receiver).ToList())
		{
			int stolen = SpoonRule.StolenOnHits(hits.Count());
			if (stolen <= 0 || !hits.Key.IsAlive) continue;
			await PowerCmd.Apply<SpoonAppetitePower>(choiceContext, hits.Key, -stolen, attacker, null);
			gained += stolen;
		}
		if (gained > 0 && attacker.IsAlive)
			await PowerCmd.Apply<SpoonAppetitePower>(choiceContext, attacker, gained, attacker, null);
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		options.Add(new SpoonFeastRestSiteOption(player));
		options.Add(new SpoonDigestRestSiteOption(player));
		return true;
	}
}
