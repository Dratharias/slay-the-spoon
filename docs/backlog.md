# Backlog

Known gaps deferred from step 1's final review. Fixed when the author feels like it.

- Damage dealt by enemy effects with no enemy dealer (Constrict, Burn/Decay, Haunt, Galvanic) is not
  scaled by Monster Damage, although the description says "enemies deal N% of their normal damage".
- The − / + buttons on the Custom Run rows can't be reached with a gamepad.
- `SpoonPercentModifier.Percent` re-parses the class name on every access; cache it per type.
- The PCK also carries `res://project.binary` and `global_script_class_cache.cfg` (Godot's
  `--export-pack` adds them).
- Top bar: HP and Damage entries are told apart only by their hover tip.
- With a card set mode and the vanilla "<Character> Cards" Custom Run modifier, an Ironclad merchant
  doesn't add the other character's cards: `CharacterCards.ModifyMerchantCardPool` gives up when an
  offered card's `Pool` isn't the player's pool, and StS1 cards live in `Sts1IroncladCardPool`.
  Card rewards still get them.
- Hybrid version switch on the merchant, choose-a-card screens (Discovery-style) and the Sealed Deck
  grid (spec §4). They show a random version for now. Each needs its own synced path: the merchant
  and choose-a-card sync only an index into their own list.
