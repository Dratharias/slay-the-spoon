using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MatriochkaRuleTests
{
	private static IReadOnlyList<IReadOnlyList<bool>> Rows(params bool[][] rows) => rows;

	[Theory]
	[InlineData(true, false, false, false, true)]
	[InlineData(false, true, false, false, true)]
	[InlineData(false, true, true, false, false)]
	[InlineData(false, true, false, true, false)]
	[InlineData(false, false, false, false, false)]
	public void OffersFloorCard_SkipsAncientsAndVictoryRoom(bool shop, bool ev, bool ancient, bool victory, bool expected) =>
		Assert.Equal(expected, MatriochkaRule.OffersFloorCard(shop, ev, ancient, victory));

	[Fact]
	public void ReflectionsRow_IsRowBeforeFinalRestRow() =>
		Assert.Equal(3, MatriochkaRule.ReflectionsRow(Rows([false], [false], [true, true], [false], [true, true, true], [false])));

	[Fact]
	public void ReflectionsRow_IgnoresMixedRows() =>
		Assert.Equal(1, MatriochkaRule.ReflectionsRow(Rows([false], [false], [true], [true, false])));

	[Fact]
	public void ReflectionsRow_NullWithoutRestRow() =>
		Assert.Null(MatriochkaRule.ReflectionsRow(Rows([false], [true, false])));

	[Fact]
	public void ReflectionsRow_NullWhenRestRowIsFirst() =>
		Assert.Null(MatriochkaRule.ReflectionsRow(Rows([true, true], [false])));

	[Fact]
	public void Rerolls_ThreePerPlayerPerAct()
	{
		var r = new MatriochkaRerolls();
		for (int i = 0; i < 3; i++) Assert.True(r.Use(1, 0, 3, 2));
		Assert.False(r.Use(1, 0, 3, 2));
		Assert.Equal(0, r.Remaining(1, 0, 3));
		Assert.Equal(3, r.Remaining(0, 0, 3));
		Assert.Equal(3, r.Remaining(1, 1, 3));
		Assert.True(r.Use(1, 1, 3, 2));
		Assert.Equal(2, r.Remaining(1, 1, 3));
	}

	[Fact]
	public void Rerolls_SurviveEncodeDecode()
	{
		var r = new MatriochkaRerolls();
		r.Use(0, 2, 3, 3);
		r.Use(2, 2, 3, 3);
		r.Use(2, 2, 3, 3);
		var back = MatriochkaRerolls.Decode(r.Encode());
		Assert.Equal(2, back.Remaining(0, 2, 3));
		Assert.Equal(3, back.Remaining(1, 2, 3));
		Assert.Equal(1, back.Remaining(2, 2, 3));
		Assert.Equal(3, MatriochkaRerolls.Decode(Array.Empty<int>()).Remaining(0, 0, 3));
	}

	[Theory]
	[InlineData(3, 1, 4)]
	[InlineData(0, -1, 0)]
	[InlineData(10, 1, 10)]
	public void StepRerolls_ClampsToGrid(int value, int direction, int expected) =>
		Assert.Equal(expected, MatriochkaRule.StepRerolls(value, direction));
}
