using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon act's boss encounter. Stub until the fight lands (Task 5): no monster yet, and the Heart's map icon as
/// the placeholder node. NBossMapPoint loads BossNodePath + ".png" and + "_outline.png".
/// </summary>
public sealed class SpoonBoss : EncounterModel
{
	public override RoomType RoomType => RoomType.Boss;

	public override string BossNodePath => "res://SlaySpoon/images/ending/heart_boss";

	public override MegaSkeletonDataResource? BossNodeSpineResource => null;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => [];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [];
}
