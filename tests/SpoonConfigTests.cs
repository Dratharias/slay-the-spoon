using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonConfigTests
{
	[Fact]
	public void Missing_file_means_not_asked_yet()
	{
		Assert.Null(SpoonConfig.ReadSyncChoice(null));
	}

	[Theory]
	[InlineData("{\"SyncRegularProgress\": \"True\"}", true)]
	[InlineData("{\"SyncRegularProgress\": \"False\"}", false)]
	public void Reads_saved_choice(string json, bool expected)
	{
		Assert.Equal(expected, SpoonConfig.ReadSyncChoice(json));
	}

	[Theory]
	[InlineData("{}")]
	[InlineData("not json")]
	[InlineData("{\"SyncRegularProgress\": \"maybe\"}")]
	public void Unreadable_choice_means_ask_again(string json)
	{
		Assert.Null(SpoonConfig.ReadSyncChoice(json));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Written_choice_reads_back(bool choice)
	{
		Assert.Equal(choice, SpoonConfig.ReadSyncChoice(SpoonConfig.WriteSyncChoice(choice)));
	}
}
