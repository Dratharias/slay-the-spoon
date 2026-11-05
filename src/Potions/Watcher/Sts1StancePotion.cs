using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Potions.Watcher;

public sealed class Sts1StancePotion : PotionModel, IBorrowedIcon
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;
	public Type IconSource => typeof(SkillPotion);
	public override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1CalmStancePower>(), HoverTipFactory.FromPower<Sts1WrathStancePower>() };
	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		var combat = Owner.Creature.CombatState!;
		CardModel[] choices = { combat.CreateCard<Sts1ChooseWrath>(Owner), combat.CreateCard<Sts1ChooseCalm>(Owner) };
		CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner, canSkip: false);
		if (chosen is Sts1ChooseCalm calm) await calm.ApplyChoice(choiceContext);
		else if (chosen is Sts1ChooseWrath wrath) await wrath.ApplyChoice(choiceContext);
	}
}
