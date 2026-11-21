using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Config;

namespace SlaySpoon;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	public const string Version = "1.8.20";

	public static void Initialize()
	{
		PercentModifierTable.SelfCheck();
		Sts1CardTypes.SelfCheck();
		try
		{
			Act4.Sts1EndingAct.SelfCheck();
		}
		catch (Exception e)
		{
			SpoonLog.Error($"Sts1EndingAct self-check failed: {e.Message}");
		}
		Act4.Act4Routing.Register();
		LinuxHarmonyFix.Apply();
		PatchEach(new Harmony("slayspoon"));
		Saves.SerializationPatchGuard.Start();
		SpoonLog.Info($"Slay the Spoon {Version} loaded");
	}

	// Patches class by class so one target broken by a game update is logged with every other one, instead of
	// stopping at the first; the mod still refuses to load when any patch fails.
	private static void PatchEach(Harmony harmony)
	{
		List<string> failed = [];
		foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(ModEntry).Assembly))
		{
			try
			{
				harmony.CreateClassProcessor(type).Patch();
			}
			catch (Exception e)
			{
				failed.Add(type.FullName ?? type.Name);
				SpoonLog.Error($"patch {type.FullName} failed: {e.InnerException?.Message ?? e.Message}");
			}
		}
		if (failed.Count > 0)
		{
			harmony.UnpatchAll(harmony.Id);
			throw new InvalidOperationException($"{failed.Count} patch classes failed: {string.Join(", ", failed)}");
		}
	}
}
