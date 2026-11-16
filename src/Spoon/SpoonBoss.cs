using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon act's boss encounter: the Spoon alone. Like the Heart, the map node shows a static icon: NBossMapPoint
/// loads BossNodePath + ".png" and + "_outline.png".
/// </summary>
public sealed class SpoonBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string BossNodePath => "res://SlaySpoon/images/spoon/spoon_boss";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<SpoonMonster>()];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
		[(ModelDb.Monster<SpoonMonster>().ToMutable(), null)];
}
