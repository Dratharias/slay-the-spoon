namespace SlaySpoon.Core;

/// <summary>Pure rules of the Matriochka modifier (see docs/superpowers/specs/2026-11-14-matriochka-design.md).</summary>
public static class MatriochkaRule
{
	/// <summary>0-based act index whose first event becomes the Doll Room.</summary>
	public const int DollRoomAct = 1;

	/// <summary>0-based act index holding the forced Reflections row.</summary>
	public const int ReflectionsAct = 2;

	/// <summary>Copies added with every card that enters the deck (vanilla Hoarder).</summary>
	public const int ExtraCopies = 2;

	/// <summary>
	/// The map row just before the last row made only of rest sites, or null when there is no such row or it is the
	/// first row. <paramref name="restSiteRows"/> lists, row by row, whether each point of the row is a rest site.
	/// </summary>
	public static int? ReflectionsRow(IReadOnlyList<IReadOnlyList<bool>> restSiteRows)
	{
		for (int row = restSiteRows.Count - 1; row >= 0; row--)
		{
			var points = restSiteRows[row];
			if (points.Count > 0 && points.All(p => p))
				return row > 0 ? row - 1 : null;
		}
		return null;
	}
}
