using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Silent;

/// <summary>Hits twice, then loses 2 damage for the rest of the combat (the Rampage pattern, downwards).</summary>
public sealed class Sts1GlassKnife : Sts1SilentCard
{
	private const string DecreaseKey = "Decrease";

	private decimal _damageLostFromPlays;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(8m, ValueProp.Move),
		new DynamicVar(DecreaseKey, 2m),
	};

	/// <summary>Kept so a downgrade (which resets the vars) can take the lost damage off again.</summary>
	private decimal DamageLostFromPlays
	{
		get => _damageLostFromPlays;
		set
		{
			AssertMutable();
			_damageLostFromPlays = value;
		}
	}

	public Sts1GlassKnife()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		decimal loss = Math.Min(DynamicVars[DecreaseKey].BaseValue, DynamicVars.Damage.BaseValue);
		DynamicVars.Damage.BaseValue -= loss;
		DamageLostFromPlays += loss;
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		DynamicVars.Damage.BaseValue = Math.Max(0m, DynamicVars.Damage.BaseValue - DamageLostFromPlays);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
