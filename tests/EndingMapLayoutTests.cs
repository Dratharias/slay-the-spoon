using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingMapLayoutTests
{
	[Fact]
	public void EndingMapIsRestShopEliteBossInOneColumn()
	{
		Assert.Equal(
			[(0, EndingNode.RestSite), (1, EndingNode.Shop), (2, EndingNode.Elite), (3, EndingNode.Boss)],
			EndingMapLayout.Nodes);
		Assert.Equal(3, EndingMapLayout.Column);
		Assert.Equal(4, EndingMapLayout.Rows);
	}

	/// <summary>Vanilla maps keep the boss one row above the grid, and the last grid row leads to it.</summary>
	[Fact]
	public void GridHoldsEveryRowBelowTheBoss()
	{
		Assert.Equal(3, EndingMapLayout.GridHeight);
		Assert.Equal((EndingMapLayout.GridHeight, EndingNode.Boss), EndingMapLayout.Nodes[^1]);
		Assert.Equal(Enumerable.Range(0, EndingMapLayout.Rows), EndingMapLayout.Nodes.Select(n => n.Row));
	}

	/// <summary>The act has no ancient, events or normal encounters: its first node is a rest site, never an ancient.</summary>
	[Fact]
	public void StartIsTheRestSite() => Assert.Equal((0, EndingNode.RestSite), EndingMapLayout.Nodes[0]);
}
