# Watcher Task 6 — data and generated content

Implemented on `step-7-watcher`. In-game validation is still pending.

## Data pipeline

`make sts1-data` includes purple without running duplicate matching: the Watcher has no native StS2 pool.
The local `CardLibrary.addPurpleCards` allowlist produces 75 cards, excluding Discipline and Unraveling.
`watcher-generated.json` contains eight generated cards plus five choice cards (three for Wish, two for Stance Potion).
Starter class names map to the existing model IDs, including Strike_Purple → Sts1StrikeWatcher.

The remaining 71 card skeletons live in `data/sts1/watcher-skeletons/`, outside compilation and model discovery.
Tasks 7–9 must port them into `src/Cards/Sts1/Watcher/`; generating data must not introduce playable cards that throw.
The four existing starter implementations and all non-Watcher data/portraits remain unchanged.

The token classes inherit `Sts1WatcherTokenCard`, not `Sts1WatcherCard` or `Sts1ColorlessCard`.
Their hidden pool is not registered in `AllCardPools`, and cannot enter rewards, shops or transforms.
All eight tokens and five options have EN/FR text and extracted portraits. Options apply their effects directly,
without generating a card-play event. Wish itself is still a Task 8 port.

## Cards and effects

| Card | Effect | Upgrade |
| --- | --- | --- |
| Miracle | 0 cost, Retain, Exhaust; 1 Energy | 2 Energy |
| Insight | 0 cost, Retain, Exhaust; draw 2 | Draw 3 |
| Smite | 1 cost, Retain, Exhaust; 12 attack damage | 16 damage |
| Safety | 1 cost, Retain, Exhaust; 12 Block | 16 Block |
| Through Violence | 0 cost, Retain, Exhaust; 20 attack damage | 30 damage |
| Expunger | 1 cost, 9 attack damage per supplied hit; neither Retain nor Exhaust | 15 damage per hit |
| Beta | 2 cost, Exhaust; shuffle an unupgraded Omega into the draw pile | 1 cost |
| Omega | 3 cost, Power; 50 unpowered damage to all enemies at the owner's turn end | 60 damage |

`Sts1Expunger.SetHits` receives Conjure Blade's eventual X result; dynamic vars preserve it on combat clones.
The creator is responsible for the X upgrade/relic adjustments when Task 8 ports Conjure Blade.

Wish options are Strength 3/4, Plated Armor 6/8, and Gold 25/30. StS2's Plating loses stacks each turn,
so a dedicated `Sts1PlatedArmorPower` preserves StS1's loss on unblocked attacks instead.
Stance Potion offers Wrath then Calm through the native synchronized choice screen.

## Relics and potions

| Relic | Rarity/source | Effect |
| --- | --- | --- |
| Pure Water | Starter | One Miracle before the first draw |
| Holy Water | Inactive former boss relic | Removes Pure Water on pickup; three Miracles before the first draw |
| Damaru | Common | One Mantra before the owner's draw, after the previous Divinity ends |
| Teardrop Locket | Uncommon | Begin combat in Calm |
| Violet Lotus | Inactive former boss relic | Leaving Calm gives one additional Energy |
| Melange | Shop | Scry 3 after the holder shuffles |
| Golden Eye | Rare | Scry shows two more cards, capped by the draw pile |
| Cloak Clasp | Native shared Rare | Reused as-is; no duplicate registration |
| Duality | Uncommon | One temporary Dexterity for each attack played; uses the native temporary-stat lifecycle |

Holy Water and Violet Lotus use Event rarity, reject `IsAllowed` and shops, and have no event/acquisition route.
This follows the main backlog's inactive-boss decision; it does not introduce a boss reward or Custom option.
Their presence in the Watcher pool serves model lookup, not ordinary acquisition.

Bottled Miracle (Common) creates two Miracles; Stance Potion (Uncommon) enters Calm or Wrath;
Ambrosia (Rare) enters Divinity. All are self-targeted combat-only potions. Borrowed art is Energy Potion,
Skill Potion and Cosmic Concoction respectively, including outlines and preload paths.
Relic icons, outlines and large images are extracted from StS1 (all nine retained as source assets;
Cloak Clasp uses native rendering at runtime).

## Testing access

- `sts1card STS1_MIRACLE` (and the other token IDs) uses the existing hidden-card command.
- `sts1watcheritem relic STS1_HOLY_WATER` grants a hidden relic explicitly, including inactive boss ports.
- `sts1watcheritem potion STS1_STANCE_POTION` requests a potion; an empty belt slot is required.
- `sts1scry 3` and `sts1stance calm` exercise Golden Eye and Violet Lotus.

These commands are networked and scoped to the issuing player. No game installation or automated clicks are part of this task.
