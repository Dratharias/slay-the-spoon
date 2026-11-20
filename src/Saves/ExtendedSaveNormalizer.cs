using System.Collections;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SlaySpoon.Saves;

/// <summary>
/// BaseLib's <c>ToSerializable</c> postfixes fill a card's (relic's, player's...) extended data: its card modifiers save
/// an empty list for a card without modifiers, written as "present, 0 entries". The runtime can drop that postfix when it
/// recompiles a hot method, and re-applying does not bring it back (the JIT inlines the method into its callers): the card
/// then writes "absent". Both forms read back, but the two peers' checksums differ and the game ends the co-op session
/// (state divergence).
///
/// Each registered extended save's writer is wrapped so an empty collection is written as absent: with or without the
/// postfix the bits are the same. Loading is unchanged (an absent empty list loads nothing, as before). Plain data
/// changes, no detour, so a recompile cannot undo them.
/// </summary>
public static class ExtendedSaveNormalizer
{
	private static readonly (Type Data, Type Serializable)[] Pairs =
	[
		(typeof(CardModel), typeof(SerializableCard)),
		(typeof(RelicModel), typeof(SerializableRelic)),
		(typeof(PotionModel), typeof(SerializablePotion)),
		(typeof(EnchantmentModel), typeof(SerializableEnchantment)),
		(typeof(Player), typeof(SerializablePlayer)),
		(typeof(Reward), typeof(SerializableReward)),
		(typeof(IRunState), typeof(SerializableRun)),
	];

	private static bool _done;

	/// <summary>Wraps every save registered so far; call after every mod's initializer has run.</summary>
	public static void Apply()
	{
		if (_done) return;
		_done = true;
		Type? handlers = AccessTools.TypeByName("BaseLib.Patches.Saves.ExtendedSaveHandlers`2");
		if (handlers == null) return;
		int count = 0;
		foreach (var (data, serializable) in Pairs)
		{
			try
			{
				Type closed = handlers.MakeGenericType(data, serializable);
				if (AccessTools.Property(closed, "RegisteredSaves")?.GetValue(null) is not IList saves) continue;
				for (int i = 0; i < saves.Count; i++)
				{
					object info = saves[i]!;
					Type infoType = info.GetType();
					var serializer = (Delegate)AccessTools.Property(infoType, "Serializer").GetValue(info)!;
					Type holder = serializer.GetType().GetGenericArguments()[0];
					string id = (string)AccessTools.Property(infoType, "Id").GetValue(info)!;
					object wrapped = WrapMethod.MakeGenericMethod(holder).Invoke(null, [serializer, id])!;
					saves[i] = Activator.CreateInstance(infoType, id,
						AccessTools.Property(infoType, "Getter").GetValue(info),
						AccessTools.Property(infoType, "Setter").GetValue(info),
						wrapped,
						AccessTools.Property(infoType, "Deserializer").GetValue(info));
					count++;
				}
			}
			catch (Exception e)
			{
				SpoonLog.Error($"extended save normalizer: {data.Name} failed: {(e.InnerException ?? e).Message}");
			}
		}
		SpoonLog.Info($"extended save normalizer: {count} saves wrapped");
	}

	private static readonly MethodInfo WrapMethod = AccessTools.Method(typeof(ExtendedSaveNormalizer), nameof(Wrap));

	private static Action<THolder, PacketWriter> Wrap<THolder>(Action<THolder, PacketWriter> inner, string id) where THolder : class
	{
		FieldInfo? dictionaries = AccessTools.Field(typeof(THolder), "Dictionaries");
		return (holder, writer) =>
		{
			if (dictionaries?.GetValue(holder) is IDictionary byType)
			{
				foreach (IDictionary values in byType.Values)
				{
					if (values.Contains(id) && values[id] is ICollection { Count: 0 })
						values.Remove(id);
				}
			}
			inner(holder, writer);
		};
	}
}
