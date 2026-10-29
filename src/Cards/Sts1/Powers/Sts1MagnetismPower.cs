using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>At the start of its owner's turn, add Amount random colorless cards to their hand, each rolled on its own.</summary>
public sealed class Sts1MagnetismPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(CreativeAiPower);

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player != Owner.Player)
		{
			return;
		}
		IEnumerable<CardModel> pool = ModelDb.CardPool<ColorlessCardPool>()
			.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
		foreach (CardModel card in CardFactory.GetForCombat(player, pool, Amount, player.RunState.Rng.CombatCardGeneration).ToList())
		{
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
		}
	}
}
