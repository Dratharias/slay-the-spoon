using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Potions.Watcher;

public sealed class Sts1BottledMiracle : PotionModel, IBorrowedIcon
{
	public override PotionRarity Rarity => PotionRarity.Common;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;
	public Type IconSource => typeof(EnergyPotion);
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(2) };
	public override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Sts1Miracle>() };
	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
		{
			CardModel miracle = Owner.Creature.CombatState!.CreateCard<Sts1Miracle>(Owner);
			await CardPileCmd.AddGeneratedCardToCombat(miracle, PileType.Hand, Owner);
		}
	}
}
