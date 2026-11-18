using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonConfigTests
{
	private const string FlagKey = "SomeFlag";

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
		Assert.Null(SpoonConfig.With("{bad", FlagKey, "True"));
		Assert.Null(SpoonConfig.With("[1,2]", FlagKey, "True"));
	}

	[Fact]
	public void With_AcceptsBlankContent()
	{
		Assert.True(SpoonConfig.ReadFlag(SpoonConfig.With("", FlagKey, "True"), FlagKey));
	}

	[Fact]
	public void Read_AcceptsJsonBooleans()
	{
		Assert.True(SpoonConfig.ReadFlag("{\"SomeFlag\": true}", FlagKey));
		Assert.False(SpoonConfig.ReadFlag("{\"SomeFlag\": false}", FlagKey));
	}

	[Fact]
	public void With_KeepsUnknownKeys()
	{
		string json = SpoonConfig.With("{\"Other\": \"x\", \"N\": 3}", FlagKey, "True")!;
		Assert.Equal("x", SpoonConfig.Read(json)["Other"]);
		Assert.Equal("3", SpoonConfig.Read(json)["N"]);
	}

	[Fact]
	public void With_KeepsALegacySyncOnlyFile()
	{
		string legacy = "{\n  \"SyncRegularProgress\": \"True\"\n}";
		string json = SpoonConfig.With(legacy, SpoonConfig.Act4UnlockKey(1), "True")!;
		Assert.Equal("True", SpoonConfig.Read(json)["SyncRegularProgress"]);
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
		string json = SpoonConfig.With("{\"List\": [1, 2], \"Obj\": {\"a\": true}, \"N\": 3, \"Nil\": null}", FlagKey, "True")!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var root = doc.RootElement;
		Assert.Equal(2, root.GetProperty("List").GetArrayLength());
		Assert.True(root.GetProperty("Obj").GetProperty("a").GetBoolean());
		Assert.Equal(3, root.GetProperty("N").GetInt32());
		Assert.Equal(System.Text.Json.JsonValueKind.Null, root.GetProperty("Nil").ValueKind);
		Assert.True(SpoonConfig.ReadFlag(json, FlagKey));
	}

	[Fact]
	public void With_RefusesDuplicateKeys() =>
		Assert.Null(SpoonConfig.With("{\"A\": 1, \"A\": 2}", FlagKey, "True"));
}
