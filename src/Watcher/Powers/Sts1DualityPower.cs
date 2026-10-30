using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Relics.Watcher;

namespace SlaySpoon.Watcher.Powers;

public sealed class Sts1DualityPower : TemporaryDexterityPower, IBorrowedIcon
{
    public override AbstractModel OriginModel => ModelDb.Relic<Sts1Duality>();
    public Type IconSource => typeof(DexterityPower);
}
