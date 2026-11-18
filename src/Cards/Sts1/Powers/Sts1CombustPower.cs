using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// At the end of the turn, lose SelfDamage HP and deal Amount damage to ALL enemies. Each Combust played adds
/// its damage and 1 HP loss, as in StS1 (the card increments SelfDamage like vanilla Inferno).
/// </summary>
public sealed class Sts1CombustPower : PowerModel, IBorrowedIcon
{
	private const string SelfDamageKey = "SelfDamage";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(InfernoPower);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(SelfDamageKey, 0m, ValueProp.Unblockable | ValueProp.Unpowered),
	};

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || !CombatState.HittableEnemies.Any())
		{
			return;
		}
		Flash();
		DamageVar selfDamage = (DamageVar)DynamicVars[SelfDamageKey];
		await CreatureCmd.Damage(choiceContext, Owner, selfDamage.BaseValue, selfDamage.Props, Owner, null, null);
		await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner, null, null);
	}

	public void IncrementSelfDamage()
	{
		AssertMutable();
		DynamicVars[SelfDamageKey].BaseValue++;
	}
}
