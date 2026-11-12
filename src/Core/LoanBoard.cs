namespace SlaySpoon.Core;

/// <summary>One player's answer to a request: a loan at <see cref="RatePercent"/>, or a gift.</summary>
public sealed record LoanOffer(int Lender, int Amount, int RatePercent, bool Gift);

/// <summary>Gold that moves from a lender to the borrower when the borrower accepts.</summary>
public sealed record LoanTransfer(int Lender, int Amount, int RatePercent, bool Gift);

/// <summary>
/// Open requests for gold at the merchant and the offers made to them. One request per borrower, one offer per lender
/// and request. Nothing is held back at offer time: acceptance clamps each offer to the lender's gold then.
/// </summary>
public sealed class LoanBoard
{
	public const int MaxRate = 900;

	private sealed class OpenRequest
	{
		public int Missing;
		public int Revision;
		public readonly List<LoanOffer> Offers = new();
	}

	private int _revision;

	private readonly List<int> _borrowers = new();
	private readonly Dictionary<int, OpenRequest> _requests = new();

	public event Action? Changed;

	public IReadOnlyList<int> Borrowers => _borrowers;

	public bool HasRequest(int borrower) => _requests.ContainsKey(borrower);

	public int Revision(int borrower) => _requests.TryGetValue(borrower, out var request) ? request.Revision : 0;

	/// <summary>Validate the exact board shown at confirmation, before moving any held gold.</summary>
	public bool CanAccept(int borrower, int revision, int lenders) =>
		_requests.TryGetValue(borrower, out var request) && request.Revision == revision
		&& request.Offers.Where(o => (lenders & (1 << o.Lender)) != 0).Sum(o => (long)o.Amount) >= request.Missing;

	public int Missing(int borrower) => _requests.TryGetValue(borrower, out OpenRequest? request) ? request.Missing : 0;

	public IReadOnlyList<LoanOffer> OffersFor(int borrower)
	{
		return _requests.TryGetValue(borrower, out OpenRequest? request) ? request.Offers : Array.Empty<LoanOffer>();
	}

	/// <summary>Opens (or reopens, without its offers) the borrower's request; nothing missing cancels it.</summary>
	public void Request(int borrower, int missing)
	{
		Close(borrower);
		if (missing > 0)
		{
			_borrowers.Add(borrower);
			_requests[borrower] = new OpenRequest { Missing = missing, Revision = ++_revision };
		}
		Changed?.Invoke();
	}

	public void Offer(int borrower, LoanOffer offer)
	{
		if (offer.Lender == borrower || !_requests.TryGetValue(borrower, out OpenRequest? request))
		{
			return;
		}
		request.Revision = ++_revision;
		offer = offer with { RatePercent = offer.Gift ? 0 : Math.Clamp(offer.RatePercent, 0, MaxRate) };
		int index = request.Offers.FindIndex(o => o.Lender == offer.Lender);
		if (offer.Amount <= 0)
		{
			if (index >= 0)
			{
				request.Offers.RemoveAt(index);
			}
		}
		else if (index >= 0)
		{
			request.Offers[index] = offer;
		}
		else
		{
			request.Offers.Add(offer);
		}
		Changed?.Invoke();
	}

	/// <summary>Takes the picked offers, in offer order, up to the missing amount, and closes the request.</summary>
	public IReadOnlyList<LoanTransfer> Accept(int borrower, IReadOnlyCollection<int> lenders, Func<int, int> goldOf)
	{
		if (!_requests.TryGetValue(borrower, out OpenRequest? request))
		{
			return Array.Empty<LoanTransfer>();
		}
		var transfers = new List<LoanTransfer>();
		int need = request.Missing;
		foreach (LoanOffer offer in request.Offers.Where(o => lenders.Contains(o.Lender)))
		{
			int amount = Math.Min(Math.Min(offer.Amount, need), goldOf(offer.Lender));
			if (amount <= 0)
			{
				continue;
			}
			transfers.Add(new LoanTransfer(offer.Lender, amount, offer.RatePercent, offer.Gift));
			need -= amount;
		}
		Close(borrower);
		Changed?.Invoke();
		return transfers;
	}

	public void Cancel(int borrower)
	{
		Close(borrower);
		Changed?.Invoke();
	}

	public void Clear()
	{
		_borrowers.Clear();
		_requests.Clear();
		Changed?.Invoke();
	}

	private void Close(int borrower)
	{
		_borrowers.Remove(borrower);
		_requests.Remove(borrower);
	}
}
