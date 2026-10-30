using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace SlaySpoon.Act4;

/// <summary>
/// StS1 Act 4 elite: Spire Shield and Spire Spear on both sides of the players, laid out and framed like vanilla
/// Kaiser Crab (<see cref="EndingEncounterScenePatch"/> builds the slot scene in code).
/// </summary>
public sealed class Sts1ShieldAndSpearElite : EncounterModel
{
	public const string ShieldSlot = "shield";
	public const string SpearSlot = "spear";

	public override RoomType RoomType => RoomType.Elite;
	public override bool FullyCenterPlayers => true;
	public override IReadOnlyList<string> Slots => [ShieldSlot, SpearSlot];
	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
		[ModelDb.Monster<Sts1SpireShield>(), ModelDb.Monster<Sts1SpireSpear>()];

	// Kaiser Crab's framing: its slots sit near the screen edges, so the camera pulls back.
	public override float GetCameraScaling() => 0.75f;
	public override Vector2 GetCameraOffset() => Vector2.Down * 35f;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
	[
		(ModelDb.Monster<Sts1SpireShield>().ToMutable(), ShieldSlot),
		(ModelDb.Monster<Sts1SpireSpear>().ToMutable(), SpearSlot),
	];

	/// <summary>
	/// The encounter scene: a full-screen Control holding one Marker2D per slot, at the positions of vanilla
	/// kaiser_crab_boss.tscn's crusher (left) and rocket (right) markers.
	/// </summary>
	public static Control CreateSlots()
	{
		var root = new Control
		{
			Name = "Sts1ShieldAndSpearElite",
			Size = new Vector2(1920, 1080),
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		root.AddChild(new Marker2D { Name = ShieldSlot, Position = new Vector2(136, 736) });
		root.AddChild(new Marker2D { Name = SpearSlot, Position = new Vector2(1856, 712) });
		return root;
	}
}
