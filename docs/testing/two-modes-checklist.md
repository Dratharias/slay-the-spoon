# Folie à deux and Matriochka (1.1.0) — in-game checklist

Nothing here has been tested in game. Co-op first: every chest must resolve the same way on both screens.
Use `make mp` for two local instances (both write to the same `godot.log`; grep `[SlaySpoon] folie:` for the bids
and awards of each chest, keyed `combat_<floor>`, `treasure_<floor>`, `floor_<floor>`, `draft_<n>`, `sealed`,
`custom_<player>_<floor>_<n>`, `pick_<player>_<floor>_<n>`).

## Folie à deux — launch

- [ ] Multiplayer host screen shows a fourth card (Madness art); the layout of the four cards is clean.
- [ ] The card opens the Standard lobby; the run starts as Standard with Folie à deux (log `Modifiers:` line).
- [ ] Going back and picking a vanilla card starts a run without the flag.
- [ ] Custom Run: the Folie à deux checkbox appears with a short description; ticking it in solo changes nothing.
- [ ] Neow stays vanilla in the Folie Standard run.
- [ ] Daily is untouched.

## Folie à deux — chest

- [ ] After a combat: the act's chest opens, then one head per player with their cards (NCard visuals placed
      correctly; more than 4 cards shows a title list).
- [ ] Each player bids once; the other player's name appears under the head they picked.
- [ ] One bidder on a head: they take it. Two on a head: rock-paper-scissors rounds play, the same on both screens.
- [ ] A head nobody bid on is lost (the owner's reward screen has no card reward).
- [ ] The winner's reward screen shows the won cards; taking one puts it in the winner's deck, with its original
      character (color) kept. Skip still works where vanilla allows it.
- [ ] The Hybrid version swap button still works on a won reward (Spire Souvenir run).
- [ ] Treasure rooms with extra card rewards (e.g. relic effects) go through a chest too.
- [ ] Event or relic card rewards (`RewardsCmd.OfferCustom`): a one-head chest pops up on every screen, even over
      another player's event; the winner gets a reward screen of their own. Check the overlay does not get stuck.
- [ ] Event card picks (Brain Leech "Share Knowledge", Room Full of Cheese "Gorge") and relic picks (Massive Scroll,
      Hefty Tablet, Lead Paperweight, Sea Glass): the owner picks first, then a one-head chest; the winner's deck gets
      the cards; nobody bidding means the pick is lost. Combat picks (Attack Potion, Discovery) open no chest.
- [ ] Hybrid run: the version the owner chose is the one at stake.
- [ ] A dead player does not bid and does not block the chest.
- [ ] Save and reload in the middle of a chest: the run resumes without a desync (the chest may replay).
- [ ] Continue button and the 6 s auto-close both close the chest.

## Folie à deux — Draft and Sealed Deck (Custom Run)

- [ ] Draft: ten chests in a row; every player takes exactly one head per round (consolation shown), then picks a
      card from it. Both screens agree on every round.
- [ ] Sealed Deck: each player picks 10 of 30; one chest then hands out the decks; every player ends with 10 cards
      and the preview shows the received deck.
- [ ] A player who reaches the Neow option first waits without a broken screen until the others join.

## Matriochka (Custom Run)

- [ ] Hoarder applies (3 copies of each added card, no card removal at the merchant) without ticking Hoarder;
      ticking both does not give 5 copies.
- [ ] Combat rewards: the card reward cannot be skipped and the screen cannot be left before taking a card.
- [ ] Merchant, rest site, event and treasure rooms each give an unskippable card reward when the room ends.
- [ ] First merchant: Dolly's Mirror is given once, its duplicate pick works.
- [ ] Act 2: the first event is the Doll Room with only the Bing Bong option.
- [ ] Act 3: the row before the final rest sites is all Reflections; only Shatter; in co-op everyone shatters.
- [ ] Rerolls: the row's −/+ sets 0–10 (default 3); the reroll button shows the rerolls left, resets each act,
      survives save and reload. At 0 no reroll button.
- [ ] With Folie à deux also ticked: the floor rewards go through a chest (`floor_<floor>`), the winner must take a card.
