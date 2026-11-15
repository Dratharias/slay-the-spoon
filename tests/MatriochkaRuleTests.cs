using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MatriochkaRuleTests
{
	private static IReadOnlyList<IReadOnlyList<bool>> Rows(params bool[][] rows) => rows;

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
}
