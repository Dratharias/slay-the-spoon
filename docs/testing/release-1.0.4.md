# Version 1.0.4 — preparation and validation

Prepared locally; not uploaded to Steam Workshop.

## Automated validation
- Build the mod and stage with `make workshop`.
- Run `make test`: includes stale acceptance, cancelled/recreated requests, insufficient selected funds and unrelated requests.
- Check that mod manifest and runtime version both read 1.0.4.
- Workshop changelog: `workshop/changelog-1.0.4.txt` (French and English).
- All peers require 1.0.4: merchant actions now include a request revision.

## In-game validation — pending
The automated checks do not exercise the rendered game or a real multiplayer session.

- Open the loan panel from the rug and by attempting an unaffordable purchase.
- At 1280×720 and 1920×1080, check the loan and campfire sale windows: no preview overlap, readable sliders, footer visible while scrolling.
- Verify red actions on the left and green confirmation on the right, with mouse and controller.
- Check that zero gold disables Request/Accept and increasing the amount enables it.
- Send a gift: interest fields disappear; acceptance creates no debt.
- Send a zero-interest loan: borrower owes at least the principal.
- Select offers exceeding the requested sum: displayed gold and debt match the actual transfer.
- Change or withdraw an offer while acceptance is queued: no transfer on stale terms, request remains available for review.
- Dismiss a lender window, then change an offer on another peer: no forced reopening.
- Confirm Request sent, Offer sent/withdrawn, Gift received and Loan received messages after execution.
- Escape and Close only close the window, on your own request too (superseded 2026-11-12: "Withdraw" cancels your request); on another player's request they close without withdrawing an active offer.
- With several requests, switch borrowers and verify each amount, interest rate and action applies to the named player.
- Verify the Sapphire Key and its button remain together, clear of relics.
- Standard run with maximum unlocked ascension 4: no new automatic unlock. With maximum 5: Act 4 enabled even at selected ascension 0. Preserve previous permanent unlocks.

## Review fixes (branch `review-fixes`, 2026-11-12)

- [ ] Loans: Escape or Close on your own request closes the window; the request and its offers stay. "Withdraw" cancels it.
- [ ] Loans (3+ players): another lender changing an offer you did not tick does not reject your Accept.
- [ ] Loans: the window never stays locked on "Transaction pending…" (buttons come back after 10 s at most; Escape always closes).
- [ ] Loans: after the window is closed by something else, buying an item does not reopen it.
- [ ] Controller: focus never lands on a disabled button in the loan or sale windows.
- [ ] Campfire sale: the buyer declines → the seller sees "The buyer declined the offer."
- [ ] Campfire sale: the buyer spends gold before accepting → both see the "no longer has the gold" message, nothing moves.
- [ ] Campfire sale: closing either sale window by any other means still ends the sale for both players.
