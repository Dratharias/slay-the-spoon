using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1MarkOfPain : Sts1InactiveBossRelic
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new EnergyVar(1), new CardsVar(2) };
	public override decimal ModifyMaxEnergy(Player player, decimal amount) => player == Owner ? amount + DynamicVars.Energy.IntValue : amount;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Wound>().Prepend(HoverTipFactory.ForEnergy(this));
	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
	 if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState is not { TurnNumber: <= 1 }) return;
	 Flash();
	 for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
	  await CardPileCmd.AddGeneratedCardToCombat(combatState.CreateCard<Wound>(Owner), PileType.Draw, Owner, CardPilePosition.Random);
	}
}
