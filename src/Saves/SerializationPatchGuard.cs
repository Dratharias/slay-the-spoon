using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
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
/// detours, then checks a test card every few seconds and re-applies everything when the card's bits come out wrong.
/// </summary>
public static class SerializationPatchGuard
{
	private const int WarmUpCalls = 200;
	private const double WarmUpWait = 3, CheckEvery = 5;

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

	private static async void Run()
	{
		try
		{
			await Wait(2);
			WarmUp();
			await Wait(WarmUpWait);
			int count = Reapply();
			SpoonLog.Info($"serialization guard: {count} serializer patches re-applied, card check {(CardIsWhole() ? "ok" : "FAILED")}");
			while (true)
			{
				await Wait(CheckEvery);
				if (CardIsWhole()) continue;
				Reapply();
				SpoonLog.Info($"serialization guard: a serializer lost its patch, re-applied (card check {(CardIsWhole() ? "ok" : "still failing")})");
			}
		}
		catch (Exception e)
		{
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

	private static int Reapply()
	{
		int count = 0;
		foreach (MethodBase method in Targets())
		{
			try
			{
				object? info = GetPatchInfo!.Invoke(null, [method]);
				if (info == null) continue;
				UpdateWrapper!.Invoke(null, [method, info]);
				count++;
			}
			catch (Exception e)
			{
				SpoonLog.Error($"serialization guard: re-applying {method.DeclaringType?.Name}.{method.Name} failed: {(e.InnerException ?? e).Message}");
			}
		}
		return count;
	}

	/// <summary>
	/// A test card must write the vanilla fields plus whatever the patches add, and read back the same number of bits.
	/// With no patch on the card serializer there is nothing to check.
	/// </summary>
	private static bool CardIsWhole()
	{
		var serialize = AccessTools.Method(typeof(SerializableCard), nameof(SerializableCard.Serialize));
		if (Harmony.GetPatchInfo(serialize) == null) return true;
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
	private static int ExtraBits(SerializableCard card)
	{
		Type? handlers = AccessTools.TypeByName("BaseLib.Patches.Saves.ExtendedSaveHandlers`2")
			?.MakeGenericType(typeof(CardModel), typeof(SerializableCard));
		MethodInfo? write = handlers == null ? null : AccessTools.Method(handlers, "Write");
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
