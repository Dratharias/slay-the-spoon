using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Forge;

namespace SlaySpoon.UselessRelics;

/// <summary>
/// Useless Relics: which relics flashed this combat, which were thrown, and the throw itself. A thrown relic is greyed
/// (Disabled status) and left out of every hook listener list until the combat ends, so it stops working.
/// </summary>
public static class UselessRelicThrow
{
	private static readonly HashSet<RelicModel> Flashed = new(ReferenceEqualityComparer.Instance);
	private static readonly HashSet<RelicModel> Thrown = new(ReferenceEqualityComparer.Instance);

	public static bool IsThrown(RelicModel relic) => Thrown.Contains(relic);

	public static bool AnyThrown => Thrown.Count > 0;

	internal static void RecordFlash(RelicModel relic)
	{
		if (SpoonUselessRelics.IsActive(ForgeLevels.CurrentRun)) Flashed.Add(relic);
	}

	public static bool CanThrow(RelicModel relic) => CombatManager.Instance.IsInProgress
		&& UselessRelicRule.CanThrow(relic.Id.Entry, Flashed.Contains(relic), relic.IsMelted, Thrown.Contains(relic));

	/// <summary>Combat over (or a new room): thrown relics come back.</summary>
	public static void Reset()
	{
		foreach (var relic in Thrown)
		{
			try { relic.Status = RelicStatus.Normal; }
			catch (Exception e) { SpoonLog.Error($"useless relics: restore {relic.Id.Entry}: {e.Message}"); }
		}
		Thrown.Clear();
		Flashed.Clear();
	}

	/// <summary>Click on a spent relic: target an enemy like a potion, then queue the synced throw.</summary>
	internal static async Task Aim(Player me, RelicModel relic, Godot.Control from)
	{
		if (NTargetManager.Instance is not { } manager) return;
		bool controller = NControllerManager.Instance?.IsUsingDirectionalNavigation == true;
		manager.StartTargeting(TargetType.AnyEnemy, from, controller ? TargetMode.Controller : TargetMode.ClickMouseToTarget,
			() => !CombatManager.Instance.IsInProgress, null);
		if (controller && me.Creature.CombatState is { } combat && NCombatRoom.Instance is { } room)
		{
			var hitboxes = combat.GetOpponentsOf(me.Creature).Where(c => c.IsAlive)
				.Select(c => room.GetCreatureNode(c)?.Hitbox).OfType<Godot.Control>().ToList();
			room.RestrictControllerNavigation(hitboxes);
			if (hitboxes.Count > 0) hitboxes[0].TryGrabFocus();
		}
		var node = await manager.SelectionFinished();
		NCombatRoom.Instance?.EnableControllerNavigation();
		if (node is not NCreature { Entity: { CombatId: { } target } }) return;
		int index = me.Relics.ToList().IndexOf(relic);
		if (index < 0 || !CanThrow(relic)) return;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
			new UselessRelicThrowAction(me, new NetUselessRelicThrow { relicIndex = (uint)index, targetId = target }));
	}

	internal static async Task Throw(GameAction action, Player player, int index, uint targetId)
	{
		if (index < 0 || index >= player.Relics.Count) return;
		var relic = player.Relics[index];
		if (!CanThrow(relic) || player.Creature.CombatState is not { } combat) return;
		var target = await combat.GetCreatureAsync(targetId, 10.0);
		if (target is not { IsAlive: true }) return;
		Thrown.Add(relic);
		relic.Status = RelicStatus.Disabled;
		SpoonLog.Info($"useless relics: {relic.Id.Entry} thrown at {target.LogName}");
		await CreatureCmd.Damage(new GameActionPlayerChoiceContext(action), new[] { target }, UselessRelicRule.Damage,
			ValueProp.Unblockable | ValueProp.Unpowered, player.Creature);
	}
}

public struct NetUselessRelicThrow : INetAction, IPacketSerializable
{
	public uint relicIndex;
	public uint targetId;

	public GameAction ToGameAction(Player player) => new UselessRelicThrowAction(player, this);

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(relicIndex, 8);
		writer.WriteUInt(targetId, 6);
	}

	public void Deserialize(PacketReader reader)
	{
		relicIndex = reader.ReadUInt(8);
		targetId = reader.ReadUInt(6);
	}

	public override string ToString() => $"useless relic throw {relicIndex} at {targetId}";
}

public class UselessRelicThrowAction(Player player, NetUselessRelicThrow action) : GameAction
{
	public override ulong OwnerId => player.NetId;

	public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

	protected override Task ExecuteAction() => UselessRelicThrow.Throw(this, player, (int)action.relicIndex, action.targetId);

	public override INetAction ToNetAction() => action;

	public override string ToString() => $"UselessRelicThrowAction for player {player.NetId}";
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Flash), typeof(IEnumerable<Creature>))]
public static class UselessRelicFlashPatch
{
	public static void Postfix(RelicModel __instance) => UselessRelicThrow.RecordFlash(__instance);
}

/// <summary>A click on a spent relic of the local player throws it instead of opening the inspect screen.</summary>
[HarmonyPatch(typeof(NRelicInventory), "OnRelicClicked")]
public static class UselessRelicClickPatch
{
	public static bool Prefix(NRelicInventory __instance, RelicModel model)
	{
		var run = ForgeLevels.CurrentRun;
		if (!SpoonUselessRelics.IsActive(run) || !UselessRelicThrow.CanThrow(model)) return true;
		if (LocalContext.GetMe(run!) is not { } me || !me.Relics.Contains(model)) return true;
		var holder = __instance.RelicNodes.FirstOrDefault(h => h.Relic.Model == model);
		if (holder == null) return true;
		_ = TaskHelper.RunSafely(UselessRelicThrow.Aim(me, model, holder));
		return false;
	}
}

/// <summary>Thrown relics leave the hook listener lists until the combat ends.</summary>
[HarmonyPatch(typeof(CombatState), nameof(CombatState.IterateHookListeners))]
public static class UselessRelicCombatHooksPatch
{
	public static void Postfix(ref IEnumerable<AbstractModel> __result)
	{
		if (UselessRelicThrow.AnyThrown) __result = __result.Where(m => m is not RelicModel r || !UselessRelicThrow.IsThrown(r)).ToList();
	}
}

[HarmonyPatch(typeof(RunState), nameof(RunState.IterateHookListeners))]
public static class UselessRelicRunHooksPatch
{
	public static void Postfix(ref IEnumerable<AbstractModel> __result)
	{
		if (UselessRelicThrow.AnyThrown) __result = __result.Where(m => m is not RelicModel r || !UselessRelicThrow.IsThrown(r)).ToList();
	}
}
