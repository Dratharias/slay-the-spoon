# The Forge — design

Status: approved decisions from the owner (2026-11-18 Q&A); builder calls marked **[builder]**.

## Intent

A Custom Run mode built around duplicates. A new rest site option fuses two copies of the same card. Fusion is the only way
past +1, up to +9, so the run becomes about collecting duplicates and pushing a few cards far up their upgrade curve.
The merchant sells copies of your own cards, which makes duplicates something you can plan for.

## Goals

- A new rest site option, **Fuse**, merges two copies of a card. The levels add up: the result is **kept level + sacrificed level + 1**,
  capped at +9 or at the card's own cap.
  - Strike + Strike = Strike+1
  - Strike+1 + Strike = Strike+2
  - Strike+1 + Strike+1 = Strike+3
  - Strike+3 + Strike+3 = Strike+7
- Every level past +1 repeats the card's own +1 gain ("follow the curve"). For example, Bash gains +2 damage and +1
  Vulnerable per level.
- The cards with complicated mechanics are reviewed by hand. Each gets a per-level gain, a cap, or no fusion at all.
  Limit Break is the example: no ×10 Strength.
- A merchant service, **Duplicate**, sells a +0 copy of a card in your deck.

## Non-goals

- No change to card rewards: the merchant is the way to find duplicates.
- No new art or cards. The level shows in the vanilla title ("Strike+3").
- No Standard or Daily access.

## Anti-goals

- No card whose cost goes negative, and no keyword applied twice.
- No reviewed card left uncapped when it scales out of control (multipliers, repeats, extra turns).
- No run made unwinnable because a card's level fails to load.

## Owner decisions

1. **Approach A:** a generic rule plus a review table (details below).
2. **Addition rule:** kept + sacrificed + 1. Upgrading copies before fusing them is worth it.
3. **Only fusion goes past +1.** Smith, in-combat upgrades (Armaments, Apotheosis), relics and events upgrade +0 → +1
   only. A card at +1 or more is no longer offered to them.
4. **Merchant Duplicate service:**
   - The copy is always +0, whatever the source card's level.
   - One copy per merchant visit.
   - The price starts at a base set by rarity, then adds +25 per earlier purchase. The purchase count is per player, over
     the whole run, like card removal.

     | | Basic / Common | Uncommon | Rare |
     |---|---|---|---|
     | Ascension 0–5 | 25 | 50 | 75 |
     | Ascension 6+ (Inflation) | 50 | 75 | 100 |

     Colorless cards follow their own rarity. **[builder]** Special-rarity cards (Event, Token, Ancient…) price as Rare.
5. **The cards with complicated mechanics are listed and their gain per level is evaluated together**, owner and builder.

## Approaches considered

- **A — generic rule + review table (picked):**
  - Level +1 is the vanilla upgrade, as it is now.
  - Every later level repeats only the **value** changes that level +1 made: damage, Block, stacks, cards drawn, and so on.
  - The cost change and keyword changes apply once, at +1.
  - A data table overrides this per card: a cap, a custom per-level gain, no fusion, or a cost that keeps dropping.
  - Every card works on day one: vanilla, StS1 ports and the Watcher.
- **B — table only:** no card goes past +1 until it has an approved row. This is safer, but it needs hundreds of rows
  before the first run, and a forgotten card stays stuck.
- **C — one C# class per card:** full control, but unrealistic for 500+ cards. It is kept only as an escape hatch for a
  table row that needs code.

## How it works

### Modifier

- `SpoonForge`, a Custom Run row like King Midas. EN title "The Forge", FR title "La Forge"; short descriptions.
- Not exclusive with the other modes. **[builder]** In a King Midas run there are no card rewards, so the merchant and
  the starter deck are the only sources of duplicates. That is acceptable.

### Upgrade levels

- The game supports levels natively:
  - `CardModel.MaxUpgradeLevel` is virtual;
  - each `UpgradeInternal` increments `CurrentUpgradeLevel` and reruns `OnUpgrade`;
  - the title shows `+N` when the max is over 1;
  - `SerializableCard.CurrentUpgradeLevel` is saved, up to 8 bits.
- In a Forge run, the effective max level of a card is 9, or the table's cap.
- **Load order risk:** setting `CurrentUpgradeLevel` above `MaxUpgradeLevel` throws. The higher max must already apply
  while a saved run's deck and combat piles are loaded, and on every co-op client. The builder must check this before
  anything else.
- **The level gate (decision 3):**
  - Ordinary upgrade paths see the card as not upgradable once it is at +1 or more (`IsUpgradable` false).
  - Fusion sets the level directly, past the gate.
  - **[builder]** StS1 Searing Blow keeps its own StS1 rule: any upgrade raises it, as in StS1.
- **Repeating the curve:**
  - Level +1 runs the card's `OnUpgrade` normally.
  - Levels 2 and up apply only the dynamic-var deltas that level +1 produced.
  - A table row can replace this with its own per-level deltas.
  - **[builder]** Recording the level +1 delta (a canonical clone upgraded once, or a diff around `OnUpgrade`) is the
    builder's call. It must be deterministic and identical on every co-op client.
- **Combat copies:** cards copied in combat (Dual Wield, Nightmare and so on) keep their level, as in vanilla.

### Review table

- `data/forge/curves.json`, keyed by card id. Each row can set:
  - `cap` (1–9);
  - `fusable: false`;
  - `perLevel` (var → delta);
  - `costPerLevel` (with a floor of 0);
  - `note`.
- `tools/forge/` (Python, Podman image like `tools/sts1/`) builds the review page `docs/review/forge-curves.md`.
  - It reads the local decompile (`~/Projects/sts-decomp/sts2-0.111.1/`) and our own card sources.
  - It writes only card ids and numbers. No decompiled code is ever committed.
- The page lists each card's curve from +1 to +9 under the generic rule. A **"to review"** section comes first, holding
  every card whose upgrade:
  - changes the cost or a keyword;
  - doubles or multiplies something (Limit Break);
  - repeats, replays or adds plays or turns (Echo Form, Burst, extra turns);
  - is a Power that scales over the combat (Demon Form);
  - or has no dynamic-var change at all.
- The owner and the builder fill the table from that section. Cards with no row follow the generic rule.

### Fuse (rest site option)

- Text: EN "Fuse", FR "Fusionner". It costs the rest site action, like Smith.
- It is enabled when the player's deck has two copies (see **Copies** below) where the result stays within the cap and the
  card is fusable. Otherwise it is greyed out and its hover says why.
- Flow:
  1. Pick the card to keep, from cards with at least one fusable copy.
  2. Pick the copy to sacrifice.
  3. A preview of the result appears (Strike+3, with its new numbers); for a StS1/StS2 pair, the player picks the
     version here.
  4. Confirm.
- The sacrificed copy leaves the deck. **[builder]** It is removed as in a card removal, so "on remove" relic triggers
  behave as they do for a removal.
- **Copies:** the same card id, or a StS1 card and its StS2 namesake (owner, 2026-11-18). Namesake pairs are every StS1
  entry with an `Sts2Id` (`Duplicate` pairs from `VersionPairs`, plus `SameKeepSts1` and `Retired` namesakes that can still
  sit in a deck). In Hybrid runs, Rampage StS1 + Rampage StS2 fuse.
  - When the two cards are different versions, the player picks which version the result is, in the preview step
    (like the Hybrid swap button). The result level is still kept + sacrificed + 1, applied to the chosen version's curve,
    with that version's cap and `fusable` flag.
  - **[builder]** The choice is synced in co-op like the other version choices (`VersionChoiceCode`).
- Co-op: each player fuses in their own deck. The option is synced like the vanilla rest site options.
- **[builder]** Enchantments: the kept card keeps its enchantment and the sacrificed card's enchantment is lost. The
  preview says so.

### Duplicate (merchant service)

- Text: EN "Duplicate", FR "Dupliquer". A slot next to card removal, priced as in decision 4.
- Picking it opens a grid of your deck. Curses and Status cards are excluded. The +0 copy is added to the deck.
- One use per merchant visit. **[builder]** The price is computed from the run's ascension and the player's own purchase
  count, which is saved with the run.

## Left to the builder

- How the level +1 delta is recorded and replayed (deterministic).
- Patch points: `MaxUpgradeLevel`, `IsUpgradable`, the rest site option list, the merchant inventory, and save/load
  timing.
- The UI for the two-step pick and the preview (vanilla card-grid and upgrade-preview nodes).
- The `tools/forge` extraction heuristics.
- EN/FR text, tests in `src/Core/ForgeRule.cs` (level arithmetic, prices, the table parse) and an in-game checklist
  `docs/testing/forge-checklist.md`.
