using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>The reviewed per-curse rows of <c>data/curses/curses.json</c>, embedded in the DLL and read once.</summary>
public static class CurseTable
{
	private static readonly Lazy<IReadOnlyDictionary<string, CurseRow>> Table = new(Load);

	public static CurseRow? For(string id) => Table.Value.TryGetValue(id, out var row) ? row : null;

	private static IReadOnlyDictionary<string, CurseRow> Load()
	{
		try
		{
			using var stream = typeof(CurseTable).Assembly.GetManifestResourceStream("SlaySpoon.curses.curses.json")
				?? throw new InvalidOperationException("embedded curses.json missing");
			using var reader = new StreamReader(stream);
			return CurseRule.ParseTable(reader.ReadToEnd());
		}
		catch (Exception e)
		{
			SpoonLog.Error($"cursed deck: curse table unreadable, played curses only feed the Grimoire: {e.Message}");
			return new Dictionary<string, CurseRow>();
		}
	}
}
