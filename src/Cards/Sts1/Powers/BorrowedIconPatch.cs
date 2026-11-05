using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
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
