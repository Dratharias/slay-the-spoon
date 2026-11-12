# Campfire sales and native trade popups

Automated: payment bounds, zero-price gifts, partial/full credit, interest rounding and saved ledger repayments.
Build and automated tests do not validate native popup layout or multiplayer interactions. All checks below remain pending.

- [ ] At a co-op rest site, choose a card and ally. Adjust price from 0 to 999 and credit interest from 0 to 900% with mouse and controller.
- [ ] The buyer sees the actual card, cash payment and total debt before accepting. The native popup, buttons and sliders match the game at 1280×720 and 1920×1080.
- [ ] Cancel card selection, targeting or pricing; decline or withdraw an offer. No gold/card moves and the seller keeps their rest action.
- [ ] Accept a zero-price gift with zero gold. Only the seller's rest action is consumed.
- [ ] Pay in full: gold goes to the seller, with no debt or interest.
- [ ] Buy for 100 with 40 gold at 50%: seller receives 40 now, buyer owes 90. Future gold repays the seller.
- [ ] Save/reload and die/revive: debt remains. Repeat with Merchant Loans disabled, then enabled alongside Campfire Sales.
- [ ] Simultaneous or crossed offers do not mix cards or responses. Double-clicking acceptance transfers a card only once.
- [ ] If the buyer spends the quoted cash elsewhere before accepting, the sale cancels without increasing the quoted debt.
- [ ] Upgrades and enchantments survive the transfer. Curses and unremovable cards cannot be sold.
- [ ] Merchant requests open native popups. Lenders can adjust amount/interest, give gold, or withdraw; borrowers can include/exclude offers and see the resulting debt before accepting.
- [ ] For lenders, Cancel/Escape closes the loan popup and leaves submitted offers intact; for borrowers, both cancel their request. The merchant's loan button reopens the interface. Leaving the room refunds held merchant offers and closes trade popups.
