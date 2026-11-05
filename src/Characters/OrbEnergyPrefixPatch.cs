using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Characters.Pools;

namespace SlaySpoon.Characters;

/// <summary>
/// An orb's hover tip passes its owner's card pool <c>Title</c> as the energy icon prefix, which is right for the
/// vanilla pools (the title is the energy color) but not for the Watcher's (<c>sts1_watcher</c> has no
/// <c>sts1_watcher_energy_icon.png</c>). A Watcher holding an orb would show a missing image: the getter reads the
/// pool's energy color instead for her pool.
/// </summary>
// Harmony treats any method named Prefix, Postfix, Transpiler, Finalizer or Prepare as a patch: helpers need other names.
[HarmonyPatch(typeof(OrbModel), nameof(OrbModel.HoverTips), MethodType.Getter)]
public static class OrbEnergyPrefixPatch
{
	private static readonly System.Reflection.MethodInfo TitleGetter =
		AccessTools.PropertyGetter(typeof(CardPoolModel), nameof(CardPoolModel.Title));

	public static string EnergyIconPrefix(CardPoolModel pool) => pool is Sts1WatcherCardPool ? pool.EnergyColorName : pool.Title;

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int replaced = 0;
		foreach (var instruction in instructions)
		{
			if (instruction.Calls(TitleGetter))
			{
				replaced++;
				yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbEnergyPrefixPatch), nameof(EnergyIconPrefix)))
					.WithLabels(instruction.labels).WithBlocks(instruction.blocks);
				continue;
			}
			yield return instruction;
		}
		if (replaced != 1)
		{
			SpoonLog.Error($"OrbEnergyPrefixPatch: expected one CardPoolModel.Title call in OrbModel.HoverTips, found {replaced}.");
		}
	}
}
