using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class CardSetComposerTests
{
	private static readonly string[] Vanilla = { "AGGRESSION", "ANGER", "BASH", "RUPTURE", "RAMPAGE" };
	private static readonly HashSet<string> Matched = new() { "ANGER", "BASH", "RUPTURE", "RAMPAGE" };
	private static readonly Sts1Entry[] Sts1 =
	{
		new("STS1_CLEAVE", null, Sts1Relation.Unique),
		new("STS1_RUPTURE", "RUPTURE", Sts1Relation.Duplicate),
		new("STS1_RAMPAGE", "RAMPAGE", Sts1Relation.SameKeepSts1),
	};

	[Fact]
	public void Compose_Sts2Only_ReturnsVanillaUnchanged()
	{
		Assert.Equal(Vanilla, CardSetComposer.Compose(CardSetMode.Sts2Only, Vanilla, Matched, Sts1));
	}

	[Fact]
	public void Compose_Sts1Only_KeepsOnlyMatchedVanillaPlusSts1()
	{
		string[] vanilla = { "AGGRESSION", "ANGER", "BASH" };
		HashSet<string> matched = new() { "ANGER", "BASH" };
		Sts1Entry[] sts1 = { new("STS1_CLEAVE", null, Sts1Relation.Unique) };
		Assert.Equal(new[] { "ANGER", "BASH", "STS1_CLEAVE" }, CardSetComposer.Compose(CardSetMode.Sts1Only, vanilla, matched, sts1));
	}

	[Fact]
	public void Compose_Sts1Only_ClaimedVanillaDropped()
	{
		Assert.Equal(new[] { "ANGER", "BASH", "STS1_CLEAVE", "STS1_RUPTURE", "STS1_RAMPAGE" },
			CardSetComposer.Compose(CardSetMode.Sts1Only, Vanilla, Matched, Sts1));
	}

	[Fact]
	public void Compose_Sts1Only_NoSts1Cards_KeepsVanilla()
	{
		Assert.Equal(Vanilla, CardSetComposer.Compose(CardSetMode.Sts1Only, Vanilla, new HashSet<string>(), Array.Empty<Sts1Entry>()));
	}

	[Fact]
	public void Compose_Hybrid_DuplicateIsOneSlot()
	{
		// RUPTURE stands for both versions; the swap picks STS1_RUPTURE.
		Assert.Equal(new[] { "AGGRESSION", "ANGER", "BASH", "RUPTURE", "STS1_CLEAVE", "STS1_RAMPAGE" },
			CardSetComposer.Compose(CardSetMode.Hybrid, Vanilla, Matched, Sts1));
	}

	[Fact]
	public void Compose_IsDeterministic()
	{
		HashSet<string> forward = new();
		foreach (string id in Matched.OrderBy(x => x))
		{
			forward.Add(id);
		}
		HashSet<string> backward = new();
		foreach (string id in Matched.OrderByDescending(x => x))
		{
			backward.Add(id);
		}
		foreach (CardSetMode mode in Enum.GetValues<CardSetMode>())
		{
			Assert.Equal(CardSetComposer.Compose(mode, Vanilla, forward, Sts1), CardSetComposer.Compose(mode, Vanilla, backward, Sts1));
			Assert.Equal(CardSetComposer.Compose(mode, Vanilla, Matched, Sts1), CardSetComposer.Compose(mode, Vanilla, Matched, Sts1));
		}
	}
}
