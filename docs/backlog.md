# Backlog

Known gaps deferred from step 1's final review. Fixed when the author feels like it.

- With a card set mode and the vanilla "<Character> Cards" Custom Run modifier, an Ironclad merchant
  doesn't add the other character's cards: `CharacterCards.ModifyMerchantCardPool` gives up when an
  offered card's `Pool` isn't the player's pool, and StS1 cards live in `Sts1IroncladCardPool`.
  Card rewards still get them.
- Hybrid version switch on the merchant, choose-a-card screens (Discovery-style) and the Sealed Deck
  grid (spec §4). They show a random version for now. Each needs its own synced path: the merchant
  and choose-a-card sync only an index into their own list.
- The Watcher (StS1 character) as a playable character when a StS1 card set mode is ticked in a
  Custom Run (owner request, 2026-10-29).
- Act 4 in Custom Run: a Custom Run option that brings back the StS1 keys (Ruby, Emerald, Sapphire)
  and the act after the last boss that they unlock (owner request, 2026-10-29).
- StS1 Masterful Stab counts unblocked damage taken, not other HP loss: an event or relic HP loss in
  combat doesn't raise its cost (no Silent card loses HP by itself).
- StS1 A Thousand Cuts would miss one hit if the same card instance were played inside its own play
  (vanilla Afterimage has the same limit).
- StS1 Panache keeps a card in its counted set if that card's play is aborted between
  `BeforeCardPlayed` and `AfterCardPlayed`; harmless, the power is gone at combat end.
