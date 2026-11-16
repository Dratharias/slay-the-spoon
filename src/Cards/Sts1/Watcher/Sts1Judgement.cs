using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Judgement: an enemy at or below the Magic HP is killed outright.</summary>
public sealed class Sts1Judgement : Sts1WatcherCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 30m),
	};

	public Sts1Judgement()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature target = cardPlay.Target;
		// A Spoon fight supporter's Judgement does nothing to the Spoon (its damage is nullified, spec §5).
		if (target.IsAlive && WatcherSkillMath.JudgementKills(target.CurrentHp, DynamicVars["Magic"].IntValue)
			&& !SlaySpoon.Spoon.SpoonFight.IsSupporter(Owner.Creature))
		{
			await CreatureCmd.Kill(target);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(10m);
	}
}
