using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Windmill Strike: each time the owner retains it, its damage grows for the rest of the combat (combat copy only).</summary>
public sealed class Sts1WindmillStrike : Sts1WatcherCard
{
	private decimal _gained;

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(7m, ValueProp.Move),
		new DynamicVar("Magic", 4m),
	};

	public Sts1WindmillStrike()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target!)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}

	public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
	{
		// Only a card the owner kept through this flush grows. The deck card is untouched, so the growth ends with the combat.
		if (player == Owner && retainedCards.Contains(this))
		{
			decimal growth = DynamicVars["Magic"].BaseValue;
			_gained += growth;
			DynamicVars.Damage.BaseValue += growth;
		}
		return Task.CompletedTask;
	}

	// A downgrade rebuilds the dynamic vars from the canonical card, which would drop the growth.
	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		DynamicVars.Damage.BaseValue += _gained;
	}
}
