using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Core;
using SlaySpoon.Campfire;

namespace SlaySpoon.Config;

/// <summary>
/// Campfire Sales (Custom Run, co-op only): every rest site offers <see cref="SpoonGiftRestSiteOption"/>. Solo runs see
/// no option. Stores sale debts when Merchant Loans is disabled.
/// </summary>
public sealed class SpoonCampfireGift : ModifierModel
{
	private LoanLedger _ledger = new();
	public LoanLedger Ledger => _ledger;

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_ledger = new LoanLedger();
	}

	// Reuse the same vanilla saved-property name as the merchant's ledger.
	[SavedProperty]
	public int[] FurCoatCoordCols
	{
		get => _ledger.Encode();
		set
		{
			try { _ledger = LoanLedger.Decode(value); }
			catch (ArgumentException e)
			{
				SpoonLog.Error($"campfire: saved ledger dropped: {e.Message}");
				_ledger = new LoanLedger();
			}
		}
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player.RunState.Players.Count < 2) return false;
		options.Add(new SpoonGiftRestSiteOption(player));
		return true;
	}
}
