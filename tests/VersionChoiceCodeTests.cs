using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class VersionChoiceCodeTests
{
	[Theory]
	[InlineData(0, false, 3, 0)]
	[InlineData(2, false, 3, 2)]
	[InlineData(0, true, 3, 3)]
	[InlineData(2, true, 3, 5)]
	[InlineData(1, true, 2, 3)]
	public void Encode_keeps_the_index_and_shifts_swapped_picks_past_the_options(int index, bool swapped, int count,
		int expected)
	{
		Assert.Equal(expected, VersionChoiceCode.Encode(index, swapped, count));
	}

	[Fact]
	public void Skip_stays_minus_one()
	{
		Assert.Equal(-1, VersionChoiceCode.Encode(-1, false, 3));
		Assert.Equal(-1, VersionChoiceCode.Encode(-1, true, 3));
		Assert.Equal((-1, false), VersionChoiceCode.Decode(-1, 3));
	}

	[Theory]
	[InlineData(0, 3)]
	[InlineData(2, 3)]
	[InlineData(1, 1)]
	public void Decode_reverses_encode(int index, int count)
	{
		Assert.Equal((index % count, false), VersionChoiceCode.Decode(VersionChoiceCode.Encode(index % count, false, count), count));
		Assert.Equal((index % count, true), VersionChoiceCode.Decode(VersionChoiceCode.Encode(index % count, true, count), count));
	}

	[Theory]
	[InlineData(6, 3)]
	[InlineData(-2, 3)]
	public void Decode_rejects_codes_out_of_range(int code, int count)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => VersionChoiceCode.Decode(code, count));
	}
}
