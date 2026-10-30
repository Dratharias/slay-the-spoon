using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Indignation: in Wrath apply Vulnerable to ALL enemies, otherwise enter Wrath.</summary>
public sealed class Sts1Indignation : Sts1WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<Sts1WrathStancePower>(), HoverTipFactory.FromPower<VulnerablePower>() };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 3m),
	};

	public Sts1Indignation()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (StanceCmd.Current(Owner) == Stance.Wrath)
		{
			await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState!.HittableEnemies.ToList(), DynamicVars["Magic"].BaseValue, Owner.Creature, this);
		}
		else
		{
			await StanceCmd.Enter(Owner, Stance.Wrath, this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(2m);
	}
}
