using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Trophies;

namespace SlaySpoon.Config;

/// <summary>
/// Trophies (Custom Run, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): each elite won gives every player a
/// <see cref="SpoonTrophyCard"/> made from one attack move of one elite in the fight. The moves are read when the fight
/// starts (the monsters are gone at victory).
/// </summary>
public sealed class SpoonTrophies : ModifierModel
{
	private readonly record struct Move(string Monster, string State, int Damage, int Hits);

	private readonly List<Move> _moves = new();
	private readonly List<string> _monsters = new();

	public override Task BeforeCombatStart()
	{
		_moves.Clear();
		_monsters.Clear();
		if (RunState.CurrentRoom is not CombatRoom { RoomType: RoomType.Elite } room) return Task.CompletedTask;
		foreach (var enemy in room.Enemies)
		{
			if (enemy.Monster is not { } monster) continue;
			string id = monster.Id.Entry;
			_monsters.Add(id);
			try
			{
				foreach (var state in monster.MoveStateMachine?.States.Values.OfType<MoveState>() ?? [])
				{
					var attack = state.Intents.OfType<AttackIntent>().FirstOrDefault();
					if (attack?.DamageCalc is not { } calc) continue;
					int damage = (int)calc();
					if (damage > 0) _moves.Add(new Move(id, state.StateId, damage, Math.Max(attack.Repeats, 1)));
				}
			}
			catch (Exception e)
			{
				SpoonLog.Error($"trophies: moves of {id} unreadable: {e.Message}");
			}
		}
		SpoonLog.Info($"trophies: {_moves.Count} attack moves from {_monsters.Count} elites");
		return Task.CompletedTask;
	}

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		if (room.RoomType != RoomType.Elite || _monsters.Count == 0) return;
		var run = RunState;
		int floor = run.TotalFloor;
		for (int slot = 0; slot < run.Players.Count; slot++)
		{
			Player player = run.Players[slot];
			try
			{
				var rng = new Rng(run.Rng.Seed, TrophyRule.Stream(floor, slot));
				var card = (SpoonTrophyCard)run.CreateCard(ModelDb.Card<SpoonTrophyCard>(), player);
				if (_moves.Count > 0)
				{
					var move = _moves[rng.NextInt(_moves.Count)];
					card.TrophyMonster = move.Monster;
					card.TrophyMove = move.State;
					card.TrophyDamage = move.Damage;
					card.TrophyHits = move.Hits;
				}
				else
				{
					card.TrophyMonster = _monsters[rng.NextInt(_monsters.Count)];
					card.TrophyDamage = TrophyRule.FallbackDamage;
					card.TrophyHits = 1;
				}
				await CardPileCmd.Add(card, PileType.Deck);
				SpoonLog.Info($"trophies: {player.NetId} gets {card.TrophyMonster}.{card.TrophyMove} {card.TrophyDamage}x{card.TrophyHits}");
			}
			catch (Exception e)
			{
				SpoonLog.Error($"trophies: card for slot {slot} failed: {e}");
			}
		}
	}
}
