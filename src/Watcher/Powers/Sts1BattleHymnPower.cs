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

/// <summary>StS1 Battle Hymn: before the owner draws at turn start, Amount Smites go into their hand (not once all enemies are dead).</summary>
public sealed class Sts1BattleHymnPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(HelloWorldPower);

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player != Owner.Player || CombatManager.Instance.IsOverOrEnding || !CombatState.Enemies.Any(enemy => enemy.IsAlive))
		{
			return;
		}
		Flash();
		for (int i = 0; i < Amount; i++)
		{
			CardModel smite = CombatState.CreateCard<Sts1Smite>(player);
			await CardPileCmd.AddGeneratedCardToCombat(smite, PileType.Hand, player);
		}
	}
}
