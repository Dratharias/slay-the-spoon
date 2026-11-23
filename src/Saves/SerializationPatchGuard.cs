using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SlaySpoon.Saves;

/// <summary>
/// Keeps Harmony patches on network serializers alive. The .NET runtime recompiles a hot method (tiered compilation),
/// and the game's MonoMod does not carry the Harmony detour over to the new code: the patch silently stops running.
/// BaseLib postfixes <c>Serialize</c> / <c>Deserialize</c> of the run's serializable types to append its extended data;
/// when one peer loses the write half and the other still reads, every message after the first card is shifted (co-op
/// state divergence, "ModelId entry ID ... is out of range" while syncing players).
///
/// At startup the guard calls every patched serializer until the runtime promotes it to its final tier, re-applies the
/// detours, then checks a test card on every frame and re-applies everything when the card's bits come out wrong.
/// BaseLib's <c>ToSerializable</c> postfix can be lost the same way (re-applying does not bring it back: the JIT inlines
/// the method into its callers); <see cref="ExtendedSaveNormalizer"/> makes that loss harmless.
/// </summary>
public static class SerializationPatchGuard
{
	// .NET 9 recompiles a hot method more than once (tier 0, instrumented tier, final tier), and each step drops the
	// detour, so the warm-up runs several rounds, re-applying after each one.
	private const int WarmUpCalls = 500, WarmUpRounds = 6;
	private const double FirstWait = 0.25, RoundWait = 1;

	private static readonly MethodInfo? GetPatchInfo =
		AccessTools.Method(AccessTools.TypeByName("HarmonyLib.HarmonySharedState"), "GetPatchInfo");

	private static readonly MethodInfo? UpdateWrapper =
		AccessTools.Method(AccessTools.TypeByName("HarmonyLib.PatchFunctions"), "UpdateWrapper");

	public static void Start()
	{
		if (GetPatchInfo == null || UpdateWrapper == null)
		{
			SpoonLog.Error("serialization guard: Harmony internals not found, guard off");
			return;
		}
		Run();
	}

	private static int _lost;
	private static bool _detours;
	private static ActionExecutor? _executor;
	private static bool _timed;

	/// <summary>Full detour sweep (see <see cref="DetourGuard.SweepAll"/>). The executor's event needs no patch, so it
	/// cannot lose a detour itself; the checksum prefix is watched by the frame slices like any patch.</summary>
	public static void SweepNow()
	{
		if (!_detours) return;
		try
		{
			long start = System.Diagnostics.Stopwatch.GetTimestamp();
			DetourGuard.SweepAll(m => Reapply([m]) == 1);
			if (_timed) return;
			_timed = true;
			SpoonLog.Info($"detour guard: full sweep takes {System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds:0.00} ms");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"detour guard sweep failed: {e.Message}");
		}
	}

	private static void WatchExecutor()
	{
		ActionExecutor? executor = RunManager.Instance?.ActionExecutor;
		if (ReferenceEquals(executor, _executor)) return;
		_executor = executor;
		if (executor != null) executor.BeforeActionExecuted += _ => SweepNow();
	}

	private static async void Run()
	{
		try
		{
			// ModelDb.AllCards caches its first answer, so it must not be read before the model DB is built: the
			// serialization cache is initialized right after it (OneTimeInitialization.ExecuteEssential).
			while (ModelIdSerializationCache.EntryIdBitSize == 0) await Wait(FirstWait);
			ExtendedSaveNormalizer.Apply();
			// A recompile can drop the detour at any moment, and a co-op message written in between is shifted: check on
			// every frame from now on (one card round trip, a few microseconds).
			var tree = (SceneTree)Engine.GetMainLoop();
			_detours = DetourGuard.Init();
			tree.ProcessFrame += CheckFrame;
			int count = 0;
			for (int round = 0; round < WarmUpRounds; round++)
			{
				WarmUp();
				await Wait(RoundWait);
				count = Reapply();
			}
			SpoonLog.Info($"serialization guard: {count} serializer patches re-applied, card check {(CardIsWhole() ? "ok" : "FAILED")}");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"serialization guard stopped: {e}");
		}
	}

	private static void CheckFrame()
	{
		try
		{
			if (_detours) DetourGuard.Tick(m => Reapply([m]) == 1);
			if (_detours) WatchExecutor();
			if (!ForgeMaxIsPatched())
			{
				int count = ReapplyOn(typeof(CardModel));
				SpoonLog.Info($"patch guard: CardModel lost its Forge patches, {count} re-applied (check {(ForgeMaxIsPatched() ? "ok" : "still failing")})");
			}
			if (CardIsWhole()) return;
			Reapply();
			_lost++;
			SpoonLog.Info($"serialization guard: a serializer lost its patch ({_lost}), re-applied (card check {(CardIsWhole() ? "ok" : "still failing")})");
		}
		catch (Exception e)
		{
			((SceneTree)Engine.GetMainLoop()).ProcessFrame -= CheckFrame;
			SpoonLog.Error($"serialization guard stopped: {e}");
		}
	}

	private static async Task Wait(double seconds)
	{
		var tree = (SceneTree)Engine.GetMainLoop();
		await tree.ToSignal(tree.CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
	}

	/// <summary>Patched Serialize/Deserialize methods of packet types, whoever patched them.</summary>
	private static List<MethodBase> Targets() => Harmony.GetAllPatchedMethods()
		.Where(m => m.Name is "Serialize" or "Deserialize" && m.DeclaringType is { } t
			&& typeof(IPacketSerializable).IsAssignableFrom(t) && !t.IsAbstract && !t.ContainsGenericParameters)
		.ToList();

	// Round-trips valid samples of the hot serializers (cards, relics, potions) until the runtime promotes them. Blank
	// instances are not an option: a blank card froze the game inside the patched Serialize.
	private static void WarmUp()
	{
		IPacketSerializable[] samples =
		[
			new SerializableCard { Id = ModelDb.AllCards.First().Id, FloorAddedToDeck = 1 },
			new SerializableRelic { Id = ModelDb.AllRelics.First().Id, FloorAddedToDeck = 1 },
			new SerializablePotion { Id = ModelDb.AllPotions.First().Id, SlotIndex = 0 },
		];
		foreach (IPacketSerializable sample in samples)
		{
			try
			{
				for (int i = 0; i < WarmUpCalls; i++)
				{
					var writer = new PacketWriter { WarnOnGrow = false };
					sample.Serialize(writer);
					var copy = (IPacketSerializable)Activator.CreateInstance(sample.GetType())!;
					copy.Deserialize(Reader(writer.Buffer[..writer.BytePosition]));
				}
			}
			catch (Exception e)
			{
				SpoonLog.Error($"serialization guard: warming {sample.GetType().Name} failed: {e.Message}");
			}
		}
	}

	private static int Reapply() => Reapply(Targets());

	/// <summary>
	/// Re-applies every Harmony patch on methods declared by <paramref name="declaring"/>. Cheap enough to call before a
	/// rare action that needs them (a Forge fusion), so the action never runs on a dropped detour.
	/// </summary>
	public static int ReapplyOn(Type declaring) =>
		GetPatchInfo == null || UpdateWrapper == null ? 0
			: Reapply(Harmony.GetAllPatchedMethods().Where(m => m.DeclaringType == declaring).ToList());

	/// <summary>Methods whose patches Harmony cannot rebuild (BaseLib's <c>NRewardButton._Ready</c>: "invalid program"); tried once.</summary>
	private static readonly HashSet<MethodBase> Unrebuildable = [];

	private static int Reapply(List<MethodBase> targets)
	{
		int count = 0;
		foreach (MethodBase method in targets)
		{
			if (Unrebuildable.Contains(method)) continue;
			try
			{
				object? info = GetPatchInfo!.Invoke(null, [method]);
				if (info == null) continue;
				UpdateWrapper!.Invoke(null, [method, info]);
				count++;
			}
			catch (Exception e)
			{
				Unrebuildable.Add(method);
				SpoonLog.Error($"serialization guard: re-applying {method.DeclaringType?.Name}.{method.Name} failed, not retried: {(e.InnerException ?? e).Message}");
			}
		}
		return count;
	}

	private static readonly MethodInfo MaxGetter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel));

	private static CardModel? _forgeSample;

	/// <summary>
	/// The Forge's <c>MaxUpgradeLevel</c> postfix answers +9 inside an open fusion scope. The getter is among the game's
	/// hottest, so its detour is the likeliest to be dropped: a fusion then threw "cannot be upgraded past its
	/// MaxUpgradeLevel" on one peer only, which shifted every later player choice (co-op crash at the rest site).
	/// </summary>
	private static bool ForgeMaxIsPatched()
	{
		if (Harmony.GetPatchInfo(MaxGetter) == null) return true;
		_forgeSample ??= ModelDb.AllCards.FirstOrDefault(SlaySpoon.Forge.ForgeLevels.Fusable);
		if (_forgeSample == null) return true;
		using (SlaySpoon.Forge.ForgeLevels.Open())
			return _forgeSample.MaxUpgradeLevel == Core.ForgeRule.MaxLevel;
	}

	/// <summary>
	/// A test card must write the vanilla fields plus whatever the patches add, and read back the same number of bits.
	/// With no patch on the card serializer there is nothing to check.
	/// </summary>
	private static bool CardIsWhole()
	{
		if (Harmony.GetPatchInfo(CardSerialize) == null) return true;
		var card = new SerializableCard { Id = ModelDb.AllCards.First().Id, FloorAddedToDeck = 1 };
		var writer = new PacketWriter { WarnOnGrow = false };
		card.Serialize(writer);
		int written = writer.BitPosition;
		int vanilla = ModelIdSerializationCache.EntryIdBitSize + 8 + 3 + 8;
		if (written == vanilla && ExtraBits(card) > 0) return false;
		var reader = Reader(writer.Buffer[..writer.BytePosition]);
		new SerializableCard().Deserialize(reader);
		return reader.BitPosition == written;
	}

	/// <summary>What BaseLib's own card writer appends, measured by calling it directly (0 without BaseLib).</summary>
	private static readonly MethodInfo? ExtraWrite = AccessTools.TypeByName("BaseLib.Patches.Saves.ExtendedSaveHandlers`2")
		?.MakeGenericType(typeof(CardModel), typeof(SerializableCard)) is { } handlers ? AccessTools.Method(handlers, "Write") : null;

	private static readonly MethodInfo CardSerialize = AccessTools.Method(typeof(SerializableCard), nameof(SerializableCard.Serialize));

	private static int ExtraBits(SerializableCard card)
	{
		MethodInfo? write = ExtraWrite;
		if (write == null) return 0;
		var writer = new PacketWriter { WarnOnGrow = false };
		write.Invoke(null, [card, writer]);
		return writer.BitPosition;
	}

	private static PacketReader Reader(byte[] bytes)
	{
		var reader = new PacketReader();
		reader.Reset(bytes);
		return reader;
	}
}
