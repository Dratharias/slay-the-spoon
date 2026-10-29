using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Core;
using SlaySpoon.Loans;

namespace SlaySpoon.Config;

/// <summary>
/// Co-op players can lend or give gold to a player short of it at the merchant (<see cref="MerchantLoans"/>). The
/// modifier owns the debts; repayment lives in <see cref="LoanRepaymentPatch"/>.
/// </summary>
public sealed class SpoonMerchantLoans : ModifierModel
{
	private LoanLedger _ledger = new();

	public LoanLedger Ledger => _ledger;

	/// <summary>The encoded ledger. Saved properties must reuse a vanilla name; Fur Coat's is an int array too.</summary>
	[SavedProperty]
	public int[] FurCoatCoordCols
	{
		get => _ledger.Encode();
		set
		{
			try
			{
				_ledger = LoanLedger.Decode(value);
			}
			catch (ArgumentException e)
			{
				SpoonLog.Error($"loans: saved ledger dropped: {e.Message}");
				_ledger = new LoanLedger();
			}
		}
	}

	// Death and co-op resurrection never change the ledger. Debts belong to the run.
}
