using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;

namespace SlaySpoon.Saves;

/// <summary>
/// Generic watch over every Harmony detour in the process (the mod's, BaseLib's, other mods'). In game, the runtime
/// sometimes installs new native code for a patched method without the detour (seen on <c>SerializableCard.Serialize</c>,
/// <c>CardModel.MaxUpgradeLevel</c> and <c>CardEnergyCost.GetWithModifiers</c>: a Cursed Deck curse cost 1 on the host and
/// stayed unplayable on the client, co-op state divergence at the first turn end). The patch then stops running on one
/// peer only.
///
/// MonoMod tells where a method's current native code starts; a detoured method starts with a jump. Each frame the guard
/// reads a slice of the patched methods and re-applies the patches of any method whose code no longer jumps. A method
/// inlined into a caller cannot be seen this way (see the functional checks in <see cref="SerializationPatchGuard"/>).
/// </summary>
internal static class DetourGuard
{
	private const int PerFrame = 48;

	private static Func<MethodBase, IntPtr>? _body;
	private static List<MethodBase> _methods = [];
	private static int _next, _sweeps;
	private static readonly Dictionary<MethodBase, int> Losses = [];

	/// <summary>False when MonoMod's internals are missing or the CPU is not x64 (no known jump encoding).</summary>
	public static bool Init()
	{
		if (RuntimeInformation.ProcessArchitecture != Architecture.X64) return false;
		try
		{
			Type? triple = typeof(Harmony).Assembly.GetType("MonoMod.Core.Platforms.PlatformTriple");
			object? current = triple?.GetProperty("Current", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
			MethodInfo? get = triple?.GetMethod("GetNativeMethodBody", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				[typeof(MethodBase)]);
			if (current == null || get == null) return false;
			_body = (Func<MethodBase, IntPtr>)Delegate.CreateDelegate(typeof(Func<MethodBase, IntPtr>), current, get);
			Refresh();
			int lost = _methods.Count(m => !Detoured(m));
			SpoonLog.Info($"detour guard: watching {_methods.Count} patched methods ({lost} without a jump at start)");
			return true;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"detour guard off: {(e.InnerException ?? e).Message}");
			_body = null;
			return false;
		}
	}

	private static void Refresh() => _methods = Harmony.GetAllPatchedMethods()
		.Where(m => !m.IsAbstract && !m.ContainsGenericParameters && m.DeclaringType is { ContainsGenericParameters: false })
		.ToList();

	/// <summary>Checks the next slice; re-applies and logs every method found without its jump.</summary>
	public static void Tick(Func<MethodBase, bool> reapply)
	{
		if (_body == null || _methods.Count == 0) return;
		for (int i = 0; i < PerFrame && _methods.Count > 0; i++)
		{
			if (_next >= _methods.Count)
			{
				_next = 0;
				// Patches added after load (other mods, late PatchAll) join the watch every 200 sweeps.
				if (++_sweeps % 200 == 0) Refresh();
			}
			MethodBase method = _methods[_next++];
			if (Detoured(method)) continue;
			bool ok = reapply(method) && Detoured(method);
			Losses[method] = Losses.GetValueOrDefault(method) + 1;
			if (Losses[method] <= 3)
				SpoonLog.Info($"detour guard: {method.DeclaringType?.Name}.{method.Name} lost its detour ({Losses[method]}), re-applied: {(ok ? "ok" : "STILL MISSING")}");
		}
	}

	/// <summary>
	/// Checks every watched method now. Called before each game action and each checksum: a detour lost between two
	/// frame slices otherwise changes what one peer computes or hashes (Q5 line 22: a playable curse's cost read
	/// through <c>CardEnergyCost.GetWithModifiers</c> was −1 on one peer in the turn start checksum, 1 on the other).
	/// </summary>
	public static void SweepAll(Func<MethodBase, bool> reapply)
	{
		if (_body == null) return;
		foreach (MethodBase method in _methods)
		{
			if (Detoured(method)) continue;
			bool ok = reapply(method) && Detoured(method);
			Losses[method] = Losses.GetValueOrDefault(method) + 1;
			if (Losses[method] <= 3)
				SpoonLog.Info($"detour guard: {method.DeclaringType?.Name}.{method.Name} lost its detour ({Losses[method]}), re-applied before an action or checksum: {(ok ? "ok" : "STILL MISSING")}");
		}
	}

	/// <summary>Whether the method's current native code starts with a jump (MonoMod's x64 detour encodings).</summary>
	private static unsafe bool Detoured(MethodBase method)
	{
		try
		{
			byte* p = (byte*)_body!(method);
			if (p == null) return true;
			return p[0] == 0xE9 // jmp rel32
				|| (p[0] == 0xFF && p[1] == 0x25) // jmp [rip+disp32]
				|| (p[0] == 0x48 && p[1] == 0xB8 && p[10] == 0xFF && p[11] == 0xE0); // movabs rax, imm64; jmp rax
		}
		catch
		{
			return true;
		}
	}
}
