using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Watcher;

/// <summary>
/// Stance changes and Mantra for StS1's Watcher. A stance is one of three Single powers on the player's creature;
/// Neutral is having none of them.
/// </summary>
public static class StanceCmd
{
	public static Stance Current(Player player) => Current(player.Creature);

	public static Stance Current(Creature creature)
	{
		if (creature.HasPower<Sts1WrathStancePower>()) return Stance.Wrath;
		if (creature.HasPower<Sts1CalmStancePower>()) return Stance.Calm;
		if (creature.HasPower<Sts1DivinityStancePower>()) return Stance.Divinity;
		return Stance.Neutral;
	}

	/// <summary>
	/// Puts <paramref name="player"/> in <paramref name="stance"/>: leaves the old stance (Calm gives energy), enters the
	/// new one (Divinity gives energy), then tells every <see cref="IStanceListener"/>. Nothing happens, and no listener
	/// fires, when the player already holds that stance.
	/// </summary>
	public static async Task Enter(Player player, Stance stance, CardModel? source, PlayerChoiceContext? choiceContext = null)
	{
		Creature creature = player.Creature;
		Stance from = Current(creature);
		if (from == stance || player.PlayerCombatState == null)
		{
			return;
		}
		choiceContext ??= new ThrowingPlayerChoiceContext();
		await PowerCmd.Remove(StancePower(creature, from));
		int exitEnergy = StanceMath.EnergyOnExit(from, violetLotus: player.GetRelic<SlaySpoon.Relics.Watcher.Sts1VioletLotus>() != null);
		if (exitEnergy > 0)
		{
			await PlayerCmd.GainEnergy(exitEnergy, player);
		}
		switch (stance)
		{
			case Stance.Calm:
				await PowerCmd.Apply<Sts1CalmStancePower>(choiceContext, creature, 1m, creature, source);
				break;
			case Stance.Wrath:
				await PowerCmd.Apply<Sts1WrathStancePower>(choiceContext, creature, 1m, creature, source);
				break;
			case Stance.Divinity:
				await PowerCmd.Apply<Sts1DivinityStancePower>(choiceContext, creature, 1m, creature, source);
				break;
		}
		int enterEnergy = StanceMath.EnergyOnEnter(stance);
		if (enterEnergy > 0)
		{
			await PlayerCmd.GainEnergy(enterEnergy, player);
		}
		StanceEyePatch.Show(creature, Current(creature));
		foreach (IStanceListener listener in Listeners(player))
		{
			await listener.AfterStanceChanged(player, from, stance);
		}
	}

	/// <summary>
	/// Adds <paramref name="amount"/> Mantra. Reaching the threshold enters Divinity and keeps the remainder, as StS1's
	/// Mantra does when it stacks.
	/// </summary>
	public static async Task GainMantra(Player player, int amount, CardModel? source = null, PlayerChoiceContext? choiceContext = null)
	{
		Creature creature = player.Creature;
		if (amount <= 0 || player.PlayerCombatState == null)
		{
			return;
		}
		choiceContext ??= new ThrowingPlayerChoiceContext();
		MantraTracker.Record(player, amount);
		int before = creature.GetPowerAmount<Sts1MantraPower>();
		Sts1MantraPower? mantra = await PowerCmd.Apply<Sts1MantraPower>(choiceContext, creature, amount, creature, source);
		(int left, bool enterDivinity) = StanceMath.AddMantra(before, amount);
		if (!enterDivinity || mantra == null)
		{
			return;
		}
		if (left > 0)
		{
			mantra.SetAmount(left);
		}
		else
		{
			await PowerCmd.Remove(mantra);
		}
		await Enter(player, Stance.Divinity, source, choiceContext);
	}

	private static PowerModel? StancePower(Creature creature, Stance stance) => stance switch
	{
		Stance.Calm => creature.GetPower<Sts1CalmStancePower>(),
		Stance.Wrath => creature.GetPower<Sts1WrathStancePower>(),
		Stance.Divinity => creature.GetPower<Sts1DivinityStancePower>(),
		_ => null,
	};

	/// <summary>Snapshot of the listeners, so one that changes powers or piles does not break the walk.</summary>
	private static List<IStanceListener> Listeners(Player player)
	{
		var models = new List<AbstractModel>();
		models.AddRange(player.Creature.Powers);
		models.AddRange(player.Relics);
		if (player.PlayerCombatState is { } combat)
		{
			models.AddRange(combat.Hand.Cards);
			models.AddRange(combat.DrawPile.Cards);
			models.AddRange(combat.DiscardPile.Cards);
		}
		return models.OfType<IStanceListener>().ToList();
	}
}
