using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Establishment: each card the owner Retains costs Amount less for the rest of the combat (never below 0).</summary>
public sealed class Sts1EstablishmentPower : PowerModel, IBorrowedIcon
{
	private static readonly AccessTools.FieldRef<CardEnergyCost, List<LocalCostModifier>> LocalModifiers =
		AccessTools.FieldRefAccess<CardEnergyCost, List<LocalCostModifier>>("_localModifiers");

	private static int CombatCost(CardEnergyCost cost)
	{
		int value = cost.GetWithModifiers(CostModifiers.None);
		foreach (var modifier in LocalModifiers(cost).Where(m => m.Expiration == LocalCostModifierExpiration.EndOfCombat))
			value = modifier.Modify(value);
		return Math.Max(0, value);
	}
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(RetainHandPower);

	public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
	{
		if (player != Owner.Player)
		{
			return Task.CompletedTask;
		}
		bool flashed = false;
		foreach (CardModel card in retainedCards)
		{
			if (card.Owner != player || !card.ShouldRetainThisTurn || card.EnergyCost.CostsX || card.EnergyCost.GetWithModifiers(CostModifiers.None) < 0)
			{
				continue;
			}
			int change = WatcherPowerMath.EstablishmentReduction(CombatCost(card.EnergyCost), Amount);
			if (change == 0)
			{
				continue;
			}
			if (!flashed)
			{
				Flash();
				flashed = true;
			}
			card.EnergyCost.AddThisCombat(change);
		}
		return Task.CompletedTask;
	}
}
