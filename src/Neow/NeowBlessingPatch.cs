using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Neow;

/// <summary>
/// With <see cref="SpoonNeowBlessing"/> ticked, Neow offers its blessings as in a Standard run. Neow picks its
/// branch on <c>RunState.Modifiers.Count</c>; the transpiler routes that read through <see cref="NeowRule"/>.
/// When vanilla modifiers bring their own Neow options, those come first and the blessings replace the
/// event's "done" page. Every client runs the same steps on its own Neow, so co-op stays in sync.
/// </summary>
[HarmonyPatch]
public static class NeowBlessingPatch
{
	private static readonly MethodInfo CountGetter =
		AccessTools.PropertyGetter(typeof(IReadOnlyCollection<ModifierModel>), nameof(IReadOnlyCollection<ModifierModel>.Count));

	[ThreadStatic] internal static bool ForceBlessings;

	public static IEnumerable<MethodBase> TargetMethods()
	{
		Type neow = typeof(MegaCrit.Sts2.Core.Models.Events.Neow);
		yield return AccessTools.Method(neow, "GenerateInitialOptions");
		yield return AccessTools.PropertyGetter(neow, nameof(EventModel.InitialDescription));
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(CountGetter))
			{
				replaced++;
				yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NeowBlessingPatch), nameof(VisibleCount)))
					.MoveLabelsFrom(instruction);
				continue;
			}
			yield return instruction;
		}
		if (replaced == 0)
		{
			SpoonLog.Error($"Neow's Blessings: no modifier count read in {original.Name}; the option does nothing there");
		}
	}

	private static int VisibleCount(IReadOnlyCollection<ModifierModel> modifiers)
	{
		return NeowRule.VisibleModifierCount(modifiers.Count, modifiers.Count(m => m is SpoonAct4),
			modifiers.Any(m => m is SpoonNeowBlessing),
			modifiers.Any(HasNeowOption), ForceBlessings);
	}

	internal static bool HasNeowOption(ModifierModel modifier)
	{
		return AccessTools.Method(modifier.GetType(), nameof(ModifierModel.GenerateNeowOption))?.DeclaringType
			!= typeof(ModifierModel);
	}
}

/// <summary>After the last modifier option, Neow shows the blessings instead of finishing the event.</summary>
[HarmonyPatch(typeof(EventModel), "SetEventFinished")]
public static class NeowChainPatch
{
	private static readonly Func<AncientEventModel, IReadOnlyList<EventOption>> GenerateOptions =
		AccessTools.MethodDelegate<Func<AncientEventModel, IReadOnlyList<EventOption>>>(
			AccessTools.Method(typeof(AncientEventModel), "GenerateInitialOptionsWrapper"));

	private static readonly Action<EventModel, LocString, IEnumerable<EventOption>> SetEventState =
		AccessTools.MethodDelegate<Action<EventModel, LocString, IEnumerable<EventOption>>>(
			AccessTools.Method(typeof(EventModel), "SetEventState"));

	/// <summary>Neows that already went from their modifier options to the blessings.</summary>
	private static readonly ConditionalWeakTable<EventModel, object> Chained = new();

	public static bool Prefix(EventModel __instance)
	{
		if (__instance is not MegaCrit.Sts2.Core.Models.Events.Neow neow || neow.Owner == null)
		{
			return true;
		}
		IReadOnlyList<ModifierModel> modifiers = neow.Owner.RunState.Modifiers;
		if (!modifiers.Any(m => m is SpoonNeowBlessing) || !modifiers.Any(NeowBlessingPatch.HasNeowOption)
			|| Chained.TryGetValue(neow, out _))
		{
			return true;
		}
		Chained.Add(neow, new object());

		IReadOnlyList<EventOption> options;
		LocString description;
		NeowBlessingPatch.ForceBlessings = true;
		try
		{
			options = GenerateOptions(neow);
			description = neow.InitialDescription;
		}
		finally
		{
			NeowBlessingPatch.ForceBlessings = false;
		}
		SetEventState(neow, description, options);
		return false;
	}
}
