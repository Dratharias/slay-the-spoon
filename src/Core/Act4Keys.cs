namespace SlaySpoon.Core;

/// <summary>The three Act 4 keys, as bit flags.</summary>
public enum Act4Key
{
	Ruby = 1,
	Emerald = 2,
	Sapphire = 4,
}

/// <summary>
/// The team's Act 4 keys (one set for the whole team) and the map node of each act's marked Emerald elite.
/// Saved as <c>[1 (version), flags, n, then n × (actIndex, col, row)]</c>; an empty array is a fresh state.
/// </summary>
public sealed class Act4Keys
{
	private const int Version = 1;
	private const int AllFlags = (int)(Act4Key.Ruby | Act4Key.Emerald | Act4Key.Sapphire);
	private const int IntsPerNode = 3;

	private readonly SortedDictionary<int, (int Col, int Row)> _emerald = new();
	private int _flags;

	/// <summary>Raised when the keys change (by <see cref="Take"/>, or by <see cref="NotifyChanged"/> after a load).</summary>
	public static event Action? Changed;

	public bool HasAll => _flags == AllFlags;

	public bool Has(Act4Key key)
	{
		return (_flags & (int)key) != 0;
	}

	/// <summary>Gives the key to the team; true only when it was missing.</summary>
	public bool Take(Act4Key key)
	{
		if (Has(key))
		{
			return false;
		}
		_flags |= (int)key;
		Changed?.Invoke();
		return true;
	}

	/// <summary>Tells listeners the state changed outside <see cref="Take"/>, for example after a load.</summary>
	public static void NotifyChanged()
	{
		Changed?.Invoke();
	}

	public (int Col, int Row)? EmeraldNode(int actIndex)
	{
		return _emerald.TryGetValue(actIndex, out var node) ? node : null;
	}

	public void MarkEmerald(int actIndex, int col, int row)
	{
		_emerald[actIndex] = (col, row);
	}

	public void ClearEmerald(int actIndex)
	{
		_emerald.Remove(actIndex);
	}

	public int[] Encode()
	{
		var data = new List<int> { Version, _flags, _emerald.Count };
		foreach (var (act, (col, row)) in _emerald)
		{
			data.Add(act);
			data.Add(col);
			data.Add(row);
		}
		return data.ToArray();
	}

	/// <summary>Rebuilds a state from <see cref="Encode"/> output; throws <see cref="ArgumentException"/> on an unknown version, bad flags or a duplicate act; a corrupt node count keeps the flags and stores no node.</summary>
	public static Act4Keys Decode(int[]? data)
	{
		var keys = new Act4Keys();
		if (data == null || data.Length == 0)
		{
			return keys;
		}
		if (data.Length < 3 || data[0] != Version)
		{
			throw new ArgumentException("Unknown Act 4 keys data.", nameof(data));
		}
		int flags = data[1];
		int count = data[2];
		if (flags < 0 || flags > AllFlags)
		{
			throw new ArgumentException("Corrupt Act 4 keys data.", nameof(data));
		}
		keys._flags = flags;
		// Bound the count by what the array can hold before multiplying (no int overflow); a corrupt count stores no node.
		if (count < 0 || count > (data.Length - 3) / IntsPerNode || data.Length != 3 + count * IntsPerNode)
		{
			return keys;
		}
		for (int i = 0; i < count; i++)
		{
			int at = 3 + i * IntsPerNode;
			if (!keys._emerald.TryAdd(data[at], (data[at + 1], data[at + 2])))
			{
				throw new ArgumentException("Duplicate act in Act 4 keys data.", nameof(data));
			}
		}
		return keys;
	}
}
