using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ProgressPathsTests
{
	[Fact]
	public void Vanilla_path_is_the_unmodded_profile()
	{
		Assert.Equal("profile1/saves/progress.save", ProgressPaths.Vanilla(1));
	}

	[Fact]
	public void Modded_path_is_under_the_modded_folder()
	{
		Assert.Equal("modded/profile2/saves/progress.save", ProgressPaths.Modded(2));
	}
}
