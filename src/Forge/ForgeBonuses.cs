using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>
/// The curve table's per-level extras for forged cards (level 2 and above): <c>powerBonus</c> raises a power the card
/// applies, <c>play</c> adds a small effect each time it is played. One step per level past +1
/// (<see cref="ForgeRule.PerLevelBonus"/>).
/// </summary>
public static class ForgeBonuses
{
	public static decimal PowerBonus(PowerModel power, CardModel? card)
	{
		if (card is not { CurrentUpgradeLevel: > 1 } || ForgeCurves.For(card.Id.Entry)?.PowerBonus is not { } bonus) return 0m;
		return bonus.TryGetValue(power.GetType().Name, out var per) ? ForgeRule.PerLevelBonus(per, card.CurrentUpgradeLevel, ForgeLevels.Cap(card)) : 0m;
	}

	public static async Task OnPlayed(PlayerChoiceContext ctx, CardPlay play)
	{
		var card = play.Card;
		if (card is not { CurrentUpgradeLevel: > 1 } || ForgeCurves.For(card.Id.Entry)?.Play is not { } bonus) return;
		var me = play.Player.Creature;
		if (me == null || me.IsDead) return;
		foreach (var (kind, per) in bonus)
		{
			decimal amount = ForgeRule.PerLevelBonus(per, card.CurrentUpgradeLevel, ForgeLevels.Cap(card));
			if (amount <= 0) continue;
			try
			{
				await Apply(ctx, play, kind, amount);
			}
			catch (Exception e)
			{
				SpoonLog.Error($"forge: play bonus {kind} of {card.Id.Entry} failed: {e.Message}");
			}
		}
	}

	private static Creature? Enemy(CardPlay play)
	{
		if (play.Target is { IsDead: false } t && t.Side != play.Player.Creature.Side) return t;
		var enemies = play.Card.CombatState?.HittableEnemies;
		return enemies is { Count: > 0 } ? play.Player.RunState.Rng.CombatTargets.NextItem(enemies) : null;
	}

	private static async Task Apply(PlayerChoiceContext ctx, CardPlay play, string kind, decimal amount)
	{
		var card = play.Card;
		var me = play.Player.Creature;
		switch (kind)
		{
			case "Block":
				await CreatureCmd.GainBlock(me, amount, ValueProp.Unpowered, play);
				break;
			case "Damage":
				if (Enemy(play) is { } hit) await CreatureCmd.Damage(ctx, hit, amount, ValueProp.Unpowered, me, card, play);
				break;
			case "Draw":
				await CardPileCmd.Draw(ctx, amount, play.Player);
				break;
			case "Energy":
				await PlayerCmd.GainEnergy(amount, play.Player);
				break;
			case "Strength":
				await PowerCmd.Apply<StrengthPower>(ctx, me, amount, me, card);
				break;
			case "Dexterity":
				await PowerCmd.Apply<DexterityPower>(ctx, me, amount, me, card);
				break;
			case "Vigor":
				await PowerCmd.Apply<VigorPower>(ctx, me, amount, me, card);
				break;
			case "Poison":
				if (Enemy(play) is { } p) await PowerCmd.Apply<PoisonPower>(ctx, p, amount, me, card);
				break;
			case "Weak":
				if (Enemy(play) is { } w) await PowerCmd.Apply<WeakPower>(ctx, w, amount, me, card);
				break;
			case "Vulnerable":
				if (Enemy(play) is { } v) await PowerCmd.Apply<VulnerablePower>(ctx, v, amount, me, card);
				break;
			default:
				SpoonLog.Error($"forge: unknown play bonus {kind} on {card.Id.Entry}");
				break;
		}
	}
}
