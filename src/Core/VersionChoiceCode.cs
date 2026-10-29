namespace SlaySpoon.Core;

/// <summary>
/// The game syncs a choose-a-card pick as one index into the options (-1 = skipped). A Hybrid pick also carries the
/// version: picking option i in its other version is sent as <c>count + i</c>.
/// </summary>
public static class VersionChoiceCode
{
	public static int Encode(int index, bool swapped, int count)
	{
		return index < 0 ? -1 : swapped ? count + index : index;
	}

	public static (int Index, bool Swapped) Decode(int code, int count)
	{
		if (code < -1 || code >= 2 * count)
		{
			throw new ArgumentOutOfRangeException(nameof(code), code, $"not a pick among {count} options");
		}
		return code < 0 ? (-1, false) : code >= count ? (code - count, true) : (code, false);
	}
}
