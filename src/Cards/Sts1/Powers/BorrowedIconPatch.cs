using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Powers;

[HarmonyPatch(typeof(PowerModel))]
public static class BorrowedIconPatch
{
	// The big-icon getter caches its path in this field. Callers where the JIT inlined the getter read the field
	// directly, past any postfix, and showed the game's "NOPE" missing-power image: seed the field instead.
	private static readonly AccessTools.FieldRef<PowerModel, string?> ResolvedBigIcon =
		AccessTools.FieldRefAccess<PowerModel, string?>("_resolvedBigIconPath");

	[HarmonyPostfix]
	[HarmonyPatch(nameof(PowerModel.PackedIconPath), MethodType.Getter)]
	public static void PackedIcon(PowerModel __instance, ref string __result)
	{
		if (__instance is IBorrowedIcon borrowed)
		{
			__result = Source(borrowed).PackedIconPath;
		}
		else if (Sts1PowerIcons.Has(__instance.Id.Entry))
		{
			__result = Sts1PowerIcons.Packed(__instance.Id.Entry);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PowerModel.ResolvedBigIconPath), MethodType.Getter)]
	public static void BigIcon(PowerModel __instance)
	{
		Seed(__instance);
	}

	/// <summary>Points a borrowed power's cached big-icon path at its source's icon, or an own-icon power's at its StS1 icon.</summary>
	public static void Seed(PowerModel power)
	{
		if (power is IBorrowedIcon borrowed)
		{
			ResolvedBigIcon(power) = Source(borrowed).ResolvedBigIconPath;
		}
		else if (Sts1PowerIcons.Has(power.Id.Entry))
		{
			ResolvedBigIcon(power) = Sts1PowerIcons.Big(power.Id.Entry);
		}
	}

	/// <summary>Resolve explicitly at the UI call site: JIT inlining can bypass the PackedIconPath patch.</summary>
	public static Texture2D CombatIcon(PowerModel power)
	{
		string path = power is IBorrowedIcon borrowed ? Source(borrowed).PackedIconPath
			: Sts1PowerIcons.Has(power.Id.Entry) ? Sts1PowerIcons.Packed(power.Id.Entry)
			: power.PackedIconPath;
		return ResourceLoader.Load<Texture2D>(path);
	}

	private static PowerModel Source(IBorrowedIcon borrowed)
	{
		return ModelDb.GetById<PowerModel>(ModelDb.GetId(borrowed.IconSource));
	}
}

/// <summary>Seeds every canonical borrowed power once the game has resolved its icon paths; combat copies inherit it.</summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Preload))]
public static class BorrowedIconPreloadPatch
{
	[HarmonyPostfix]
	public static void Postfix()
	{
		foreach (PowerModel power in ModelDb.AllPowers)
		{
			BorrowedIconPatch.Seed(power);
		}
	}
}

/// <summary>
/// Replace the combat UI's Icon getter call before JIT compilation. Patching only PackedIconPath is insufficient:
/// PowerModel.Icon may already contain its inlined original body, producing a missing atlas sprite (NOPE).
/// </summary>
[HarmonyPatch(typeof(NPower), "Reload")]
internal static class BorrowedCombatIconPatch
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var iconGetter = AccessTools.PropertyGetter(typeof(PowerModel), nameof(PowerModel.Icon));
		var resolve = AccessTools.Method(typeof(BorrowedIconPatch), nameof(BorrowedIconPatch.CombatIcon));
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(iconGetter))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = resolve;
				replaced++;
			}
			yield return instruction;
		}
		if (replaced != 1) throw new InvalidOperationException($"Expected one NPower.Reload Icon call, found {replaced}");
	}
}
