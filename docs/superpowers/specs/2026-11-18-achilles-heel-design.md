# Achilles' Heel — design

Owner idea (2026-11-18): a Custom Run mode named after the Achilles' heel. The player is untouchable except for one
weak spot: any attack that gets past their Block kills them. The run steers the player toward a turtle (Block) deck
that scales well into the late game.

## Owner decisions (2026-11-18)

- **HP and max HP stay vanilla.** The owner rejected a 1 max HP start because it breaks the Spoon mode, which uses
  max HP as its currency. The heel follows the mechanic of the vanilla card The Gambit (`TheGambit` /
  `TheGambitPower`), which reads: "If you take unblocked attack damage this combat, die." With the heel, this applies
  to every combat.
- Every player gets a new relic at the start of the run: **Achilles' Heel** (`SpoonAchillesHeel`; French
  "Talon d'Achille"). Its art is a Pixabay photo of a Greek war sandal (sources go in `docs/backlog.md`, like the
  other Pixabay art). The relic carries the mode's rules:
  1. **The heel**: in every combat, unblocked attack damage kills the holder.
  2. **Dexterity per act**: +1 Dexterity for each act past the first. Act 1 gives 0, act 2 gives 1, act 3 gives 2,
     act 4 (The Ending or the Spoon act) gives 3.
  3. **Block hits**: the holder's attacks deal +1 damage for every 5 Block the holder has.
  4. **Block-leaning rewards**: in card offers, cards that gain Block are drawn twice as often.
- Every player also gets the 3 common Block relics at the start: **Anchor**, **Oddly Smooth Stone** and
  **Gorget**. These relics and the heel relic are removed from every grab bag, like the Midas and Spoon start
  relics. The uncommon and rare Block relics stay in the normal pools.
- The mode is `SpoonAchillesHeel`. It is Custom Run only and off by default. Standard and Daily are untouched.

## Approaches

- **Chosen: a relic that carries the rules, plus a modifier that hands it out.** This follows the existing modes
  (Deck of 10, Blood Fang, Unholy Grimoire). The rules show on a relic the player can inspect. Other sources could
  later grant the relic without the mode.
- **Rejected: 1 max HP.** It breaks the Spoon mode (owner).
- **Rejected: rules on the modifier only.** The player would have nothing to inspect, and nothing else could grant
  the heel later.

## Builder calls

- **The heel is the mod's own power**, `SpoonHeelPower`, applied at the start of every combat. It is not vanilla
  `TheGambitPower`, which is a Debuff: Artifact would block it, and it removes itself once it fires. The heel can't be
  blocked or removed. It has the same trigger: the holder receives powered attack damage
  (`props.IsPoweredAttack()`) with `UnblockedDamage > 0`. The holder then dies (`CreatureCmd.Kill`).
  - Poison, Thorns-style damage, HP loss and unpowered damage don't trigger it.
  - It has EN and FR text and an icon borrowed from a vanilla power.
- **Dexterity per act** is applied as Dexterity at combat start. The amount is the act index (0-based), which counts
  appended acts. The numbers live in a pure rule, `src/Core/AchillesRule.cs`.
- **Block hits** is an additive damage modifier for powered attacks the holder deals: +floor(current Block / 5).
  Block is read when the hit lands. The divisor 5 is a constant in `AchillesRule`.
- **Block-leaning rewards** gives weight ×2 to cards with `GainsBlock` in each same-rarity draw, for players who hold
  the relic. It reuses `OfferWeights` and `OfferWeightPatch`, so equal weights still mean a vanilla draw. The weights
  multiply with the Souvenir lean and the combo factor. The factor 2 is a constant in `AchillesRule`.
- **Co-op**: a player the heel kills is dead like any dead player in vanilla co-op. They come back after the combat,
  and the team loses only when every player is dead.
- **Grab bag removal** uses the existing start-relic pattern (`SpoonDeckOfTen`, the Midas and Spoon start relics):
  the relic is given in `AfterRunCreated`, then removed from `player.RelicGrabBag` and `SharedRelicGrabBag`.

- **Heel trigger edges** follow vanilla The Gambit exactly:
  - Damage that Intangible or Buffer reduces still counts if `UnblockedDamage > 0`.
  - Revive effects react to `CreatureCmd.Kill` as they do for The Gambit.
  - The heel stays active after a revive.
- **Act 1 safety**: none beyond the three common Block relics. Balance is the owner's to tune after play.
- **Block card classification**: a card gains Block if `CardModel.GainsBlock` is true, or if its vars hold a
  `BlockVar`. The second check covers StS1 ports that don't override `GainsBlock`.
- **Block hits** covers powered attack damage only, which is the same scope as Strength. Orbs, relics and powers that
  deal unpowered damage get no bonus.
- The 3 start relics are in vanilla's shared pool, so every character can get them. King Midas gives only gold
  relics, so nothing is given twice. The modifier skips a relic the player already holds.

## Compatibility

| Mode | Decision |
|------|----------|
| Spoon | Combine. Max HP still grows; the heel ignores HP. |
| Blood | Combine. HP is the price and the heel ignores HP. |
| Cursed Deck | Combine. Curse HP losses are not attacks. |
| King Midas | Combine. Midas gold relics and the 3 Block relics are all given. |
| Deck of 10, Matriochka, Folie à deux, The Forge | Combine. |
| Monster Damage modifier | Combine. Only Block matters, so damage scaling makes the heel harder to protect. |
| Every other mode | Combine. |

## Not in scope

- No change to HP, max HP or healing.
- The uncommon and rare Block relics stay in the normal pools, with no extra lean toward them.
