# Step 2a — StS1 card tooling, card set modes, Ironclad's StS1-only cards — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The host can pick a card set (StS2 only / StS1 only / Hybrid) in Custom Run. The Ironclad's 31 StS1 cards that have no StS2 namesake are playable. The 44 namesakes get a drafted duplicate table, which the owner reviews.

**Architecture:** Python tools (run in Podman) read the decompiled StS1 source, StS1's `cards.json` and the jar's art. They produce card data, a draft duplicate table, 250×190 portraits, and C# card skeletons whose `OnPlay` bodies are ported by hand. StS1 cards are `CardModel` subclasses (`Sts1*`) in our own `Sts1IroncladCardPool`, drawn with the vanilla Ironclad frame. Two `ModifierModel`s carry the mode in their class name, the same way step 1 carries the percent. Their hooks swap the card candidates of rewards, shops and transforms for a list built by a pure, tested composer.

**Tech Stack:** C# / .NET 9, Harmony, Godot 4.5.1 (PCK), xUnit; Python 3.12 + Pillow + pytest in a Podman image; CFR 0.152 decompile already in `~/Projects/sts-decomp/sts1/` (local only).

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3.3, §3.3b, §3.4, §4, §6, §7 step 2).

## Scope split (builder's call)

Spec step 2 is split in two:
- **2a (this plan):**
  - the tooling;
  - the card set modes;
  - the 31 Ironclad cards with no StS2 namesake (no duplicate question, so no owner review needed);
  - the draft duplicate table for the 44 namesakes.
- **2b (next plan, after the owner reviewed the table):**
  - porting the namesakes classified `same-sts1` or `duplicate`;
  - the hybrid version-swap UI.

Until 2b, every namesake plays as its vanilla StS2 card, in StS1-only mode too.

Facts found during recon that shrink step 2:
- The Ironclad's StS1 starter deck is identical to StS2's: 5 Strike (6, +3), 4 Defend (5, +3), Bash (8 dmg, 2 Vulnerable, +2/+1). So StS1-only keeps the vanilla starter.
- For the same reason, Archaic Tooth and Amalgamator work unchanged.
- The hybrid starting-card choice has no Ironclad card to offer. The mechanism moves to step 3, where Silent and Defect may need it.

## Global Constraints

- Target StS2 v0.107.1; mod id `SlaySpoon`; all docs/comments in English; mod text in `eng` and `fra` only.
- StS1 card classes are prefixed `Sts1` (ModelDb id `STS1_*`). **Never rename one after release:** saves store cards by id.
- StS1 card art and text are redistributed (spec §3.9). Decompiled code (Java or C#) never enters the repo, not even as comments.
- Every option lives in Custom Run. With none of our card-set modifiers ticked, and in Standard/Daily runs, every card pool is exactly vanilla.
- Card candidate lists must be built in a deterministic order: clients roll the same RNG on them.
- Characters without StS1 cards yet (Silent, Defect in 2a; Regent, Necrobinder forever) keep their vanilla pool in every mode. Colorless stays vanilla until step 3.
- Random in-combat generation by *vanilla* StS2 cards (e.g. Infernal Blade) keeps drawing from vanilla pools. This is a known gap: add it to `docs/backlog.md`.
- Podman for every tool (Bazzite is immutable). Heavy jobs run under `nice -n 19`.
- No `make up` / `make mp` while `pgrep -x SlayTheSpire2` finds the game. No in-game testing in this step (owner's call). In-game checks go to `docs/testing/step-2a-checklist.md` for later.
- Work on branch `step-2`. Commit per task. No push.

## Review Focus

1. **Co-op client and host compute the same candidate list:** the composer output order depends only on input order. Test `Compose_IsDeterministic` (Task 5).
2. **Mode unticked / Standard run:** vanilla pools. Test `Compose_Sts2Only_ReturnsVanillaUnchanged` (Task 5); the hooks live only on our modifiers (Task 6).
3. **Silent / Defect / Regent / Necrobinder in StS1-only:** their pool must not become empty. Test `Compose_Sts1Only_NoSts1Cards_KeepsVanilla` (Task 5).
4. **Card shows a raw key or a missing portrait:** every `Sts1*` card class has eng+fra title/description and a PNG. Test `Sts1Assets_AreComplete` (Task 4).
5. **Save and resume with StS1 cards in the deck:** ids must be stable. Test `ModelIds_FromClassName` against the game's slug rule (Task 4), plus a startup self-check comparing it to `ModelDb.GetId`.

---

### Task 1: Tools image and StS1 card extraction

**Files:**
- Create: `tools/sts1/Containerfile` (`python:3.12-slim`, `pip install pillow==11.0.0 pytest==8.3.3`)
- Create: `tools/sts1/extract.py`, `tools/sts1/tests/test_extract.py`, `tools/sts1/tests/fixtures/` (hand-written Java-shaped snippets, **not** copied from the decompile)
- Create: `data/sts1/ironclad.json`
- Modify: `Makefile` (targets `tools-image`, `sts1-data`; `test` also runs pytest), `.gitignore` (`__pycache__/`)

**Interfaces:**
- Produces: `python tools/sts1/extract.py --decomp <dir> --color red --out data/sts1/ironclad.json`. It writes a JSON list, sorted by `class`, of:
  ```json
  {"class": "HeavyBlade", "id": "Heavy Blade", "snake": "heavy_blade",
   "cost": 2, "cost_upgraded": null, "type": "attack", "rarity": "common", "target": "enemy",
   "damage": 14, "block": null, "magic": 3,
   "upgrade": {"damage": null, "block": null, "magic": 2},
   "keywords": [], "keywords_upgraded": [],
   "portrait": "red/attack/heavy_blade",
   "text": {"eng": {"name": "...", "description": "...", "upgrade_description": null},
            "fra": {"name": "...", "description": "...", "upgrade_description": null}}}
  ```
  - `cost`: -1 means X, -2 means unplayable.
  - `keywords`: any of `exhaust`, `ethereal`, `innate`, `retain`, `unplayable`. They come from `this.exhaust = true`, `isEthereal`, `isInnate`, `selfRetain`, or cost -2.
  - `keywords_upgraded`: the keyword set after `upgrade()`.
- Produces Make targets:
  - `make tools-image` builds `localhost/slayspoon-tools`.
  - `make sts1-data` runs extract, match (Task 2), art (Task 3) and codegen (Task 4) in that image. It mounts `$(STS1_DECOMP)` (default `~/Projects/sts-decomp/sts1`), the StS1 jar and the StS2 install read-only.

- [ ] **Step 1:** In `test_extract.py`, write tests on fixtures:
  - `test_parses_constructor`: cost 2, attack, common, enemy;
  - `test_parses_base_values_and_upgrade_deltas`: `upgradeDamage(4)` → `upgrade.damage == 4`; `upgradeMagicNumber(1)`; `upgradeBaseCost(1)` → `cost_upgraded == 1`;
  - `test_keywords_and_upgrade_removal`: `this.exhaust = true` in the constructor and `this.exhaust = false` in `upgrade()` → `keywords == ["exhaust"]`, `keywords_upgraded == []`;
  - `test_x_cost_and_unplayable`;
  - `test_text_joined_from_both_languages`.
- [ ] **Step 2:** Run `make tools-image && podman run … pytest tools/sts1/tests`. Expected: FAIL (module missing).
- [ ] **Step 3:** Implement `extract.py` with regexes over the constructor `super(...)` call, `this.base*` assignments and the `upgrade()` body. Text is keyed by the class's `ID` constant in `localization/{eng,fra}/cards.json`. Unknown constructs must not crash: they go into a `"warnings"` list on the card.
- [ ] **Step 4:** Run pytest. Expected: PASS. Then run `make sts1-data` (extract only for now). Expected: 75 entries, and a warnings count printed. Spot-check Bash: 2, 8, magic 2, +2/+1.
- [ ] **Step 5:** Commit `feat: StS1 card data extraction tool and Ironclad data`.

### Task 2: Draft duplicate table and owner review page

**Files:**
- Create: `tools/sts1/dump_sts2_loc.gd` (Godot headless: `load_resource_pack` on `SlayTheSpire2.pck`, copies `res://localization/{eng,fra}/cards.json` to an output dir; run it on the host with the cached Godot 4.5.1 like `tools/build-pck.sh`; check the in-pack path first with a `DirAccess` listing)
- Create: `tools/sts1/match.py`, `tools/sts1/tests/test_match.py`
- Create: `data/duplicates/ironclad.json`, `docs/review/ironclad-duplicates.md`, `src/CardSets/Sts1Matches.g.cs`

**Interfaces:**
- Consumes: `data/sts1/ironclad.json` (Task 1).
- Produces: `data/duplicates/ironclad.json`, a list of entries:
  ```json
  {"sts1": "HeavyBlade", "sts2": null, "relation": "unique", "draft": true, "note": ""}
  ```
  - Normalized name match (lowercase letters only, `_Red` dropped; `Strike_Red`→`StrikeIronclad`, `Defend_Red`→`DefendIronclad`).
  - `relation` is one of:
    - `unique`;
    - `same-sts2`: same effect, StS2 weaker or equal, the vanilla card serves every mode;
    - `same-sts1`: same effect, StS1 weaker, `Sts1*` replaces vanilla in StS1-only and Hybrid;
    - `duplicate`.
  - Re-running `match.py` keeps hand-edited `relation`, `note` and `draft` values.
- Produces: `src/CardSets/Sts1Matches.g.cs`:
  ```csharp
  // Generated by tools/sts1/match.py — do not edit.
  public static class Sts1Matches { public static readonly string[] Ironclad = { "ANGER", … }; }
  ```
  It lists the StS2 ids that have a StS1 namesake, in sorted order.

- [ ] **Step 1:** Write tests:
  - `test_name_normalization` (`ThunderClap`↔`Thunderclap`, `Strike_Red`↔`StrikeIronclad`);
  - `test_identical_card_drafts_same_sts2` (Bash fixture pair);
  - `test_numbers_only_weaker_sts1_drafts_same_sts1`;
  - `test_keyword_difference_drafts_duplicate`;
  - `test_mixed_numbers_is_flagged` (note contains `?`);
  - `test_rerun_keeps_hand_edits`.
- [ ] **Step 2:** Run pytest. Expected: FAIL.
- [ ] **Step 3:** Implement `match.py`. On the StS2 side it uses regexes over the decompiled `MegaCrit.Sts2.Core.Models.Cards/<Class>.cs`:
  - `base(cost, CardType.X, CardRarity.Y, TargetType.Z)`;
  - `new XVar(nm`;
  - `UpgradeValueBy(nm)`;
  - `CanonicalKeywords`;
  - keyword changes in `OnUpgrade`.

  English text comes from the dumped `cards.json`. A rarity difference alone never makes a duplicate.
- [ ] **Step 4:** Run pytest (PASS). Run `make sts1-data`. Expected: 31 `unique` + 44 drafted namesakes; Strike, Defend and Bash come out `same-sts2`.
- [ ] **Step 5:** Read every one of the 44 drafted rows against both texts and set `relation` and a one-line `note` by hand. Leave `draft: true`, which becomes `false` only when the owner approves.
- [ ] **Step 6:** Generate `docs/review/ironclad-duplicates.md`, one table per proposed relation. Columns: StS1 card (cost, text, upgrade), StS2 card (same), proposed relation, note. Head it with the four relation definitions from spec §3.4.
- [ ] **Step 7:** Commit `feat: draft Ironclad duplicate table for owner review`.

### Task 3: Card portraits

**Files:**
- Create: `tools/sts1/art.py`, `tools/sts1/tests/test_art.py`
- Create: `godot/SlaySpoon/images/cards/ironclad/sts1_<snake>.png` (one per card whose relation is not `same-sts2`)

**Interfaces:**
- Consumes: `data/sts1/ironclad.json` (`portrait`, `snake`) and `data/duplicates/ironclad.json`.
- Produces: one 250×190 RGBA PNG per StS1 class card, at `res://SlaySpoon/images/cards/ironclad/sts1_<snake>.png`. 250×190 is the vanilla card atlas sprite size, measured with a Godot probe on `SlayTheSpire2.pck`. StS1's `images/1024Portraits/<portrait>.png` is 500×380, the same aspect.

- [ ] **Step 1:** Test `test_resizes_to_card_atlas_size` (synthetic 500×380 PNG → 250×190) and `test_skips_same_sts2`.
- [ ] **Step 2:** Run pytest. Expected: FAIL.
- [ ] **Step 3:** Implement with `zipfile` + Pillow `LANCZOS`.
- [ ] **Step 4:** Run pytest (PASS). Run `make sts1-data`, then `make build`. Expected: the PCK holds an imported texture for each PNG (`unzip -l` doesn't work on PCK; `grep -a -c 'sts1_heavy_blade' build/mods/SlaySpoon/SlaySpoon.pck` ≥ 1).
- [ ] **Step 5:** Commit `feat: StS1 Ironclad card portraits`.

### Task 4: Card infrastructure and code generation

**Files:**
- Create: `src/Core/ModelIds.cs`, `src/Core/Sts1Relation.cs`, `tests/ModelIdsTests.cs`, `tests/Sts1AssetsTests.cs`
- Create: `src/Cards/Sts1Card.cs`, `src/Cards/Sts1IroncladCard.cs`, `src/Cards/Pools/Sts1IroncladCardPool.cs`
- Create: `tools/sts1/codegen.py`, `tools/sts1/tests/test_codegen.py`
- Create: `src/Cards/Sts1/Ironclad/Sts1<Class>.cs` × 31 (skeletons), `godot/SlaySpoon/localization/{eng,fra}/cards.json`
- Modify: `src/ModEntry.cs` (self-check call), `tests/SlaySpoon.Tests.csproj` (link `src/Core/ModelIds.cs`, `src/Core/Sts1Relation.cs`)

**Interfaces:**
- Produces:
  - `SlaySpoon.Core.ModelIds.FromClassName(string className) -> string`. It mirrors `StringHelper.Slugify`; copy the game's `CamelCaseRegex` pattern exactly from the decompile, where it is a `[GeneratedRegex]`.
  - `public abstract class Sts1Card : CardModel`, with:
    - constructor `(int cost, CardType type, CardRarity rarity, TargetType target)`;
    - `public virtual Sts1Relation Relation => Sts1Relation.Unique`;
    - `public virtual Type? Sts2Counterpart => null`;
    - `PortraitPath` → `res://SlaySpoon/images/cards/{PortraitFolder}/{Id.Entry.ToLowerInvariant()}.png`;
    - `protected abstract string PortraitFolder { get; }`.
  - `public abstract class Sts1IroncladCard : Sts1Card`, which sets:
    - `Pool` → `ModelDb.CardPool<Sts1IroncladCardPool>()`;
    - `VisualCardPool` → `ModelDb.CardPool<IroncladCardPool>()`;
    - `PortraitFolder` → `"ironclad"`.
  - `public sealed class Sts1IroncladCardPool : CardPoolModel`:
    - it copies `IroncladCardPool`'s visual members;
    - `Title` is `"sts1_ironclad"`;
    - `GenerateAllCards()` returns every non-abstract `Sts1IroncladCard` subtype in the assembly, ordered by type name, via `ModelDb.GetById<CardModel>(ModelDb.GetId(t))`.
  - The pool is **not** added to `ModelDb.AllCardPools`. `Sts1Card.Pool` is overridden, so nothing needs to look it up there.
  - `public enum Sts1Relation { Unique, SameKeepSts1, Duplicate }` in `src/Core/Sts1Relation.cs` (pure, so the composer of Task 5 can use it).
- Produces: `python tools/sts1/codegen.py`. For each `unique` entry (2b extends it to the others):
  - it writes `src/Cards/Sts1/Ironclad/Sts1<Class>.cs` **only if the file does not exist**;
  - it adds missing `STS1_<SNAKE>.title` / `.description` keys to both `cards.json` files, never overwriting.
  - Each skeleton contains:
    - constructor args;
    - `CanonicalVars`: `DamageVar`/`BlockVar` with `ValueProp.Move`, and `new DynamicVar("Magic", n)` for the magic number;
    - `CanonicalKeywords`;
    - an `OnUpgrade` with `UpgradeValueBy` calls (cost upgrade: copy the call vanilla `Barricade.OnUpgrade` uses; keyword changes: `RemoveKeyword`/`AddKeyword`);
    - `OnPlay` with a body of `throw new NotImplementedException("STS1 port");`.
  - Text conversion:
    - `!D!`→`{Damage:diff()}`, `!B!`→`{Block:diff()}`, `!M!`→`{Magic:diff()}`;
    - `NL`→`\n`;
    - each run of `[R]` → `{Energy:energyIcons()}` with an `EnergyVar` of that count;
    - known keywords (Block, Vulnerable, Weak, Strength, Exhaust, Ethereal, Innate, Retain, Unplayable, Wound, Burn, Dazed, Dexterity and their `fra` forms as StS2 writes them) → `[gold]…[/gold]`;
    - if `upgrade_description` differs, the whole text becomes `{IfUpgraded:show:<upgraded>|<base>}`.

- [ ] **Step 1:** Write tests:
  - `ModelIds_FromClassName`: `Sts1HeavyBlade`→`STS1_HEAVY_BLADE`, `SpoonMonsterHp150`→`SPOON_MONSTER_HP150`, `SpoonCardSetSts1Only`→`SPOON_CARD_SET_STS1_ONLY`; the last two must match what step 1 logged in game.
  - `Sts1Assets_AreComplete`: for each `src/Cards/Sts1/**/Sts1*.cs` file, both `cards.json` files have non-empty `.title` and `.description` and the PNG exists.
  - `NoUnportedCards`: no file under `src/Cards/Sts1/` contains `NotImplementedException`. Mark it `[Fact(Skip = …)]` until Task 10 removes the skip.
  - pytest:
    - `test_text_conversion` (Bloodletting: `Lose 3 HP. NL Gain [R] [R].` → `Lose 3 HP.\nGain {Energy:energyIcons()}.`, Energy 2);
    - `test_upgrade_text_switch`;
    - `test_never_overwrites_existing_file`.
- [ ] **Step 2:** Run `make test`. Expected: FAIL.
- [ ] **Step 3:** Implement `ModelIds`, `Sts1Card`, `Sts1IroncladCard`, `Sts1IroncladCardPool` and `codegen.py`. Run `make sts1-data` to generate the 31 skeletons and their loc keys. Also add to `ModEntry.Initialize` a self-check that logs an error for any `Sts1Card` type where `ModelIds.FromClassName(t.Name) != ModelDb.GetId(t).Entry`.
- [ ] **Step 4:** Run `make test` and `make build`. Expected: PASS (except the skipped gate) and a clean build.
- [ ] **Step 5:** Commit `feat: StS1 card base classes, Ironclad pool and card skeletons`.

### Task 5: Card set composer (pure logic)

**Files:**
- Create: `src/Core/CardSetMode.cs`, `src/Core/CardSetComposer.cs`, `tests/CardSetComposerTests.cs`
- Modify: `tests/SlaySpoon.Tests.csproj` (link both)

**Interfaces:**
- Consumes: `Sts1Relation` (Task 4).
- Produces:
  ```csharp
  public enum CardSetMode { Sts2Only, Sts1Only, Hybrid }
  public sealed record Sts1Entry(string Sts1Id, string? Sts2Id, Sts1Relation Relation);
  public static class CardSetComposer
  {
      public static IReadOnlyList<string> Compose(CardSetMode mode, IReadOnlyList<string> vanilla,
          IReadOnlyCollection<string> matchedSts2, IReadOnlyList<Sts1Entry> sts1);
  }
  ```
  Rules:
  - **Sts2Only:** `vanilla`.
  - **Sts1Only:**
    - if `sts1` is empty, `vanilla`;
    - otherwise the vanilla ids that are in `matchedSts2` and not claimed by any entry's `Sts2Id`, then all `sts1` ids.
  - **Hybrid:** the vanilla ids not claimed by a `SameKeepSts1` entry, then all `sts1` ids. Until 2b's swap UI, a duplicate shows up as two cards.
  - Order: vanilla order first, then `sts1` order. No set iteration in the output.

- [ ] **Step 1:** Write tests:
  - `Compose_Sts2Only_ReturnsVanillaUnchanged`;
  - `Compose_Sts1Only_KeepsOnlyMatchedVanillaPlusSts1` (vanilla `[AGGRESSION, ANGER, BASH]`, matched `{ANGER, BASH}`, sts1 `[STS1_CLEAVE]` → `[ANGER, BASH, STS1_CLEAVE]`);
  - `Compose_Sts1Only_ClaimedVanillaDropped` (`Duplicate` claiming `ANGER` → no `ANGER`);
  - `Compose_Sts1Only_NoSts1Cards_KeepsVanilla`;
  - `Compose_Hybrid_DropsOnlySameKeepSts1Claims`;
  - `Compose_IsDeterministic` (same inputs in two calls → `SequenceEqual`, and `matchedSts2` passed as a `HashSet` built in two different insertion orders gives the same output).
- [ ] **Step 2:** Run `make test`. Expected: FAIL.
- [ ] **Step 3:** Implement `Compose`.
- [ ] **Step 4:** Run `make test`. Expected: PASS.
- [ ] **Step 5:** Commit `feat: card set composer`.

### Task 6: Card set modifiers, Custom Run rows and the hooks

**Files:**
- Create: `src/Config/CardSetModifiers.cs`, `src/CardSets/CardSetPools.cs`, `src/CardSets/TransformOptionsPatch.cs`, `src/CustomRunUi/MutuallyExclusivePatch.cs`
- Modify:
  - `src/CustomRunUi/ModifierListPatch.cs`: add both canonical card-set modifiers, mutable;
  - `src/TopBar/TopBarPercentPatch.cs`: card-set modifiers show a short label from loc key `<ID>.short` instead of the missing icon;
  - `godot/SlaySpoon/localization/{eng,fra}/modifiers.json`.

**Interfaces:**
- Consumes:
  - `CardSetComposer`, `CardSetMode`, `Sts1Entry` (Task 5);
  - `Sts1Matches.Ironclad` (Task 2);
  - `Sts1IroncladCardPool`, `Sts1Card.Relation` / `Sts2Counterpart` (Task 4).
- Produces:
  - `public abstract class SpoonCardSet : ModifierModel { public abstract CardSetMode Mode { get; } }`;
  - sealed `SpoonCardSetSts1Only` and `SpoonCardSetHybrid`;
  - `CardSetPools.ModeOf(IRunState) -> CardSetMode` (`Sts2Only` when neither is present);
  - `CardSetPools.CardsFor(Player player, CardSetMode mode) -> IReadOnlyList<CardModel>`:
    - it maps `player.Character.CardPool` to its StS1 pool (only `IroncladCardPool` → `Sts1IroncladCardPool` in 2a);
    - it runs the composer on ids taken from `GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)` of both pools;
    - it maps the ids back to the canonical models.
- Hooks on `SpoonCardSet`:
  - `ModifyCardRewardCreationOptions`:
    - unchanged when `NoCardPoolModifications` is set, `CustomCardPool` is non-null, or `CardPools` lacks the character pool;
    - otherwise `options.WithCustomPool(CardsFor(...)` + the cards of the other pools in `options`, filtered by `CardPoolFilter` + `)` and the same `RarityOdds`.
  - `ModifyMerchantCardPool`: when every option belongs to the character pool (same test as vanilla `CharacterCards`), return `CardsFor(...)`.
  - `TransformOptionsPatch`: postfix on `CardFactory.GetDefaultTransformationOptions(CardModel, bool)`. When the run has a card-set mode other than `Sts2Only` and the original's `Pool` is the character pool or its StS1 pool, return `CardsFor(original.Owner, mode)` filtered like vanilla (`FilterForCombat` when `isInCombat`).
- `MutuallyExclusivePatch`: postfix on the `ModelDb.MutuallyExclusiveModifiers` getter that appends the set `{SpoonCardSetSts1Only, SpoonCardSetHybrid}`.
- Loc (`modifiers.json`):

  | Key | eng | fra |
  |-----|-----|-----|
  | `SPOON_CARD_SET_STS1_ONLY.title` | Slay the Spire 1 Cards | Cartes de Slay the Spire 1 |
  | `SPOON_CARD_SET_HYBRID.title` | Cards From Both Games | Cartes des deux jeux |
  | `.short` | StS1 / 1+2 | StS1 / 1+2 |

  The descriptions say which characters have StS1 cards so far (the Ironclad) and that the others keep their own.

- [ ] **Step 1:** Implement the modifiers, `CardSetPools`, the hooks and the patches.
- [ ] **Step 2:** Run `make build` and `make test`. Expected: both clean. Startup can't be checked in game in this step; instead confirm by grep that no `ModelDb.GoodModifiers` / `BadModifiers` reference was added.
- [ ] **Step 3:** Add to `docs/backlog.md`: vanilla StS2 cards that generate random cards in combat ignore the card set mode.
- [ ] **Step 4:** Commit `feat: card set modes in Custom Run`.

### Tasks 7–10: Port the 31 Ironclad cards

The same steps apply to each batch. For each card in the batch:

- [ ] **Step 1:** Read the StS1 card in `~/Projects/sts-decomp/sts1/com/megacrit/cardcrawl/cards/red/<Class>.java`, and the action it uses under `actions/` when it isn't a plain damage/block/power.
- [ ] **Step 2:** Find the closest vanilla StS2 card or power by grepping `~/Projects/sts-decomp/sts2` for the command (`DamageCmd`, `PowerCmd.Apply`, `CardPileCmd`, `CardSelectCmd`, `CreatureCmd`), and copy its pattern, including `WithHitFx`.
- [ ] **Step 3:** Fill in `OnPlay`. Where the game has a typed var (`PowerVar<T>`, `CardsVar`, `EnergyVar`, `HpLossVar`), replace `DynamicVar("Magic")` with it and rename it in **both** `cards.json` files. Add `ExtraHoverTips` for the powers and statuses the card mentions.
- [ ] **Step 4:** Where a card needs a power StS2 lacks, add `src/Cards/Sts1/Powers/Sts1<Name>Power.cs` with its loc in `godot/SlaySpoon/localization/{eng,fra}/powers.json`. Copy the key format from the game's `VULNERABLE_POWER.*` keys (grep `SlayTheSpire2.pck`).
- [ ] **Step 5:** Run `make build`. It must compile without new warnings.
- [ ] **Step 6:** Add one checklist line per card to `docs/testing/step-2a-checklist.md`: what to play and what should happen, base and upgraded.
- [ ] **Step 7:** Commit `feat: port StS1 Ironclad <batch name>`.

**Task 7 — plain attacks:** Cleave, Clothesline, HeavyBlade (Strength counts ×3, ×5 upgraded), Pummel, SeverSoul, WildStrike (Wound into draw pile), RecklessCharge (Dazed into draw pile), Carnage (Ethereal), Immolate (Burn into discard), Dropkick (energy + draw if the target is Vulnerable).

**Task 8 — special attacks:**
- BloodForBlood: cost −1 per time the owner lost HP this combat, including while the card sits in another pile.
- Reaper: heal the total unblocked damage dealt.
- SearingBlow:
  - `MaxUpgradeLevel` is `int.MaxValue`;
  - damage is `12 + Σ(4+i)` over the upgrade levels: 12, 16, 21, 27;
  - pure helper `SlaySpoon.Core.Sts1Math.SearingBlowDamage(int level)` in Core, with test `SearingBlowDamage_MatchesSts1` ([0→12, 1→16, 2→21, 3→27]);
  - the title shows `+N`; check how vanilla shows the upgrade level.

**Task 9 — skills:** Disarm, Flex (vanilla `TemporaryStrengthPower`), GhostlyArmor, Intimidate, LimitBreak, PowerThrough, SeeingRed, Sentinel (energy when exhausted), SpotWeakness (only if the target intends to attack), Warcry, Exhume, DoubleTap.

**Task 10 — powers:** Berserk, Brutality, Combust, Evolve, FireBreathing, Metallicize. Use a vanilla power when its behaviour matches exactly. Plating decays each turn, so it does **not** match Metallicize. Afterwards, remove the skip from `NoUnportedCards`; `make test` must pass with it active.

### Task 11: Finish step 2a

**Files:**
- Modify:
  - `README.md` (Features: card set modes, Ironclad StS1 cards; Planned list);
  - `mod/SlaySpoon.json` and `src/ModEntry.cs` (version `0.2.0`);
  - `CLAUDE.md` (Where we are, code map, tools);
  - `docs/testing/step-2a-checklist.md` (header and mode checks).

- [ ] **Step 1:** Add mode-level lines to the checklist:
  - Custom Run shows both card-set rows, and ticking one unticks the other;
  - the client sees the host's tick;
  - StS1-only Ironclad rewards show only StS1 namesakes and `Sts1*` cards;
  - Hybrid shows both;
  - Silent and Necrobinder rewards are vanilla in StS1-only;
  - shop and transform follow the mode;
  - save, quit and resume with a StS1 card in the deck;
  - Standard run rewards are vanilla.
- [ ] **Step 2:** Update README, CLAUDE.md, version.
- [ ] **Step 3:** Run `make test` and `make build`. Expected: all green.
- [ ] **Step 4:** Commit `docs: step 2a wrap-up`. Report in French to the owner:
  - what is ready to test in game;
  - the review page `docs/review/ironclad-duplicates.md`, with the rows the draft flagged `?`.
