using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SlaySpoon.Cards;

/// <summary>
/// The game finds [SavedProperty] members only on its own model types, through a list built at compile
/// time, so a mod card's saved values would be dropped from saves and co-op sync. This adds the mod's
/// models to that cache. They must reuse property names a vanilla model already saves: the network ids
/// are sized from the vanilla names, so a new name is refused here and the card simply isn't saved.
/// </summary>
public static class Sts1SavedProperties
{
	public static void Register()
	{
		IEnumerable<Type> models = typeof(Sts1SavedProperties).Assembly.GetTypes()
			.Where(t => !t.IsAbstract && typeof(AbstractModel).IsAssignableFrom(t));
		foreach (Type type in models)
		{
			List<string> names = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(p => p.GetCustomAttribute<SavedPropertyAttribute>() != null)
				.Select(p => p.Name)
				.ToList();
			if (names.Count == 0)
			{
				continue;
			}
			List<string> unknown = names.Where(n => !IsVanillaName(n)).ToList();
			if (unknown.Count > 0)
			{
				SpoonLog.Error($"{type.Name}: saved properties {string.Join(", ", unknown)} have no vanilla name; not saved");
				continue;
			}
			SavedPropertiesTypeCache.InjectTypeIntoCache(type);
		}
	}

	private static bool IsVanillaName(string name)
	{
		try
		{
			SavedPropertiesTypeCache.GetNetIdForPropertyName(name);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
	}
}
