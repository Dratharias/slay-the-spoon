# Step 5 — Merchant Loans in-game checklist

Build and automated tests pass. All checks below require the owner in game; none is claimed complete.
Enable Merchant Loans in a new co-op Custom Run. An existing run needs to already contain this
modifier. Load the new mod build on both clients before testing.

- [ ] Solo with the modifier: no loan UI or repayment effects.
- [ ] Standard and Daily without the modifier: vanilla merchant and gold gains.
- [ ] Two players: try an unaffordable card, relic, potion and removal. Ask popup shows the exact shortage.
- [ ] Cancel the popup: no request. Repeated clicks do not open duplicate popups.
- [ ] Ask: both players see the request, even if the lender has not opened the merchant inventory.
- [ ] Amount and interest reset/+1/+5/+10 work; amount stays within available gold and interest within 0–300%.
- [ ] Lend: offered gold is reserved. Replacing or withdrawing an offer adjusts/refunds it once.
- [ ] Give: no interest and no resulting debt.
- [ ] Three players: deselect an offer; Accept stays disabled until selected offers cover the shortage.
- [ ] Overfunded request: final offer is only partly taken, remainder and unselected offers are refunded.
- [ ] Debt preview matches accepted principal and interest (5 gold at 10% owes 6; 100 at 300% owes 400).
- [ ] Accept: gold arrives once, request closes on both clients, and retrying the purchase succeeds.
- [ ] Cancel a request or leave the merchant: every held offer is refunded once.
- [ ] Combat gold and gold rewards repay debts; oldest loan first, equal shares within that loan.
- [ ] A lender with their own debt keeps incoming repayment without a second repayment cascade.
- [ ] Kill an indebted borrower, finish combat and revive them: the full unpaid debt remains.
- [ ] The revived borrower's next gold gain repays that same debt; intentional death cannot erase it.
- [ ] Save/quit/resume, including co-op resume: remaining debt persists and repayment continues.
- [ ] Compare both clients' `[SlaySpoon] loans:` ledger logs after acceptance and repayment.
- [ ] Test concurrent merchant purchase/offer/withdrawal and room departure on both clients:
      total gold is conserved and both clients agree (purchase and loan queues are separate).
- [ ] Panel, scrolling, focus and popup are usable in English/French at the owner's window sizes.
- [ ] Merchant cards, relics, potion slots, removal and exit remain reachable with requests open.
