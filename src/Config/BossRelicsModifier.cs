using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Config;

/// <summary>
/// StS1 Boss Relics (Custom Run): the act 2 and act 3 Ancients gain a "boss chest" option that replaces their own gift
/// (<see cref="SlaySpoon.Relics.BossChest"/>). The modifier holds no state; Neow does not offer the chest.
/// </summary>
public sealed class SpoonBossRelics : ModifierModel
{
}
