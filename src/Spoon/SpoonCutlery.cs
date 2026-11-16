using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// The cutlery the Spoon deals into every player's draw pile each cycle (docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md).
/// Combat-only, Exhaust, outside every card source. The Spoon-targeted ones act only when their target is the Spoon.
/// </summary>
public abstract class SpoonCutleryCard : SpoonCard
{
	public override bool CanBeGeneratedInCombat => false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected SpoonCutleryCard(int cost, TargetType target)
		: base(cost, CardType.Skill, target)
	{
	}

	/// <summary>The Spoon this card was played on, or null.</summary>
	protected static SpoonMonster? SpoonOf(CardPlay cardPlay) => cardPlay.Target?.Monster as SpoonMonster;
}

/// <summary>Teaspoon: remove 2 Appetite (3 upgraded), draw 1.</summary>
public sealed class SpoonTeaspoon : SpoonCutleryCard
{
	private const string AppetiteKey = "Appetite";

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(AppetiteKey, SpoonRule.TeaspoonAppetite), new CardsVar(1)];

	public SpoonTeaspoon()
		: base(0, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (SpoonOf(cardPlay) is { } spoon && spoon.Creature.GetPower<SpoonAppetitePower>() is { } appetite)
		{
			int removed = Math.Min(DynamicVars[AppetiteKey].IntValue, appetite.Amount);
			if (removed > 0) await PowerCmd.ModifyAmount(choiceContext, appetite, -removed, Owner.Creature, this);
		}
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
	}

	protected override void OnUpgrade() =>
		DynamicVars[AppetiteKey].UpgradeValueBy(SpoonRule.TeaspoonAppetiteUpgraded - SpoonRule.TeaspoonAppetite);
}

/// <summary>Wooden Spoon: the Spoon's next Appetite gain (2 upgraded) does nothing.</summary>
public sealed class SpoonWoodenSpoon : SpoonCutleryCard
{
	private const string SatedKey = "Sated";

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(SatedKey, 1m)];

	public SpoonWoodenSpoon()
		: base(1, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (SpoonOf(cardPlay) is not { } spoon) return;
		await PowerCmd.Apply<SpoonSatedPower>(choiceContext, spoon.Creature, DynamicVars[SatedKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars[SatedKey].UpgradeValueBy(1m);
}

/// <summary>Skimmer: gain 6 Block (9 upgraded); the Spoon's next Banquet eats none of your max HP.</summary>
public sealed class SpoonSkimmer : SpoonCutleryCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(SpoonRule.SkimmerBlock, ValueProp.Move)];

	public SpoonSkimmer()
		: base(1, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<SpoonSkimmedPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(SpoonRule.SkimmerBlockUpgraded - SpoonRule.SkimmerBlock);
}

/// <summary>
/// Melon Baller: take 12 max HP (15 upgraded) from the Spoon, never its last one; you gain what it lost. Fixed: no
/// damage, so Strength and damage modifiers do not apply.
/// </summary>
public sealed class SpoonMelonBaller : SpoonCutleryCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(SpoonRule.BallerMaxHp)];

	public SpoonMelonBaller()
		: base(2, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (SpoonOf(cardPlay) is not { } spoon || !spoon.Creature.IsAlive) return;
		Creature body = spoon.Creature;
		int take = SpoonRule.BallerTake(body.MaxHp, DynamicVars.MaxHp.IntValue);
		if (take <= 0) return;
		await CreatureCmd.SetMaxHp(body, body.MaxHp - take);
		await CreatureCmd.GainMaxHp(Owner.Creature, take);
		await spoon.SyncStrength();
	}

	protected override void OnUpgrade() => DynamicVars.MaxHp.UpgradeValueBy(SpoonRule.BallerMaxHpUpgraded - SpoonRule.BallerMaxHp);
}
