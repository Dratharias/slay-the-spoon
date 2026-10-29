# Spire Souvenir relic — Design

Date: 2026-10-29
Status: approved by the owner 2026-10-29
Amends: `2026-10-28-slay-the-spoon-design.md` (§2 non-goal "anything outside Custom Run", §3.1)

## 1. Intent

The owner wants Slay the Spire 1 cards to show up in normal runs too, as a lucky find rather than a
run setting. A character relic does that: whoever picks it up starts getting cards from the other game
on top of their own, for the rest of the run. In a Custom Run with "Slay the Spire 1 Cards" ticked, it
works the other way round and brings Slay the Spire 2 cards back. Either way the owner ends up in the
Hybrid experience ("Cards From Both Games") for that one player.

## 2. Goals, non-goals, anti-goals

**Goals**
- A new relic in the **Ironclad relic pool**, rarity **Common** (owner: "regular, not too rare, not
  always there").
- It is a **character relic for one player**: only its owner's card offers change. The other players in
  a co-op run keep their own card set.
- From the moment it is obtained, its owner's card pool behaves as in Hybrid mode:
  - in a run with no card set mode (Standard run, or Custom Run with neither card set ticked): the
    owner also gets Slay the Spire 1 cards;
  - in a Custom Run with "Slay the Spire 1 Cards": the owner also gets Slay the Spire 2 cards.
- **Never offered when the run is already Hybrid** ("Cards From Both Games" ticked): it would do nothing.
- "Hybrid for its owner" means everything the Hybrid mode does today, for that player only: card
  rewards, merchant, transforms, random card generation, a duplicate (Rupture) as one slot that comes out
  as a random version, and the version swap button on the owner's card rewards.

**Non-goals**
- Slay the Spire 1 relics in general. This is one mod relic, not a port of StS1 relics.
- Characters without Slay the Spire 1 cards yet (Silent, Defect, Regent, Necrobinder): the relic is in
  the Ironclad pool only, so they don't find it. When step 3 brings Silent and Defect cards, adding it to
  their pools is a one-line follow-up.
- Daily runs: unchanged (see §4).

**Anti-goals**
- Co-op desync: every client must compute the same card offers for every player.
- The relic changing anything for players who don't own it.
- A Standard run without the relic differing in any way from vanilla.

## 3. Decisions made by the owner

1. The relic exists in Standard runs. This replaces the earlier rule "Standard and Daily runs are not
   modified" for Standard runs: they stay vanilla **unless a player gets this relic**.
2. Common rarity.
3. One player only (character relic), not run-wide.
4. In StS1-only mode it adds StS2 cards; with no mode it adds StS1 cards; unavailable in Hybrid.

Also decided on 2026-10-29 (already built): the **Neow's Blessings** Custom Run option brings back Neow's
usual blessings in runs with modifiers; modifiers with their own Neow option (Draft, Sealed Deck) go
first. And the Watcher as a playable character in StS1 card set modes is in the backlog (this lifts the
"no Watcher character" non-goal later, not now).

## 4. Builder's calls (Claude; the owner may override)

- **Name:** "Spire Souvenir" / « Souvenir de la Tour » (the French game text calls the Spire « la Tour »).
  Not "Strange Spoon": that is a Slay the Spire 1 relic.
- **Description** (both languages), in the game's relic style, e.g. "You can now find cards from the
  other Slay the Spire." The exact wording follows the vanilla relic texts.
- **Icon:** borrowed from a vanilla relic, like the StS1 powers borrow vanilla power icons. No new art.
- **Daily runs:** the relic is not offered (Daily runs keep their "not modified" rule). Reason: they are
  a shared, seeded challenge.
- **Custom Runs:** offered like in Standard runs, except when Hybrid is ticked.
- **Where it can be found:** wherever a Common Ironclad relic can (combat rewards, chests, the merchant).
  Not at Neow: Neow's blessings stay vanilla.
- **Losing the relic** (an event that removes or swaps it): the owner's pool goes back to the run's mode
  from then on. Cards already in the deck stay.
- **Save/resume:** relics are saved by the game, so the effect comes back with the relic.

## 5. What the game gives us (recon, v0.107.1)

- `RelicRarity.Common` exists; per-character pools live in `Models.RelicPools` (`IroncladRelicPool`,
  built by `GenerateAllRelics`, filtered by epoch unlocks in `GetUnlockedRelics`). The mod relic has no
  epoch, so it is always unlocked once it is in the pool.
- Each player has their own relic grab bag, filled at run start from the shared pool plus **their
  character's pool** (`RelicGrabBag.Populate(Player, Rng)`). So a relic in the Ironclad pool is only ever
  drawn for Ironclad players, which is exactly "a character relic". The relic must be in the pool before
  the grab bag is filled (a postfix on the pool's relic list, not a late insertion).
- `RelicModel.IsAllowed(IRunState)` decides whether a relic may be offered in a run (used for "not in
  Hybrid, not in Daily").
- Today the card set mode is read once per run (`CardSetPools.ModeOf(IRunState)`) inside the
  `CardPoolModel.GetUnlockedCards` postfix, which has **no player argument**. The per-player mode needs
  the player whose offer is being built; the builder must find it at each call site (reward options,
  merchant, transforms, combat generation) and route it through. This is the main technical risk.
- The random version patch and the reward swap patch also read the run's mode; they must read the
  player's mode.

## 6. Left to the builder

- How the player reaches the pool composition (a scoped "current player" around the vanilla calls, or
  patches at each call site), as long as co-op clients agree.
- The exact relic text, icon choice and its rendering in the relic tooltip.
- Tests: the per-player mode rule (run mode + owns relic → effective mode) is pure logic and gets unit
  tests; the rest goes in an in-game checklist (solo, co-op with the relic on one player only,
  save/resume, merchant, transform).
