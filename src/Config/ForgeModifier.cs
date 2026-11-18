using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Forge;

namespace SlaySpoon.Config;

/// <summary>
/// The Forge (Custom Run, docs/superpowers/specs/2026-11-18-forge-design.md): the Fuse rest option takes cards past +1
/// (<see cref="ForgeLevels"/>), and the merchant sells +0 copies of deck cards (<see cref="ForgeDuplicate"/>).
/// </summary>
public sealed class SpoonForge : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonForge);

	public static SpoonForge? Of(IRunState? run) => run?.Modifiers.OfType<SpoonForge>().FirstOrDefault();

	/// <summary>Duplicate purchases per player slot (index in <see cref="IRunState.Players"/>), over the whole run.</summary>
	private int[] _purchases = [];

	/// <summary>Players (net id) who used the Duplicate service at the current merchant. Not saved: one per visit.</summary>
	private HashSet<ulong> _usedHere = new();

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_purchases = [];
		_usedHere = new HashSet<ulong>();
	}

	[SavedProperty]
	public int[] SpoonForgeDuplicates
	{
		get => _purchases.ToArray();
		set => _purchases = value?.Select(v => Math.Max(v, 0)).ToArray() ?? [];
	}

	public int Purchases(Player player)
	{
		int slot = Slot(player);
		return slot >= 0 && slot < _purchases.Length ? _purchases[slot] : 0;
	}

	public bool UsedHere(Player player) => _usedHere.Contains(player.NetId);

	public void Purchased(Player player)
	{
		int slot = Slot(player);
		if (slot < 0) return;
		if (_purchases.Length <= slot) Array.Resize(ref _purchases, slot + 1);
		_purchases[slot]++;
		_usedHere.Add(player.NetId);
	}

	private static int Slot(Player player)
	{
		var players = player.RunState.Players;
		for (int i = 0; i < players.Count; i++)
			if (players[i] == player) return i;
		return -1;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is MerchantRoom) _usedHere.Clear();
		return Task.CompletedTask;
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		options.Add(new ForgeFuseRestSiteOption(player));
		return true;
	}
}
