using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.WatcherTokens;

public sealed class Sts1Expunger : Sts1WatcherTokenCard
{
	public Sts1Expunger() : base(1, CardType.Attack, TargetType.AnyEnemy) { }
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(9m, ValueProp.Move), new RepeatVar(0) };
	/// <summary>Conjure Blade supplies its X result on the combat copy; dynamic vars survive cloning.</summary>
	public void SetHits(int hits)
	{
		AssertMutable();
		DynamicVars.Repeat.BaseValue = Math.Max(0, hits);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (DynamicVars.Repeat.IntValue > 0)
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue)
				.FromCard(this).Targeting(cardPlay.Target!).Execute(choiceContext);
	}
	protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(6m); }
}
