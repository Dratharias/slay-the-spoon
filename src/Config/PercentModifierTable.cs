using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>Maps (family, percent) to the concrete modifier class.</summary>
public static class PercentModifierTable
{
	private static readonly Type[] Families = { typeof(SpoonMonsterHp), typeof(SpoonMonsterDamage), typeof(SpoonMatriochka) };

	private static readonly Dictionary<(Type Family, int Percent), Type> Types = BuildTable();

	public static ModifierModel CanonicalFor(Type familyType, int percent)
	{
		if (!Types.TryGetValue((familyType, percent), out Type? type))
		{
			throw new ArgumentException($"No {familyType.Name} modifier for {percent}%");
		}
		return ModelDb.GetById<ModifierModel>(ModelDb.GetId(type));
	}

	/// <summary>Fails loudly at startup if a percent of the grid has no class in a family.</summary>
	public static void SelfCheck()
	{
		foreach (Type family in Families)
		{
			IReadOnlyList<int> values = family == typeof(SpoonMatriochka) ? MatriochkaRule.AllRerolls : ScalingMath.AllPercents;
			int[] missing = values.Where(p => !Types.ContainsKey((family, p))).ToArray();
			if (missing.Length > 0)
			{
				throw new InvalidOperationException($"{family.Name} has no class for: {string.Join(", ", missing)}");
			}
		}
		int hp = Types.Keys.Count(k => k.Family == typeof(SpoonMonsterHp));
		int damage = Types.Keys.Count(k => k.Family == typeof(SpoonMonsterDamage));
		SpoonLog.Info($"percent modifiers: hp={hp} damage={damage} matriochka={Types.Keys.Count(k => k.Family == typeof(SpoonMatriochka))}");
	}

	private static Dictionary<(Type, int), Type> BuildTable()
	{
		Dictionary<(Type, int), Type> table = new();
		foreach (Type type in typeof(PercentModifierTable).Assembly.GetTypes())
		{
			if (type.IsAbstract || !Families.Contains(type.BaseType))
			{
				continue;
			}
			table.Add((type.BaseType!, SpoonPercentModifier.PercentOf(type)), type);
		}
		return table;
	}
}
