using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Plated Armor loses a stack on an unblocked attack, not on each turn like StS2 Plating.</summary>
public sealed class Sts1PlatedArmorPower : PowerModel, IBorrowedIcon
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public Type IconSource => typeof(PlatingPower);

    public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || Owner.IsDead || dealer == null || dealer == Owner ||
            !props.IsPoweredAttack() || result.UnblockedDamage <= 0) return;
        Flash();
        await PowerCmd.Decrement(this);
    }
}
