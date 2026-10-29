using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

public static class Sts1CardTypes
{
	/// <summary>Canonical models of every concrete <typeparamref name="T"/> in the mod, ordered by type name.</summary>
	public static CardModel[] Of<T>() where T : Sts1Card
	{
		return typeof(T).Assembly.GetTypes()
			.Where(t => !t.IsAbstract && typeof(T).IsAssignableFrom(t))
			.OrderBy(t => t.Name, StringComparer.Ordinal)
			.Select(t => ModelDb.GetById<CardModel>(ModelDb.GetId(t)))
			.ToArray();
	}

	/// <summary>Logs every StS1 card whose game id differs from <see cref="Core.ModelIds"/> (asset names rely on it).</summary>
	public static void SelfCheck()
	{
		Type[] types = typeof(Sts1Card).Assembly.GetTypes()
			.Where(t => !t.IsAbstract && typeof(Sts1Card).IsAssignableFrom(t))
			.ToArray();
		foreach (Type type in types)
		{
			string expected = Core.ModelIds.FromClassName(type.Name);
			string actual = ModelDb.GetId(type).Entry;
			if (expected != actual)
			{
				SpoonLog.Error($"{type.Name}: game id {actual} differs from {expected}; its text and portrait won't load");
			}
		}
		SpoonLog.Info($"StS1 cards: {types.Length}");
	}
}
