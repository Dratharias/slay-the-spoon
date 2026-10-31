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
		Assert.Equal(choice, SpoonConfig.ReadSyncChoice(SpoonConfig.WriteSyncChoice(null, choice)));
	}

	[Fact]
	public void WriteSyncChoice_KeepsTheUnlock()
	{
		string json = SpoonConfig.With(null, SpoonConfig.Act4UnlockKey(1), "True")!;
		json = SpoonConfig.WriteSyncChoice(json, false)!;
		Assert.True(SpoonConfig.ReadFlag(json, SpoonConfig.Act4UnlockKey(1)));
		Assert.False(SpoonConfig.ReadSyncChoice(json));
	}

	[Fact]
	public void Unlock_IsPerProfile()
	{
		string json = SpoonConfig.With(null, SpoonConfig.Act4UnlockKey(1), "True")!;
		Assert.False(SpoonConfig.ReadFlag(json, SpoonConfig.Act4UnlockKey(2)));
	}

	[Fact]
	public void Read_BadJsonIsEmpty()
	{
		Assert.Empty(SpoonConfig.Read("{nope"));
	}

	[Fact]
	public void With_RefusesToOverwriteUnreadableContent()
	{
		Assert.Null(SpoonConfig.With("{bad", SpoonConfig.PreviewKey, "True"));
		Assert.Null(SpoonConfig.WriteSyncChoice("{bad", true));
		Assert.Null(SpoonConfig.With("[1,2]", SpoonConfig.PreviewKey, "True"));
	}

	[Fact]
	public void With_AcceptsBlankContent()
	{
		Assert.True(SpoonConfig.ReadFlag(SpoonConfig.With("", SpoonConfig.PreviewKey, "True"), SpoonConfig.PreviewKey));
	}

	[Fact]
	public void Read_AcceptsJsonBooleans()
	{
		Assert.True(SpoonConfig.ReadFlag("{\"Act4Preview\": true}", SpoonConfig.PreviewKey));
		Assert.False(SpoonConfig.ReadFlag("{\"Act4Preview\": false}", SpoonConfig.PreviewKey));
	}

	[Fact]
	public void With_KeepsUnknownKeys()
	{
		string json = SpoonConfig.With("{\"Other\": \"x\", \"N\": 3}", SpoonConfig.PreviewKey, "True")!;
		Assert.Equal("x", SpoonConfig.Read(json)["Other"]);
		Assert.Equal("3", SpoonConfig.Read(json)["N"]);
	}

	[Fact]
	public void With_KeepsALegacySyncOnlyFile()
	{
		string legacy = "{\n  \"SyncRegularProgress\": \"True\"\n}";
		string json = SpoonConfig.With(legacy, SpoonConfig.Act4UnlockKey(1), "True")!;
		Assert.True(SpoonConfig.ReadSyncChoice(json));
		Assert.True(SpoonConfig.ReadFlag(json, SpoonConfig.Act4UnlockKey(1)));
	}

	[Fact]
	public void ReadFlag_FalseForFalseAndMissing()
	{
		Assert.False(SpoonConfig.ReadFlag("{\"K\": \"False\"}", "K"));
		Assert.False(SpoonConfig.ReadFlag("{}", "K"));
	}

	[Fact]
	public void With_KeepsNonScalarValues()
	{
		string json = SpoonConfig.With("{\"List\": [1, 2], \"Obj\": {\"a\": true}, \"N\": 3, \"Nil\": null}", SpoonConfig.PreviewKey, "True")!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var root = doc.RootElement;
		Assert.Equal(2, root.GetProperty("List").GetArrayLength());
		Assert.True(root.GetProperty("Obj").GetProperty("a").GetBoolean());
		Assert.Equal(3, root.GetProperty("N").GetInt32());
		Assert.Equal(System.Text.Json.JsonValueKind.Null, root.GetProperty("Nil").ValueKind);
		Assert.True(SpoonConfig.ReadFlag(json, SpoonConfig.PreviewKey));
	}
}
