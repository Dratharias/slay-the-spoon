using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace SlaySpoon.Act4;

/// <summary>
/// StS1 Act 4 boss: the Corrupt Heart alone, with the act's own background and music. Like vanilla placeholder
/// bosses, the map node shows a static icon: NBossMapPoint loads BossNodePath + ".png" and + "_outline.png".
/// </summary>
public sealed class Sts1CorruptHeartBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string BossNodePath => "res://SlaySpoon/images/ending/heart_boss";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Sts1CorruptHeart>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
		[(ModelDb.Monster<Sts1CorruptHeart>().ToMutable(), null)];
}
