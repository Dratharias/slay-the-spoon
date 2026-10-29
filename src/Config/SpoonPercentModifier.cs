using System.Collections.Concurrent;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Config;

/// <summary>
/// A run modifier carrying a percentage. The percent is fixed by the concrete class, so the game's own
/// modifier-id sync and save handle it without any saved property.
/// </summary>
public abstract class SpoonPercentModifier : ModifierModel
{
	private static readonly ConcurrentDictionary<Type, int> Percents = new();

	/// <summary>Read from the class name suffix (SpoonMonsterHp150 → 150), the single source of the value; parsed once per type.</summary>
	public int Percent => Percents.GetOrAdd(GetType(), PercentOf);

	/// <summary>Percent encoded in a concrete family member's class name.</summary>
	public static int PercentOf(Type type)
	{
		return int.Parse(type.Name.Substring(type.BaseType!.Name.Length));
	}

	/// <summary>Localization key shared by every member of the family.</summary>
	protected abstract string FamilyKey { get; }

	public override LocString Title => new LocString("modifiers", FamilyKey + ".title");

	public override LocString Description
	{
		get
		{
			LocString description = new LocString("modifiers", FamilyKey + ".description");
			description.Add("Percent", Percent);
			return description;
		}
	}

	/// <summary>The abstract family type (e.g. <see cref="SpoonMonsterHp"/>) this modifier belongs to.</summary>
	public Type FamilyType => GetType().BaseType!;

	/// <summary>Members of a family are equivalent whatever their percent, so the list ticks the family row.</summary>
	public override bool IsEquivalent(ModifierModel other)
	{
		return IsCanonical == other.IsCanonical && other is SpoonPercentModifier o && o.FamilyType == FamilyType;
	}
}
