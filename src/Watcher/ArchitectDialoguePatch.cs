using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using SlaySpoon.Characters;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher;

/// <summary>
/// The Architect's victory-scene dialogue for the Watcher (owner decision 2026-11-05: short, EN/FR, "you do not really
/// exist", a nod to the mod). Without an entry the scene skips straight to the proceed button. Both dialogues repeat
/// (the "r" keys in <c>ancients.json</c>), so one is drawn on the event RNG at every win count; her four hits reuse
/// vanilla attack effects (<see cref="Sts1Watcher.GetArchitectAttackVfx"/>).
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
internal static class ArchitectDialoguePatch
{
	private static void Postfix(AncientDialogueSet __result)
	{
		try
		{
			// Empty SFX paths fall back to the game's default line sound, as vanilla's own lines do.
			__result.CharacterDialogues[ModelDb.Character<Sts1Watcher>().Id.Entry] = new[]
			{
				new AncientDialogue("", "", "") { VisitIndex = 0, EndAttackers = ArchitectAttackers.Both },
				new AncientDialogue("", "") { VisitIndex = 0, EndAttackers = ArchitectAttackers.Both },
			};
		}
		catch (Exception e)
		{
			SpoonLog.Error($"watcher: Architect dialogue failed: {e}");
		}
	}
}
