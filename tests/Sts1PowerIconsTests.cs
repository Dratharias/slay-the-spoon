using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>Every power that shows its own StS1 icon ships both icon files in the PCK.</summary>
public class Sts1PowerIconsTests
{
	private static string Repo()
	{
		string? dir = AppContext.BaseDirectory;
		while (dir != null && !File.Exists(Path.Combine(dir, "Makefile")))
		{
			dir = Path.GetDirectoryName(dir);
		}
		return dir ?? throw new InvalidOperationException("repo root not found");
	}

	[Fact]
	public void BlockReturn_HasItsOwnIcon()
	{
		Assert.True(Sts1PowerIcons.Has(ModelIds.FromClassName("Sts1BlockReturnPower")));
	}

	[Fact]
	public void EveryIcon_Exists()
	{
		foreach (string id in Sts1PowerIcons.Ids)
		{
			foreach (string path in new[] { Sts1PowerIcons.Packed(id), Sts1PowerIcons.Big(id) })
			{
				string file = Path.Combine(Repo(), "godot", path["res://".Length..]);
				Assert.True(File.Exists(file), "missing " + file);
			}
		}
	}
}
