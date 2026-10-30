using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// <c>NRestSiteCharacter._Ready</c> picks the characters' loop animation with a switch on
/// <c>Player.RunState.CurrentActIndex</c> and throws "Unexpected act" past index 2, which would abort it before
/// the Spine animation, the Necrobinder fire and the hitbox signals at the Ending's rest site. The transpiler
/// routes that index read through <see cref="VanillaActIndex.For"/>, so act 4 plays act 3's glory_loop
/// (the Ending already borrows Glory's assets, see <see cref="EndingActAssetsPatch"/>).
/// </summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
internal static class EndingRestSiteCharacterPatch
{
	private static readonly MethodInfo Map = AccessTools.Method(typeof(VanillaActIndex), nameof(VanillaActIndex.For));

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int rewritten = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
			if (instruction.operand is MethodInfo { Name: "get_CurrentActIndex" })
			{
				rewritten++;
				yield return new CodeInstruction(OpCodes.Call, Map);
			}
		}
		if (rewritten == 0)
		{
			SpoonLog.Error("act4: no CurrentActIndex read in NRestSiteCharacter._Ready; act 4 rest site characters won't animate");
		}
	}
}
