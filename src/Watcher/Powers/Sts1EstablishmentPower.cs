using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Establishment: each card the owner Retains costs Amount less for the rest of the combat (never below 0).</summary>
public sealed class Sts1EstablishmentPower : PowerModel, IBorrowedIcon
{
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
			if (card.Owner != player || card.EnergyCost.CostsX || card.EnergyCost.Canonical < 0)
			{
				continue;
			}
			int change = WatcherPowerMath.EstablishmentReduction(card.EnergyCost.GetWithModifiers(CostModifiers.Local), Amount);
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
