# Step 2b — Ironclad namesakes and the hybrid version swap — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port the three Ironclad namesakes that don't play as their StS2 card (Juggernaut, Rampage, Rupture) and let Hybrid players pick the version of a duplicate card.

**Architecture:**
- Juggernaut and Rampage are `same-sts1`: the StS1 version is the weaker one and replaces the StS2 card in both StS1-only and Hybrid (the composer already does this through `Sts2Counterpart`).
- Rupture is a `duplicate`. In Hybrid it is **one slot** in the pool: the StS2 card. Whenever a card is created in a Hybrid run, a duplicate comes out as either version at random (run RNG, so co-op clients agree).
- On the card reward screen, a duplicate shows a swap button. Pressing it picks one of the reward's **alternatives**, the game's own synced mechanism for Skip / Reroll: every client runs the swap on its copy of the reward, so decks stay in sync. The alternatives are hidden from the alternatives bar; only the per-card button is visible.

**Tech Stack:** C# .NET 9, Harmony, Godot 4.5.1 nodes, xUnit; Python codegen in Podman.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3.4 duplicates, §4 hybrid choice, §7 step 2).

## Global Constraints

- English and French only; every new text in `godot/SlaySpoon/localization/{eng,fra}/`.
- Never write the regular save. Never commit decompiled code.
- Standard and Daily runs stay vanilla: every new patch returns early unless the run's mode is Hybrid.
- No in-game test now (the owner is playing): `make build` and `make test` only; never `make up` while `pgrep -x SlayTheSpire2` finds the game.
- Commit on `step-2`; no push.

## Review Focus

1. **Co-op desync on swap:** the swap must happen on every client, in the same order as the choice. It rides the synced alternative index; the alternative list must be the same length on every client and stay stable after a reroll (our alternatives go first, fixed count).
2. **Indices after reroll:** vanilla keeps the alternative list computed at the start of `OnSelect`; the screen gets a fresh one. Ours being a fixed-size prefix keeps Skip at the same index in both.
3. **Standard / StS1-only runs:** no alternatives added, no random swap, no button.
4. **Upgraded duplicates:** a swapped card keeps the upgrade level.
5. **Random version determinism:** the coin flip uses `RunState.Rng.Niche`, consumed only on the code paths every client runs.

---

### Task 1: Duplicates are one slot in Hybrid; version pairs

**Files:**
- Modify: `src/Core/CardSetComposer.cs`, `tests/CardSetComposerTests.cs`
- Create: `src/Core/VersionPairs.cs`, `tests/VersionPairsTests.cs`

**Interfaces:**
- Produces: `VersionPairs.Of(IEnumerable<Sts1Entry> entries) -> IReadOnlyDictionary<string, string>` mapping each `Duplicate` StS1 id to its StS2 id and back.

- [ ] **Step 1:** Change `Compose_Hybrid_DropsOnlySameKeepSts1Claims` to expect `AGGRESSION, ANGER, BASH, RUPTURE, STS1_CLEAVE, STS1_RAMPAGE` (no `STS1_RUPTURE`), rename it `Compose_Hybrid_DuplicateIsOneSlot`; add `VersionPairs_MapBothWays` (RUPTURE↔STS1_RUPTURE, no RAMPAGE/CLEAVE keys).
- [ ] **Step 2:** `make test` → both fail.
- [ ] **Step 3:** Hybrid drops `Duplicate` entries from the StS1 side; write `VersionPairs`.
- [ ] **Step 4:** `make test` → green. Commit `feat: hybrid duplicates are one slot`.

### Task 2: Port Juggernaut, Rampage, Rupture

**Files:**
- Create via `codegen.py --relations same-sts1,duplicate`: `src/Cards/Sts1/Ironclad/Sts1{Juggernaut,Rampage,Rupture}.cs` + text.
- Create: `src/Cards/Sts1/Powers/Sts1RupturePower.cs`; loc in `powers.json`.
- Modify: `docs/testing/step-2a-checklist.md` → new section in `docs/testing/step-2b-checklist.md`.

- Juggernaut: `Relation => SameKeepSts1`, `Sts2Counterpart => typeof(Juggernaut)`; vanilla `JuggernautPower` with 5 (+2).
- Rampage: same-sts1; 8 damage, +5 (+3 upgraded) damage per play this combat (vanilla pattern, including `AfterDowngraded`).
- Rupture: `Duplicate`, counterpart `Rupture`; `Sts1RupturePower`: whenever the owner loses HP **from its own source** (dealer is the owner: cards, Brutality, Combust), gain Amount Strength at once, on any turn. 1 (+1).

- [ ] Run codegen, port the three, `make build` (no new warnings), `make test` (assets, no-unported, placeholders) green, checklist lines, commit `feat: port StS1 Juggernaut, Rampage, Rupture`.

### Task 3: Random version on creation (Hybrid)

**Files:**
- Create: `src/CardSets/VersionSwap.cs` (game side: `CardModel? Partner(CardModel)`, `CardModel Swap(CardModel)` creating the partner in the card's scope with the same upgrade level), `src/CardSets/RandomVersionPatch.cs`.
- Modify: `src/CardSets/CardSetPools.cs` (expose the pairs from the composed entries).

- Postfixes, Hybrid only, swap each produced duplicate when `RunState.Rng.Niche.NextBool()`:
  `CardFactory.CreateForReward`, both `CreateForMerchant`, `GetDistinctForCombat`, `GetForCombat`, both `CreateRandomCardForTransform`.
- A `CardCreationResult` is rebuilt around the swapped card (relic flashes are lost; acceptable).
- [ ] `make build`, `make test`, checklist lines, commit `feat: hybrid duplicates come out as a random version`.

### Task 4: Swap button on the card reward screen

**Files:**
- Create: `src/CardSets/RewardSwapPatch.cs`; loc `godot/SlaySpoon/localization/{eng,fra}/card_reward_ui.json` (`OPTION_SPOON_SWAP.name`) and a `SPOON_SWAP_BUTTON` label.

- Postfix `CardRewardAlternative.Generate`: Hybrid only, insert `SwapSlots = 8` alternatives at index 0, `OptionId "SPOON_SWAP"`, `AfterSelected = DoNothing`; alternative k swaps `_cards[k]` if it has a partner (`VersionSwap.Swap`), then refreshes the reward's shown screen.
- Postfix `NCardRewardSelectionScreen.RefreshOptions`: free the first `SwapSlots` alternative buttons it just created (only when the options start with ours); under each card holder whose card has a partner, add a game-styled button (`NCardRewardAlternativeButton.Create`) that calls the screen's `OnAlternateRewardSelected(k)`.
- [ ] `make build`, `make test`, checklist lines, commit `feat: swap a duplicate's version on the card reward screen`.

### Task 5: Wrap-up

- README (Features: namesakes, version swap), CLAUDE.md (where we are, code map), checklist header, backlog: swap button on the merchant, choose-a-card screens and Sealed Deck (these show a random version for now).
- `make build`, `make test`; commit `docs: step 2b wrap-up`.
