using System.Runtime.InteropServices;

namespace SlaySpoon;

/// <summary>
/// On Linux, Harmony's native exception helper (mm-exhelper.so) needs libgcc_s's _Unwind_* symbols,
/// but the game loads libgcc_s only in a local scope, so every patch fails with
/// "undefined symbol: _Unwind_RaiseException". Re-opening it with RTLD_GLOBAL exposes the symbols.
/// </summary>
public static class LinuxHarmonyFix
{
	private const int RtldNow = 0x2;
	private const int RtldGlobal = 0x100;

	[DllImport("libdl.so.2")]
	private static extern IntPtr dlopen(string fileName, int flags);

	public static void Apply()
	{
		if (!OperatingSystem.IsLinux())
		{
			return;
		}
		IntPtr handle = dlopen("libgcc_s.so.1", RtldNow | RtldGlobal);
		SpoonLog.Info(handle == IntPtr.Zero ? "libgcc_s.so.1 not found; Harmony patches may fail" : "libgcc_s.so.1 made global for Harmony");
	}
}
