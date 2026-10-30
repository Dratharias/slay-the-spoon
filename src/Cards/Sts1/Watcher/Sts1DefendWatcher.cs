using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>The Watcher's starter Defend.</summary>
public sealed class Sts1DefendWatcher : Sts1WatcherCard
{
	public override bool GainsBlock => true;

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(5m, ValueProp.Move),
	};

	public Sts1DefendWatcher()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}
