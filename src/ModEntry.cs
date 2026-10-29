using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Config;

namespace SlaySpoon;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	public const string Version = "0.3.0";

	public static void Initialize()
	{
		PercentModifierTable.SelfCheck();
		Sts1CardTypes.SelfCheck();
		Sts1SavedProperties.Register();
		LinuxHarmonyFix.Apply();
		new Harmony("slayspoon").PatchAll(typeof(ModEntry).Assembly);
		SpoonLog.Info($"Slay the Spoon {Version} loaded");
	}
}
