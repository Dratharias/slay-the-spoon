# Slay the Spoon — Design

Date: 2026-10-28
Status: draft, awaiting approval
Target game: Slay the Spire 2 Early Access, v0.107.1 (commit 59260271)

## 1. Intent

Slay the Spoon is a Slay the Spire 2 mod that brings the vanilla Slay the Spire 1 cards (all
characters except Watcher) into StS2,
so a group of friends can play co-op (or solo) runs with strange builds that mix both games. The point
is fun, not balance or perfect fidelity: weird cross-game combos are the success case.

The lobby host (or the solo player) configures the run before it starts: which card set is in play,
how strong monsters are, and a few optional rules (merchant loans, Sealed Deck).

The mod is published on the Steam Workshop and in a public GitHub repository.

## 2. Goals, non-goals, anti-goals

**Goals**
- The 265 vanilla StS1 cards of Ironclad (75), Silent (75), Defect (76) and Colorless (39) playable in
  StS2. Watcher (77 cards) is out of scope.
- Per-run card set: StS1 only, StS2 only, or hybrid (both).
- In hybrid mode, cards that exist in both games with a different effect let the player pick which
  version they take.
- Monster HP and damage multipliers.
- Optional merchant loans between players, repaid automatically.
- Works in multiplayer (StS2 co-op) and solo.

**Non-goals (for now)**
- StS1 relics, potions, events, monsters, bosses. Cards only.
- Watcher: no cards, no character, no stances/Mantra/Scry/Divinity.
- Regent and Necrobinder: untouched. They have no StS1 counterpart and keep their StS2 cards in every
  mode (colorless cards still follow the mode, see §3.3).
- Balancing. Strange and overpowered builds are intended.
- Anything outside StS2's Custom Run mode. A normal (Standard) run is untouched vanilla StS2, except
  for the Spire Souvenir relic (amended 2026-10-29, see `2026-10-29-spire-souvenir-relic-design.md`).

**Anti-goals — failure even if it technically works**
- Multiplayer desyncs.
- Crashes or save corruption that lose a run.
- A mod that stays broken for weeks after each StS2 patch. Stability beats completeness: ship small,
  working increments rather than a big half-working port.

## 3. Decisions made by the owner

These were decided in conversation and are not open for the builder to change.

1. **Custom Run only.** Every Slay the Spoon option lives in StS2's Custom Run screen (solo and
   multiplayer `host_custom`). Standard and Daily runs are not modified. Amended 2026-10-29: the Spire
   Souvenir relic can be found in Standard runs (see `2026-10-29-spire-souvenir-relic-design.md`).
2. **Who configures:** the lobby host in multiplayer, the player in solo.
3. **Card set mode:** `StS1 only` / `StS2 only` / `Hybrid`, one choice per run. Colorless cards follow
   the mode too (StS1 colorless in StS1 only, both in Hybrid), for every character including Regent
   and Necrobinder.
3b. **Starting decks:**
   - StS1 only: the StS2 starting deck is replaced by the character's StS1 starting deck (e.g. Defect:
     4 Strike, 4 Defend, Zap, Dualcast).
   - Hybrid: the player chooses the version of each starting card.
   - StS2 only: vanilla.
   - Exception: Archaic Tooth (Orobas's Transcendence) and the Amalgamator event always turn the card
     into the **StS2 ultimate version**, automatically, whichever version the player holds:
     Bash → Break, Neutralize → Suppress, Dualcast → Quadcast; two Strikes → Ultimate Strike, two
     Defends → Ultimate Defend. StS1 starters must therefore be recognised by
     `ArchaicTooth.TranscendenceUpgrades` and by Amalgamator's `CardTag.Strike` / `CardTag.Defend`
     checks.
   - **Colorless event-card amendment (2026-10-29):** StS2 event-only cards such as Apotheosis
     remain outside ordinary StS1-only and Hybrid colorless pools. Apotheosis, Apparition,
     Enlightenment and Metamorphosis must be obtained through events, not ordinary rewards,
     merchant stock, transforms or random combat generation. StS1 source rarity does not
     override this rule. Developer-console access remains available for testing.
4. **Duplicates (cards that exist in both games):**
   - Same effect, only numbers differ (one version is a nerf of the other) → treat as **one card**,
     keep **the weaker version**. No choice is offered.
   - Different effect or keyword (example: Blade Dance does not Exhaust in StS1 but Exhausts in StS2)
     → a real **duplicate**. In Hybrid mode, **the player picks the version at the moment of choosing
     the card**. Only one copy of the card is offered; the player switches between versions.
   - In StS1-only or StS2-only mode there is no duplicate question: only that game's version exists.
5. **Monster multipliers:** separate HP and damage multipliers, set by the host.
6. **Merchant loans (optional, toggled by the host):** at the merchant, a player short of gold for a
   relic or a card can ask other players for financing, with or without interest. Several lenders can
   **pool their gold** to fund the same purchase. Repayment is **automatic**: gold the borrower gains
   afterwards goes to repaying the debt first.
   Owner's details (2026-10-29): a lender sets the amount with +1 / +5 / +10 buttons and may ask for
   interest up to 300%, set with +1 / +5 / +10 buttons too; anyone may give the gold instead (a gift is
   never repaid); the borrower picks which offers fund the purchase; a loan is repaid in equal shares
   between its lenders.
7. **Sealed Deck:** use StS2's existing `SealedDeck` modifier as is (30 cards, keep 10, replaces the
   starting deck). Do not reinvent it. It must respect the chosen card set mode.
8. **Scope now:** cards only.
9. **Assets:** StS1 card art and card text are redistributed inside the mod. Mega Crit's Content Policy
   (https://www.megacrit.com/content-policy/) allows mods, forbids monetization beyond donations and
   forbids implying official affiliation; it says nothing about asset reuse in mods. The owner chose to
   redistribute.
10. **Public repo + Workshop.** README must state: unofficial, not affiliated with Mega Crit; StS1/StS2
    assets belong to Mega Crit; the code is **vibe coded** (written by an AI, Claude, directed by the
    author); no paid content, donations only if any.
11. **Roles:** the owner does not write C#. Claude writes all code; the owner makes architecture
    decisions and play-tests in game with friends.

## 4. Builder's calls (decided by Claude, owner may override)

- Repo: `Dratharias/slay-the-spoon` on GitHub, local clone `~/Projects/github/slay-the-spoon`. License
  MIT for our code.
- Decompiled game code never enters the repo. It lives in `~/Projects/sts-decomp/` (local only).
  `sts2/` already exists (ilspycmd 9.1 output, 3424 files). StS1 will be decompiled to `sts1/`.
- No BaseLib dependency: StS2's own modding API covers what we need (see §5). Revisit only if a
  concrete need appears.
- Monster multipliers: 0.25× to 5× in 0.25 steps, default 1×, applied to every enemy (normal, elite,
  boss, summoned minions). Rounding: round half up, minimum 1.
- Loans: the borrower sends a request (item, missing amount). Other players answer with
  contributions: an amount and an interest rate in percent (0% = no interest) on that share. Several
  lenders may pool contributions to fund one purchase (owner's decision); once the contributions
  cover the missing amount, the borrower accepts the offers they pick, the gold is transferred and the
  item can be bought. A contribution may be a gift (never repaid). Interest is simple and fixed when the
  loan is accepted: owed = amount + amount × rate / 100, rounded half up; rates go from 0% to 300%.
  Repayment takes 100% of gold gained until all debts are cleared; within one loan, each payment is
  split in equal shares between the lenders still owed (owner's decision; a lender paid off drops out
  and an indivisible remainder goes to the earliest lenders); separate loans are repaid oldest first.
  Repayments reaching a lender are not "gold gained" for that lender. Debts are cancelled if the
  borrower dies or the run ends.
- Hybrid starting deck: start from the StS2 starting deck composition; at run start, each starting
  card that has a different StS1 version is offered as a version choice. Starting cards classified
  `same` need no choice.
- Hybrid duplicate choice appears on every **choose-a-card** screen (card rewards, merchant, choose-1
  effects like Discovery, Sealed Deck grid). Where a card is generated **without** a choice (e.g. random
  card added to hand), the version is picked at random with the run's RNG.
- Localization: the mod ships in English and French only (owner decision, 2026-10-28). Every text
  the mod adds (UI, modifiers, cards) exists in both `eng` and `fra`; StS1 card text comes from the
  jar's `localization/eng/cards.json` and `localization/fra/cards.json`. Other game languages fall
  back to English.

## 5. What the game already gives us (recon on v0.107.1)

Paths are in `~/Projects/sts-decomp/sts2/`.

**Mod loading** (`MegaCrit.Sts2.Core.Modding/`)
- Mods live in `<StS2>/mods/<ModId>/` with `<ModId>.json` (manifest: `id`, `name`, `author`,
  `description`, `version`, `has_pck`, `has_dll`, `dependencies`, `affects_gameplay`,
  `min_game_version`), `<ModId>.dll`, `<ModId>.pck`.
- Entry point: `[ModInitializer(nameof(Initialize))]` on a static class.
- `ModHelper.AddModelToPool<TPool, TModel>()` adds models to pools (must run before pools freeze).
- `ModelDb`, network message types (`MessageTypes`) and network actions (`ActionTypes`) all pick up
  subclasses from mod assemblies by reflection (`ReflectionHelper.GetSubtypesInMods<T>()`).
- The lobby already refuses players whose gameplay-affecting mods differ (`NETWORK_ERROR.MOD_MISMATCH`).
- Harmony (`0Harmony.dll`) ships with the game.
- Reference tutorial: https://github.com/fresh-milkshake/Modding-Tutorial (net9.0, Godot 4.5.1 Mono,
  localization at `res://<ModId>/localization/<lang>/cards.json`, card art via `PortraitPath`).

**Modifiers = our configuration channel** (`MegaCrit.Sts2.Core.Models/ModifierModel.cs`)
- `ModifierModel` subclasses are run-lifetime models. `[SavedProperty]` fields are serialized
  (`SerializableModifier.Props`), saved with the run, and synced to clients by `StartRunLobby` via
  `LobbyModifiersChangedMessage`. Example with a parameter: `Modifiers/CharacterCards.cs`.
- Modifiers receive all hooks (`ShouldReceiveCombatHooks => true`).
- The Custom Run screen lists `ModelDb.GoodModifiers` / `BadModifiers` (hard-coded arrays, need a
  Harmony patch to add ours) and `MutuallyExclusiveModifiers`. UI is tick boxes only
  (`NRunModifierTickbox`): numeric settings need a custom control.

**Hooks we need** (virtuals on `AbstractModel`, dispatched by `MegaCrit.Sts2.Core.Hooks/Hook.cs`)
- Card set: `ModifyCardRewardCreationOptions` (+ `Late`), `ModifyMerchantCardPool`,
  `ModifyMerchantCardCreationResults`. `CharacterCards` shows the pattern
  (`options.WithCustomPool(...)`).
- Monster HP: `AfterCreatureAddedToCombat`. Monster damage: `ModifyDamageMultiplicative`.
- Loans: `ModifyGoldGained`, `AfterGoldGained`, `AfterItemPurchased`, `ModifyMerchantPrice`.

**Cards** (`MegaCrit.Sts2.Core.Models.Cards/`, 578 files)
- A card is a `CardModel` subclass: constructor `(cost, CardType, CardRarity, TargetType)`,
  `CanonicalVars` (`DamageVar`, `BlockVar`, `PowerVar<T>`…), `async OnPlay(...)` using commands
  (`DamageCmd`, `PowerCmd`, `CardPileCmd`, …), `OnUpgrade()`. See `Bash.cs`.
- Keywords available: Exhaust, Ethereal, Innate, Unplayable, Retain (+ StS2's Sly, Eternal).
- Orbs: Lightning, Frost, Dark, Plasma (+ Glass). Characters: Ironclad, Silent, Defect, Regent,
  Necrobinder. **No Watcher**, no stances, Mantra, Scry or Divinity.
- 165 of 342 StS1 card names (Watcher included in that count) also exist as StS2 card classes
  (normalized name match) and need the same-card / duplicate classification of §3.4.
- Starter transformations: `Relics/ArchaicTooth.cs` (`TranscendenceUpgrades` dictionary keyed by
  card id) and `Events/Amalgamator.cs` (counts `CardTag.Strike` / `CardTag.Defend`).
- `SealedDeck` (`Modifiers/SealedDeck.cs`) draws 30 cards via `CardFactory.CreateForReward` with
  `IsCardReward`; check that our card set modifier reaches it.

**StS1 source** (`~/.local/share/Steam/steamapps/common/SlayTheSpire/desktop-1.0.jar`)
- Card classes in `com/megacrit/cardcrawl/cards/{red,green,blue,purple,colorless}/`.
- Card text in `localization/<lang>/cards.json`; art in the jar's card atlases.
- Prior art for Watcher in StS2: https://github.com/lfyxhappy/STS2mods (not reviewed yet).

## 6. Architecture

```
SlaySpoon (C# mod, one assembly)
├── Config/        Modifiers carrying the run configuration ([SavedProperty])
│   ├── CardSetModifier         mode: Sts1Only | Sts2Only | Hybrid
│   ├── MonsterScalingModifier  hpMultiplier, damageMultiplier
│   └── MerchantLoansModifier   on/off
├── CustomRunUi/   Harmony patches adding our modifiers + a numeric control to NCustomRunScreen
├── Cards/Sts1/    One CardModel per StS1 card, class prefix Sts1 (ModelDb id STS1_*)
│   ├── Ironclad/ Silent/ Defect/ Colorless/
│   └── Pools: Sts1IroncladCardPool … (our own CardPoolModels, not attached to any character)
├── Duplicates/    Classification table + hybrid version-swap UI on card choice screens
├── StartingDecks/  StS1 starter decks, hybrid version choice, Archaic Tooth / Amalgamator compatibility
├── Loans/         Ledger, net messages/actions, merchant UI, gold-gain interception
└── godot/SlaySpoon/  localization/<lang>/*.json, images/cards/*.png (→ SlaySpoon.pck)

tools/ (not shipped)
├── sts1-extract   Reads decompiled StS1 cards + cards.json → card data (cost, type, rarity,
│                  target, values, upgrade deltas, keywords, text) as JSON
├── sts1-codegen   Card data → C# skeletons (values, keywords, upgrade); OnPlay bodies hand-ported
└── sts1-art       Extracts and resizes StS1 card art to StS2's portrait format
```

**Card set mode.** StS1 cards live in our own pools, so vanilla StS2 pools stay untouched.
`CardSetModifier` rewrites the candidate pool on every card-creation path (rewards, merchant,
Sealed Deck, random generation):
- StS2 only: vanilla behaviour.
- StS1 only: replace the character's pool with the matching StS1 pool (Ironclad, Silent, Defect;
  Regent and Necrobinder keep their StS2 pool); colorless → StS1 colorless for everyone.
- Hybrid: union of both, minus the "same card" StS1 entries; duplicates are one slot that resolves to
  the version the player picks.

**Duplicates.** A data table (checked into the repo, generated then hand-reviewed) maps each of the
165 name matches to `same` (keep the weaker version) or `duplicate`. The owner reviews this table
before cards are coded.

**Monster scaling.** On `AfterCreatureAddedToCombat` for enemy creatures, scale max and current HP once
(guard against re-scaling on reload/summon). `ModifyDamageMultiplicative` scales damage dealt by enemies.

**Loans.** A ledger (lender, borrower, principal, interest, remaining) stored in the modifier's saved
properties so it survives save/load and is identical on all clients. Requests/offers travel as mod
synced actions (`INetAction`, found in mods by the game); the gold transfer and every repayment are deterministic synced game actions, never
local-only state changes.

## 7. Delivery order

Each step ships as a usable Workshop update.

1. **Skeleton + config:** mod loads, Custom Run shows our modifiers, settings sync to clients, survive
   save/load. Includes **monster multipliers** (smallest real feature, proves the pipeline).
2. **Tooling + Ironclad (StS1):** extraction, codegen, art; 75 Ironclad cards; card set modes;
   StS1 starting deck and hybrid starting-card choice; Archaic Tooth / Amalgamator compatibility;
   duplicate table for Ironclad reviewed by the owner; hybrid version-swap UI.
3. **Silent, Defect, Colorless.**
4. **Sealed Deck check:** confirm the vanilla modifier respects the card set mode; fix if not.
5. **Merchant loans.**

## 8. Testing

- Pure logic (card data extraction, duplicate classification, loan ledger maths, multiplier rounding)
  gets automated tests.
- In-game behaviour is verified by the owner with a checklist per step (solo + 2-player co-op,
  save/quit/resume mid-run, host and client both checked).
- Every release states the StS2 version it was tested against (`min_game_version`).

## 9. Left to the builder

- Exact project layout, build scripts (Podman containers for .NET 9 SDK and Godot 4.5.1 Mono, since
  the dev machine is an immutable Bazzite system), Java decompiler choice for StS1.
- Visual design of the numeric control and the version-swap toggle, as long as they match StS2's UI.
- How each StS1 card effect maps onto StS2 commands.
- Workshop upload process.
