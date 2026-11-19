# Glass Cannon — design

Owner idea (2026-11-18 backlog, reworked 2026-11-19): a Custom Run mode that makes players give up defense and
build toward killing monsters and bosses in one big turn. The backlog's first version ("halved max HP, damage ×1.5")
is dropped. It overlapped with the Monster HP / Monster Damage percent modifiers. The mode does not punish Skills or
Powers. It punishes only Block played from cards.

## Owner decisions (2026-11-19)

- **Start relics.** Every player starts with the mode's relic, plus **Vajra** and **Girya** (vanilla).
- **The mode's relic** applies two things at the start of every combat:
  - **Frail 99**: vanilla Frail, −25 % Block from cards.
  - **The new power** (described below).
- **The power stacks.**
  - Every card played adds 1 stack, whatever its type. Skills and Powers feed the combo too.
  - Each stack gives +10 % damage, linear like vanilla Slow: 10 stacks = ×2.
  - Stacks last across turns until the holder plays a Block card.
  - Stacks reset to 0 between combats.
- **Block card.** A card that is not an Attack and gains Block. Attacks that gain Block, such as Iron Wave, are not
  Block cards and never break anything.
- **Strength streak.** Each turn that ends with no Block card played gives Strength for the rest of the combat. The
  amount is the length of the streak: +1 on the first turn, +2 on the second, +3 on the third, so +6 after three turns.
- **The Corrupt Heart is unchanged.** Invincible and Beat of Death stay vanilla, so the minimum is 3 turns up to A9 and
  4 at A10. Solving the Heart is the player's job.
- **Achilles' Heel combines with Glass Cannon**, an absurd combination the owner allows. With both modes, card offers
  lean toward Block cards and toward Attacks, and other cards weigh less.

## Approaches

- **Chosen: a relic that carries the rules, plus a modifier that hands it out.** This is the pattern of Achilles' Heel,
  Deck of 10 and Blood Fang (owner's choice). The rules show on a relic the player can inspect.
- **Rejected: halved max HP and damage ×1.5.** It duplicates the percent modifiers and doesn't push away from Block.
- **Rejected: rules on the modifier only.** The player would have nothing to inspect.

## Names (builder proposal, not yet confirmed by the owner)

| Thing | Id | English | French |
|-------|----|---------|--------|
| Mode | `SpoonGlassCannon` | Glass Cannon | Canon de verre |
| Relic | `SpoonGlassHeart` | Glass Heart | Cœur de verre |
| Power | `SpoonFrenzyPower` | Frenzy | Frénésie |

The owner first called the power "Rampage". That name is taken by the Ironclad card Rampage ("Saccage") and by the
mod's own `STS1_RAMPAGE`, so the power needs another name.

## Builder calls

- **The power is the mod's own Buff** (`SpoonFrenzyPower`). It is not a Debuff, so Artifact can't block it, and it is
  never removed during the combat. It tracks two numbers, the stacks and the streak. The power's displayed amount is
  the bonus percent, as Slow does it.
- **Damage**: a multiplicative modifier, 1 + 0.1 × stacks, on powered attacks the holder deals
  (`props.IsPoweredAttack()`, the scope of Slow and Strength). Orbs, Poison, Thorns and other unpowered damage get no
  bonus.
- **Osty (owner, 2026-11-19)**: Frenzy applies to the Necrobinder's own attacks and also multiplies the powered attacks of the holder's Osty (the dealer is
  `Owner.Player.Osty`). The streak Strength stays vanilla: it is the Necrobinder's own and doesn't reach Osty.
- **Stack gain**: `AfterCardPlayed` for a card the holder played. A Block card resets the stacks to 0 instead of adding
  one. Cards played by other effects, such as Havoc-style autoplay, count as played.
- **Streak**: at the end of the holder's turn, if no Block card was played that turn, the streak goes up by 1 and the
  holder gains Strength equal to the new streak. A Block card resets the streak to 0, but the Strength already gained
  stays. Strength is granted at end of turn, so the first turn's attacks don't get it. Owner's "+6 after three turns"
  counts end-of-turn gains.
- **Block card test**: reuse the Achilles' Heel test (`AchillesBlock.IsBlockCard`: `GainsBlock` or a `BlockVar`), and
  require any type other than Attack or Power. Power cards never break the combo, even those with a `BlockVar`
  (Pillar of Creation, Shroud): the owner does not want Powers discouraged, and Feel No Pain-style Powers already
  escape the test. The same test serves the Achilles lean.
- **Block from relics and powers** (Metallicize, Anchor, Orichalcum, Feel No Pain...) is not a card. It doesn't break
  the stacks or the streak. Frail still reduces Block from cards only, as in vanilla.
- **Frail 99** is applied with `PowerCmd.Apply<FrailPower>` at combat start. Artifact blocks it as it would any Frail
  (vanilla behavior, a valid player answer). Frail ticks down 1 per turn, and no combat lasts 99 turns.
- **Combat start order**: the relic applies its powers in `AfterRoomEntered` for a `CombatRoom`, as Vajra and Girya do.
- **Start relics**: given in `AfterRunCreated` and removed from `player.RelicGrabBag` and `SharedRelicGrabBag`, the
  pattern of the Midas, Spoon and Achilles start relics. A relic the player already holds is skipped. Girya's
  `IsAllowed` (before the act 3 chest) only filters random draws, so the start grant is unaffected.
- **Co-op**: everything is per player. Each player's stacks and streak depend only on their own cards.
- **Achilles' Heel lean**: for a player holding both relics, a card weighs ×2 if it is a Block card or an Attack, and
  never ×4 when it is both. Other cards keep weight 1, so they weigh less only in proportion. The Achilles-only lean
  (Block ×2) is unchanged. Glass Cannon alone has no lean. `OfferWeights.Compute` gets the attack set next to the
  block set, and equal weights still mean a vanilla draw.
- **Numbers** live in a pure rule, `src/Core/GlassCannonRule.cs`: 10 % per stack, Frail 99, the streak Strength, the
  Block card decision from plain flags (type, gains Block), and the start relic ids (`VAJRA`, `GIRYA`). xUnit tests
  cover them.
- **Text**: EN and FR for the mode, the relic and the power, kept short (owner rule: let players discover). Card and
  power wording follows the game's ("Armure" in French for Block, "Fragile" for Frail).
- **Art**: placeholder Pixabay images for the relic and the power icon, or an icon borrowed from a vanilla power through
  `IBorrowedIcon`. Sources go in `docs/backlog.md`.

## Compatibility

| Mode | Decision |
|------|----------|
| Achilles' Heel | Combine (owner). Offers lean toward Block cards and Attacks. Frail and the heel stack up: the player must choose. |
| Spoon | Combine. Max HP and Glass Cannon don't interact. |
| Blood | Combine. |
| Cursed Deck | Combine. Playable curses count as played cards and give stacks. |
| King Midas | Combine. Midas gold relics, Vajra and Girya are all given. Midas rest sites still charge for Lift. |
| The Forge | Combine. A forged card that gains Block is still a Block card. |
| Deck of 10, Matriochka, Folie à deux, Mutation, Trophies, Forced Trade, Conductor, Useless Relics | Combine. |
| Monster HP / Monster Damage modifiers | Combine. |
| Every other mode | Combine. |

## Not in scope

- No change to HP, max HP, healing or to any monster, the Heart included.
- No lean toward Attacks when Glass Cannon is alone.
- No change to Block that doesn't come from cards.
