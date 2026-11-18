using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Cards.Sts1.Ironclad;

public sealed class Sts1LimitBreak : Sts1IroncladCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

	/// <summary>
	/// Strength gained before the doubling. 0 in StS1; The Forge raises it by 1 per level past +1
	/// (<c>data/forge/curves.json</c>), so Limit Break+5 gains 4 Strength, then doubles.
	/// </summary>
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<StrengthPower>(0m) };

	public Sts1LimitBreak()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int bonus = (int)DynamicVars["StrengthPower"].BaseValue;
		if (bonus > 0)
		{
			await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, bonus, Owner.Creature, this);
		}
		// Doubling negative Strength makes it more negative, as in StS1.
		int strength = Owner.Creature.GetPowerAmount<StrengthPower>();
		if (strength != 0)
		{
			await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strength, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
