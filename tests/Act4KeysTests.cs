using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Act4KeysTests
{
	[Fact]
	public void Take_FirstTakeChangesSecondDoesNot()
	{
		var k = new Act4Keys();
		Assert.True(k.Take(Act4Key.Ruby));
		Assert.False(k.Take(Act4Key.Ruby));
		Assert.True(k.Has(Act4Key.Ruby));
	}

	[Fact]
	public void HasAll_NeedsTheThreeKeys()
	{
		var k = new Act4Keys();
		k.Take(Act4Key.Ruby);
		k.Take(Act4Key.Emerald);
		Assert.False(k.HasAll);
		k.Take(Act4Key.Sapphire);
		Assert.True(k.HasAll);
	}

	[Fact]
	public void Encode_RoundTripsKeysAndNodes()
	{
		var k = new Act4Keys();
		k.Take(Act4Key.Sapphire);
		k.MarkEmerald(0, 3, 5);
		k.MarkEmerald(1, 2, 9);
		var back = Act4Keys.Decode(k.Encode());
		Assert.True(back.Has(Act4Key.Sapphire));
		Assert.False(back.Has(Act4Key.Ruby));
		Assert.Equal((3, 5), back.EmeraldNode(0));
		Assert.Equal((2, 9), back.EmeraldNode(1));
		Assert.Null(back.EmeraldNode(2));
	}

	[Fact]
	public void Take_IsOrderIndependent()
	{
		var a = new Act4Keys();
		a.Take(Act4Key.Ruby);
		a.Take(Act4Key.Sapphire);
		a.Take(Act4Key.Ruby);
		var b = new Act4Keys();
		b.Take(Act4Key.Sapphire);
		b.Take(Act4Key.Ruby);
		Assert.Equal(a.Encode(), b.Encode());
	}

	[Fact]
	public void Decode_NullIsAFreshState()
	{
		Assert.False(Act4Keys.Decode(null!).Has(Act4Key.Ruby));
	}

	[Fact]
	public void Decode_EmptyIsAFreshState()
	{
		Assert.False(Act4Keys.Decode(Array.Empty<int>()).Has(Act4Key.Ruby));
	}

	[Theory]
	[InlineData(new[] { 2, 0, 0 })]
	[InlineData(new[] { 1, 8, 0 })]
	[InlineData(new[] { 1, 0, 1, 0, 3 })]
	public void Decode_RejectsBadData(int[] data)
	{
		Assert.Throws<ArgumentException>(() => Act4Keys.Decode(data));
	}
}
