using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SlaySpoon.Config;

namespace SlaySpoon;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	public const string Version = "0.1.0";

	public static void Initialize()
	{
		PercentModifierTable.SelfCheck();
		new Harmony("slayspoon").PatchAll(typeof(ModEntry).Assembly);
		SpoonLog.Info($"Slay the Spoon {Version} loaded");
	}
}
