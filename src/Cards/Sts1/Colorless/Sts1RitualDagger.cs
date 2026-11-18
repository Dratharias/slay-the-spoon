using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Colorless;

public sealed class Sts1RitualDagger : Sts1ColorlessCard
{
	private const string IncreaseKey = "Increase";
	private int _currentDamage = 15;
	private int _increasedDamage;

	// Reuse The Scythe's saved property names: new names would change network property IDs.
	[SavedProperty]
	public int CurrentDamage
	{
		get => _currentDamage;
		set
		{
			AssertMutable();
			_currentDamage = value;
			DynamicVars.Damage.BaseValue = value;
		}
	}

	[SavedProperty]
	public int IncreasedDamage
	{
		get => _increasedDamage;
		set
		{
			AssertMutable();
			_increasedDamage = value;
		}
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.Fatal) };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(CurrentDamage, ValueProp.Move),
		new IntVar(IncreaseKey, 3m),
	};

	public Sts1RitualDagger()
		: base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		// Snapshot Fatal eligibility before damage removes powers from a dying target (same as Feed).
		bool eligible = cardPlay.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
		AttackCommand attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
			.Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (eligible && attack.Results.SelectMany(hits => hits).Any(hit => hit.WasTargetKilled))
		{
			int increase = DynamicVars[IncreaseKey].IntValue;
			Grow(increase);
			(DeckVersion as Sts1RitualDagger)?.Grow(increase);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars[IncreaseKey].UpgradeValueBy(2m);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		CurrentDamage = 15 + IncreasedDamage;
	}

	private void Grow(int increase)
	{
		IncreasedDamage += increase;
		CurrentDamage = 15 + IncreasedDamage;
	}
}
