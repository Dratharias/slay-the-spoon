namespace SlaySpoon.Core;

/// <summary>
/// Run-scoped record of a chest's pickups: the relic ids a chest awarded (consumed as each one is obtained) and one
/// entry per obtained relic, valid only for the floor it was recorded on.
/// </summary>
public sealed class PickupLedger<TKey, TEntry> where TKey : notnull
{
	private readonly Dictionary<TKey, (int Floor, TEntry Entry)> _entries = new();
	private readonly List<string> _expected = [];

	public void Record(TKey key, int floor, TEntry entry) => _entries[key] = (floor, entry);

	public bool TryFind(TKey key, int floor, out TEntry entry)
	{
		if (_entries.TryGetValue(key, out var found) && found.Floor == floor)
		{
			entry = found.Entry;
			return true;
		}
		entry = default!;
		return false;
	}

	/// <summary>Adds the relic ids a chest is about to hand out.</summary>
	public void Expect(IEnumerable<string> ids) => _expected.AddRange(ids);

	/// <summary>True (once per awarded copy) when this relic id is one the chest handed out.</summary>
	public bool ConsumeExpected(string id) => _expected.Remove(id);

	public void Clear()
	{
		_entries.Clear();
		_expected.Clear();
	}
}
