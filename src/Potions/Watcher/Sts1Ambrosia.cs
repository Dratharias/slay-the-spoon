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
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Potions.Watcher;

public sealed class Sts1Ambrosia : PotionModel, IBorrowedIcon
{
	public override PotionRarity Rarity => PotionRarity.Rare;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;
	public Type IconSource => typeof(CosmicConcoction);
	public override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1DivinityStancePower>() };
	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		await StanceCmd.Enter(Owner, Stance.Divinity, null, choiceContext);
	}
}
