# Cursed Deck Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The Custom Run mode **Cursed Deck** (`SpoonCursedDeck`): curses become playable through the Unholy Grimoire, hit
the targeted enemy with their own effect, can be upgraded, events offer a curse for a reward, and relic offers are
mostly cursed relics.

**Architecture:** Pure numbers and the curse table parser live in `src/Core/CurseRule.cs` (xUnit-tested, reads the
embedded `data/curses/curses.json`). Game glue lives in `src/Curses/`: the mode, card patches (keywords, target, cost,
upgrade), the Grimoire's played effect, the event option, the relic draw patch and four relics. The StS1 Parasite and
Pride are ported next to Pain in `src/Cards/Sts1/Curses/`.

**Tech Stack:** C# .NET 9, Harmony, StS2 0.111.1 API (decompile `~/Projects/sts-decomp/sts2-0.111.1/`), xUnit, pytest.

**Spec:** `docs/superpowers/specs/2026-11-18-cursed-deck-design.md`, curse table `data/curses/curses.json`, owner page
`docs/review/cursed-curses.md`.

## Global Constraints

- Custom Run only, off by default; Standard and Daily untouched; vanilla RNG streams never consumed by the mod (own `Rng`
  from the run seed and a stream name).
- Every added text in `godot/SlaySpoon/localization/eng/` and `.../fra/`; menu texts short.
- Harmony helpers are never named `Prefix`, `Postfix`, `Transpiler`, `Finalizer` or `Prepare`.
- `data/curses/curses.json` is the only source of curse numbers; never hardcode a curse's amounts in C#.
- Version 1.6.0 in `mod/SlaySpoon.json` and `src/ModEntry.cs`.
- Build 0 warnings; `make test` green. Commit on `backlog-modes`, no push, no Workshop upload.

## Review Focus

1. A curse in a run **without** the mode (or a player without the Grimoire) stays exactly vanilla: unplayable, +0, no
   target. Test: `CurseRuleTests.PlayableOnlyWithModeAndGrimoire`.
2. Necronomicurse+ never costs 0 (it returns to the hand when exhausted: infinite loop). Test:
   `CurseRuleTests.NecronomicurseKeepsCostOne`.
3. A cursed relic draw never gives a relic the player (or, for the shared bag, any player) already holds, and falls back
   to the vanilla draw when none is left. Test: `CurseRuleTests.CursedRelicPickSkipsHeld`.
4. Save/reload: an upgraded curse reloads upgraded (vanilla caps curses at +0 in `FromSerializable`). Owner checklist item.
5. Co-op: the event option, the relic roll and the start curse give the same result on every client (seeded streams,
   no `System.Random`). Owner checklist item.

---

### Task 1: Curse rules and the table

**Files:**
- Modify: `src/Core/CurseRule.cs`, `src/SlaySpoon.csproj` (embed `../data/curses/curses.json` as `SlaySpoon.curses.curses.json`)
- Test: `tests/CurseRuleTests.cs`

**Interfaces:**
- Produces:
  - `record CurseRow(string Id, string Played, int Amount, int AmountPlus, string? SelfVar, int[]? Self, int CostPlus)`
    (`Played` is one of `None, HpLoss, Damage, HpLossPerHandCard, HpLossPerCardPlayed, Weak, Vulnerable, GainGold, MaxHpLoss`;
    `CostPlus` defaults to 0).
  - `CurseRule.ParseTable(string json) -> IReadOnlyDictionary<string, CurseRow>` (skips `_` keys).
  - `CurseRule.GrimoireDamage = 5`, `CurseRule.GrimoireNoEffectPlus = 12`, `CurseRule.PlayCost = 1`.
  - `CurseRule.GrimoireDamageFor(CurseRow? row, bool upgraded) -> int` (12 only when upgraded and `Played == "None"`, else 5;
    a curse without a row counts as `None`).
  - `CurseRule.CostFor(CurseRow? row, int vanillaCost, bool upgraded) -> int`: not upgraded → `vanillaCost >= 0 ? vanillaCost : 1`;
    upgraded → `row.CostPlus` (0 by default).
  - `CurseRule.EnemyAmount(CurseRow row, bool upgraded) -> int`.
  - `CurseRule.IsPlayable(bool modeActive, bool holdsGrimoire) -> bool`.
  - `CurseRule.CursedRelicRoll(int roll) -> bool` (true for roll % 3 != 0: 2 in 3).
  - `CurseRule.PickCursedRelic(IReadOnlyList<string> pool, ISet<string> held, int roll) -> string?`.
  - `CurseRule.EventReward(int roll, bool cursedRelicLeft) -> CurseEventReward` (enum `CursedRelic, Gold, Heal`; equal
    thirds, `CursedRelic` folds into `Gold` when none is left), `CurseRule.EventGold = 75`, `CurseRule.EventHealPercent = 25`.
  - Stream names: `CurseRule.RelicStream(int floor, int draw)`, `CurseRule.EventStream(int floor, int slot)`,
    `CurseRule.StartStream(int slot)`.

- [ ] **Step 1: Write the failing tests** in `tests/CurseRuleTests.cs`, reading the repo's `data/curses/curses.json`:
  `ParsesEveryRow` (22 rows; `BAD_LUCK` = HpLoss 13/20 selfVar HpLoss), `GrimoireTwelveOnlyForUpgradedNoEffect`
  (CLUMSY+ 12, CLUMSY 5, BAD_LUCK+ 5), `NecronomicurseKeepsCostOne` (`CostFor(STS1_NECRONOMICURSE, -1, true) == 1`,
  `CostFor(BAD_LUCK, -1, true) == 0`, `CostFor(ENTHRALLED, 2, false) == 2`, `CostFor(BAD_LUCK, -1, false) == 1`),
  `PlayableOnlyWithModeAndGrimoire` (only `(true, true)` is true), `TwoInThreeRelicRoll` (rolls 0..2: false, true, true),
  `CursedRelicPickSkipsHeld` (all held → null; one left → that one), `NormalityAndGuiltyNerfs` (`Self` = [3,4] and [5,3]).
- [ ] **Step 2:** `make test` → the new tests fail to compile.
- [ ] **Step 3:** Implement in `src/Core/CurseRule.cs` (System.Text.Json; keep `GrimoireDamage`).
- [ ] **Step 4:** `make test` → green.
- [ ] **Step 5:** Commit `feat(cursed-deck): curse rules and table`.

### Task 2: StS1 Parasite and Pride

**Files:**
- Create: `src/Cards/Sts1/Curses/Sts1Parasite.cs`, `src/Cards/Sts1/Curses/Sts1Pride.cs`; portraits beside Pain's (same tool
  and folder Pain used); EN/FR text in `cards.json` (StS1 wording from `~/Projects/sts-decomp/sts1/localization/{eng,fra}/cards.json`).
- Modify: `tools/curse_table.py` (drop `TO_PORT`), `tools/tests/test_curse_table.py` if needed.

**Interfaces:**
- Produces: `Sts1Parasite` (Unplayable; var `MaxHp` 3; on removal from the deck the owner loses `MaxHp` max HP — find the
  StS2 hook for "removed from deck" in the decompile) and `Sts1Pride` (cost 1, Innate, Exhaust; end of turn in hand: a
  copy goes on top of the draw pile through `HasTurnEndInHandEffect`/`OnTurnEndInHand`). Both `Sts1CurseCard`, hidden pool.

- [ ] **Step 1:** Write both classes and their text; the existing repo tests (text in both languages, portrait, placeholders) cover them.
- [ ] **Step 2:** `python3 tools/curse_table.py --sts2 ~/Projects/sts-decomp/sts2-0.111.1/MegaCrit.Sts2.Core.Models.Cards` → "22 curses reviewed"; `make test` green.
- [ ] **Step 3:** Commit `feat(curses): StS1 Parasite and Pride`.

### Task 3: The mode and its start

**Files:**
- Create: `src/Curses/SpoonCursedDeck.cs` (modifier), `src/Curses/CursedDeckCurses.cs` (random curse helper).
- Modify: `src/CustomRunUi/ModifierListPatch.cs`, `src/TopBar/TopBarPercentPatch.cs` (the flag list on line 48),
  `modifiers.json` EN/FR (`SPOON_CURSED_DECK.title` "Cursed Deck" / "Deck maudit", short description, `.short`).

**Interfaces:**
- Produces: `SpoonCursedDeck.IsActive(IRunState?) -> bool`; `CursedDeckCurses.Pool(Player) -> IReadOnlyList<CardModel>`
  (the game's curse pool cards with `CanBeGeneratedByModifiers`, plus `Sts1Pain`, `Sts1Parasite`, `Sts1Pride`; never
  Ascender's Bane, Curse of the Bell or the Necronomicurse) and `CursedDeckCurses.AddRandom(Player, Rng) -> Task<CardModel>`.
- Start (`AfterRunCreated`): every player gets the Grimoire (removed from every grab bag, like `SpoonDeckOfTen`) and one
  random curse on `CurseRule.StartStream(slot)`.

- [ ] **Step 1:** Implement; `make build` 0 warnings.
- [ ] **Step 2:** Commit `feat(cursed-deck): the mode gives the Grimoire and a curse`.

### Task 4: Playable curses and their effect on the enemy

**Files:**
- Create: `src/Curses/PlayableCursePatch.cs`, `src/Curses/CurseTable.cs` (embedded table loader, like `ForgeCurves`).
- Modify: `src/Curses/SpoonUnholyGrimoire.cs`, relic text EN/FR (mention playable curses in this mode only if it fits short).

**Interfaces:**
- Consumes: Task 1 (`CurseRule`, `CurseRow`), Task 3 (`SpoonCursedDeck.IsActive`).
- Produces: `CursePlay.IsPlayableCurse(CardModel) -> bool` (Curse type, mode active, owner holds the Grimoire, in combat).
- A postfix on `CardModel.GetKeywordsWithSources` removes `Unplayable` and adds `Exhaust` for playable curses; a postfix on
  the `TargetType` getter returns `AnyEnemy`; the cost comes from `CurseRule.CostFor` (find the hook: `SetCustomBaseCost` on
  the combat copies at `BeforeCombatStart` and for curses generated mid-combat, or a cost getter postfix: base −1 is
  returned unmodified by `CardEnergyCost.GetWithModifiers`).
- The Grimoire's `AfterCardExhausted` deals `CurseRule.GrimoireDamageFor`; its `AfterCardPlayed` applies the row's
  `Played` effect to `cardPlay.Target` with `CurseRule.EnemyAmount` (HpLoss unblockable; Damage unpowered; per hand card
  counted after the curse left the hand; per card played this turn from combat history; Weak/Vulnerable via
  `PowerCmd.Apply`; GainGold via `PlayerCmd.GainGold`; MaxHpLoss on the monster's max HP only).
- Enthralled and Spore Mind keep their vanilla play; Enthralled is exhausted too.

- [ ] **Step 1:** Implement; `make build` 0 warnings; `make test` green.
- [ ] **Step 2:** Commit `feat(cursed-deck): curses played through the Grimoire hit the targeted enemy`.

### Task 5: Upgraded curses

**Files:**
- Create: `src/Curses/CurseUpgradePatch.cs`; Regret, Normality and Guilty player-side patches in the same file.
- Modify: `src/Cards/Sts1/Curses/Sts1Pain.cs`, `Sts1Parasite.cs` (read their amount from the card var so the upgrade raises it).

**Interfaces:**
- Consumes: Task 1, Task 4's `CurseTable`.
- In a Cursed Deck run, every curse class's `MaxUpgradeLevel` getter (each overrides it: patch them by `TargetMethods`
  over the curse types) returns 1, and `FromSerializable` keeps a saved +1.
- On upgrade (postfix on `CardModel.OnUpgrade` for rows with `SelfVar`): the var's base goes from `Amount` to `AmountPlus`.
  Regret+: 2 HP per card; Normality+: limit `Self[1]` (4); Guilty+: leaves after `Self[1]` (3) combats; Pain+ 2; Parasite+ 5.
- The card text shows the upgraded numbers (vars) and the `+` title as for any card.

- [ ] **Step 1:** Implement; `make build`; `make test`.
- [ ] **Step 2:** Commit `feat(cursed-deck): upgraded curses`.

### Task 6: Event option "Embrace the curse"

**Files:**
- Create: `src/Curses/CurseEventOptionPatch.cs` (`EventModel.SetEventState` prefix, like `MidasEventOptionsPatch`).
- Modify: `events.json` EN/FR (`SPOON_CURSED_DECK_EMBRACE.title` "Embrace the curse" / "Embrasser la malédiction", description).

**Interfaces:**
- Consumes: Task 1 (`EventReward`, `EventStream`), Task 3 (`CursedDeckCurses.AddRandom`), Task 7's cursed relic pool
  (`CursedRelics.Pool`), or the 7 vanilla ones if Task 7 is not merged yet.
- Added once per event (first page only, not on Ancients), last before any option whose key ends in `LEAVE`/`PROCEED`.
  Chosen: add a random curse, then the reward (relic, 75 gold, or heal 25 % of max HP; Blood runs: the heal replaces gold),
  then the event proceeds to its usual end (`SetEventFinished`-style, as the vanilla "leave" does).

- [ ] **Step 1:** Implement; build; test.
- [ ] **Step 2:** Commit `feat(cursed-deck): every event offers a curse for a reward`.

### Task 7: Cursed relic draws and the four custom relics

**Files:**
- Create: `src/Curses/CursedRelics.cs` (pool + `RelicGrabBag.PullFromFront(RelicRarity, Func<RelicModel,bool>, IRunState)`
  prefix), `src/Curses/SpoonBloodPact.cs`, `SpoonPinDoll.cs`, `SpoonBlackCenser.cs`, `SpoonReliquary.cs`; icons in
  `godot/SlaySpoon/images/relics/spoon/` (Pixabay, same sizes as the Grimoire: 128 / outline / 256); `relics.json` EN/FR.

**Interfaces:**
- `CursedRelics.Pool -> IReadOnlyList<RelicModel>`: Calling Bell, Distinguished Cape, Neow's Bones, Cursed Pearl,
  Blood-Soaked Rose, Preserved Fog, Darkstone Periapt, and the four below (all `RelicRarity.Event`, in the event pool).
- The prefix, in Cursed Deck runs: roll `CurseRule.CursedRelicRoll` on `CurseRule.RelicStream(floor, draw)` (draw = a
  per-floor counter); on true, `CurseRule.PickCursedRelic` among relics not held (per-player bag: its owner; shared bag:
  any player) that pass `filter`; return it without touching the vanilla bag. Otherwise vanilla.
- Relics: **Blood Pact** whenever you draw a curse, gain 4 Block; **Pin Doll** start of combat, 1 Strength per 3 curses
  in the deck; **Black Censer** whenever a curse joins your deck, upgrade it; **Reliquary** end of combat, 3 gold per
  curse in the deck. Numbers in `CurseRule`.

- [ ] **Step 1:** Implement and fetch the art (WebSearch + web-fetch for the CDN URL, curl with `-A Mozilla/5.0 -e https://pixabay.com/`); sources in `docs/backlog.md`.
- [ ] **Step 2:** Build; test; commit `feat(cursed-deck): cursed relic draws and four cursed relics`.

### Task 8: Docs and version

**Files:** `docs/testing/cursed-deck-checklist.md` (every behavior above, Review Focus 4–5, compatibility table rows),
`docs/backlog.md` (Owner ideas: the mode is built), `CLAUDE.md` (1.6.0 entry), `mod/SlaySpoon.json`, `src/ModEntry.cs`.

- [ ] **Step 1:** Write; `make build test` green; `make workshop` stages.
- [ ] **Step 2:** Commit `docs: cursed deck checklist; version 1.6.0`.
