# Slay the Spoon

A Slay the Spire 2 mod for chaotic co-op and solo runs: it brings Slay the Spire 1 cards into
Slay the Spire 2 and adds run options that the lobby host sets in the **Custom Run** screen.
Daily runs stay vanilla. Standard runs can offer the Spire Souvenir relic described below.

> **Unofficial.** This mod is not affiliated with, endorsed by or sponsored by Mega Crit.
> Slay the Spire, Slay the Spire 2 and their assets (art, card text) belong to Mega Crit.
> The mod is free; there is no paid content.

> **Vibe coded.** The code in this repository was written with AI coding agents (Claude and Codex) under
> the direction of the author, who designed the mod and tests it in game.

> **Found a bug?** The code is public. I patch the bugs that annoy me, when I have tokens to spare.
> You are entirely free to fork the mod and make your own version.

## Features

Current (0.10.0):

- **Monster HP** and **Monster Damage** options in Custom Run, 25% to 500% in 25% steps (− / + buttons,
  or left/right on a gamepad), synced to every player in co-op and kept in saves. The top bar shows each one as its percent,
  red when monsters are stronger than vanilla and blue when weaker.

- **Progress sync (optional):** with mods active, Slay the Spire 2 uses a separate "modded" profile
  that starts with nothing unlocked. The first time you launch with the mod, it asks whether to copy
  your regular progress into that profile on every launch. One way only: your regular save is read,
  never written; progress made only in modded runs is replaced at the next launch. The answer is
  stored in `mod_configs/SlaySpoon.cfg` in the game's user folder (on Linux
  `~/.local/share/SlayTheSpire2/mod_configs/`); delete that file to be asked again.

- **Card sets:** two Custom Run options, at most one ticked. **Slay the Spire 1 Cards** offers
  Slay the Spire 1 cards instead of Slay the Spire 2 ones in rewards, shops and card generation;
  **Spire Souvenir** starts every character that has Slay the Spire 1 cards with the Spire Souvenir
  relic (below), which mixes the two. With neither and without the relic, cards are vanilla. A card that exists in
  both games is offered once: as its weaker version when only the numbers differ, and as either
  version when it plays differently, with a button to switch between the two on card rewards, at
  the merchant and on choose-a-card screens. After a reward grid (Sealed Deck), each such card kept
  asks which version to take.

- **Ironclad's Slay the Spire 1 cards:** the 31 cards that Slay the Spire 2 dropped (Heavy Blade,
  Searing Blow, Limit Break, Berserk…), plus Slay the Spire 1's Juggernaut, Rampage, Rupture, Clash,
  Dual Wield, Entrench and Shockwave,
  with their Slay the Spire 1 art and numbers. The Ironclad's starting deck is the same
  in both games.

- **Silent's Slay the Spire 1 cards:** the 37 Silent cards that Slay the Spire 2 dropped or changed
  (Catalyst, Corpse Explosion, Wraith Form, A Thousand Cuts, Grand Finale…), with their Slay the
  Spire 1 art and numbers.

- **Defect's Slay the Spire 1 cards:** 38 cards ported with their Slay the Spire 1 art and numbers
  (Electrodynamics, Recursion, Seek, Amplify, Self Repair…), including four switchable versions:
  Chill, Fusion, Storm and Tempest. The starting deck stays the same; Orbs use Slay the Spire 2's
  mechanics. Regent and Necrobinder keep their vanilla character pools.

- **Colorless Slay the Spire 1 cards:** 31 cards ported with their Slay the Spire 1 art and numbers
  (Apotheosis, Chrysalis, Madness, Magnetism, Panache, Secret Weapon…), for every character.
  The colorless pool follows the card set mode for all five characters, and the Spire Souvenir mixes
  both games' colorless cards for its owner. Apotheosis, Apparition, Bite, Enlightenment, J.A.X.,
  Metamorphosis and Ritual Dagger stay out of ordinary rewards, shops, transforms and random card
  generation: events that give their Slay the Spire 2 counterpart give the matching version, and
  Bite, J.A.X. and Ritual Dagger come from the Slay the Spire 1 events below.

- **Slay the Spire 1 events:** Vampires(?), Augmenter and The Nest, with their Slay the Spire 1 art and
  text, can replace an act 2 event when Slay the Spire 1 cards are in play: in a "Slay the Spire 1
  Cards" run, or when a player holds the Spire Souvenir. The Nest and Augmenter need one holder; in
  co-op, players without the relic see their card option locked. Vampires needs every player to hold
  it. Augmenter can also give **Mutagenic Strength**, a relic ported from Slay the Spire 1.

- **The Watcher:** Slay the Spire 1's Watcher is always visible and selectable as a sixth character in a Custom Run,
  on both host and client. She is also selectable in Standard solo. In either solo mode, choosing her implicitly enables the StS1 card set (including colorless
  cards and StS1 event eligibility), even without ticking **Slay the Spire 1 Cards**. This also applies after resuming.
  In co-op, the lobby modifiers still determine the shared card set; her own cards remain StS1.
  Unticking modifiers never hides or deselects her. She is not offered in Daily or Standard co-op.
  She brings stances (Wrath, Calm, Divinity), Mantra and Scry, 71 cards plus her 4 starter cards and the cards they create, her
  relics and her three potions, with Slay the Spire 1 art and numbers. Her visuals are static images for now (no
  animation). Discipline and Unraveling, which Slay the Spire 1 never registered, are not ported. Holy Water and Violet
  Lotus work but cannot be found yet. In a solo run her ascension is capped by your best vanilla character. Her
  playtime goes into the modded profile and never counts toward the regular unlocks. Needs in-game validation; see
  `docs/testing/step-7-checklist.md`.

- **Neow's Blessings:** a Custom Run option that brings back Neow's usual starting choice. Without it,
  any Custom Run option (even this mod's) makes Neow offer nothing, or only the options of modifiers
  like Draft. With it, those options come first, then the blessings.

- **Spire Souvenir:** a Common Ironclad, Silent and Defect relic, also found in Standard runs, that lets its owner find
  cards from the other game: Slay the Spire 1 cards in a normal run, Slay the Spire 2 cards in a
  "Slay the Spire 1 Cards" run. It only affects its owner; losing it ends the effect. It never drops
  in a "Spire Souvenir" run (eligible players already start with it) or in a Daily run.

- **`sts1card` console command** for testing: in a modded game the developer console opens with the
  `` ` `` key (`*` or `'` on AZERTY); `sts1card STS1_HEAVY_BLADE` puts that card in your hand in combat,
  or in your deck elsewhere. The vanilla `card` command can't see Slay the Spire 1 cards.
  `sts1event STS1_NEST` (or `STS1_VAMPIRES`, `STS1_AUGMENTER`) opens a Slay the Spire 1 event.

**Merchant Loans** (co-op Custom Runs): enable the modifier in the lobby. If a merchant purchase
fails for lack of gold, choose **Ask** to open a request. Other players can lend gold at 0–900%
interest or give it. Offers reserve the lender's gold until accepted, withdrawn, cancelled, or the
room is left. Select enough offers and accept, then try buying the item again.

Future gold gains repay the oldest loan first, shared equally between its remaining lenders.
Interest is fixed on acceptance; gifts create no debt. **Death and resurrection do not erase debts.**
Debts are saved with the run. Hover your gold to see what is left to repay or collect; the loan panel works with a
controller. Solo, Standard and Daily runs are unaffected.
The merchant interface still needs in-game validation; see `docs/testing/step-5-checklist.md`.

**Character Color** (pause menu, every mode): tint your character with a color overlay, or recolor only the parts close
to its own color (the threshold sets how close); optionally color your map drawings too. Saved for you and shown to
your co-op partners. It is cosmetic only.

**Act 4, The Ending:** unlocking Ascension 5 with any character enables Act 4 by default in Standard runs
at every ascension level, including 0. Profiles with the earlier permanent unlock retain access.
Collect the Ruby, Emerald and Sapphire keys (one set for the whole team in co-op). The Sapphire Key sits in chests as
one more choice beside the relics: in solo you take it instead of the relic; in co-op players who pick the same relic or
the key play rock-paper-scissors for it, and the losers rematch for what is left. With all three keys, the Act 3 boss
gives its rewards and the run continues into The Ending: a rest site, a merchant, the Spire Shield and Spear, then
the Corrupt Heart. Without all keys the run ends after Act 3 as usual. A Custom Run option, "Act 4", turns on the
keys and Act 4 without the unlock. Daily runs are unchanged. It still needs in-game validation; see
`docs/testing/step-6-checklist.md`.

## Development status — 2026-10-29

Work is on `step-3`, not merged or published. The current version is **0.6.0**: Ironclad, Silent,
Defect and colorless cards and the Slay the Spire 1 events are implemented and pass the automated
checks (100 xUnit and 36 Python tests, build with zero warnings). None of it has been tested in game yet; see the checklists
for [Silent](docs/testing/step-3a-checklist.md), [Defect](docs/testing/step-3b-checklist.md),
[colorless cards](docs/testing/step-3c-checklist.md) and [events](docs/testing/step-3d-checklist.md). Which cards count as the same card in
both games is still subject to the author's review in the comparison pages under
[docs/review/](docs/review/).

## Languages

English and French. Other game languages show the English text.

## Install

Every player in a co-op lobby needs the same version of the mod.

Slay the Spoon requires [BaseLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127)
3.4.7 or later (declared in `SlaySpoon.json`; the game refuses to load the mod without it).

- Steam Workshop: a friends-only alpha. BaseLib is a required item: subscribing to Slay the Spoon
  offers to subscribe to it too.
- Manually: install BaseLib, then copy the `SlaySpoon` folder (containing `SlaySpoon.json`, `SlaySpoon.dll`,
  `SlaySpoon.pck`) into `<Slay the Spire 2>/mods/`, then start the game and accept mod loading.

## Build

Requires Podman and Slay the Spire 2 installed through Steam (Linux).

```bash
make build     # build/mods/SlaySpoon/
make test      # unit tests
make up        # build and install into the game's mods folder
make down      # uninstall
make mp        # two local game windows (host + client) to test co-op
make sts1-data # regenerate card data, art and text from a Slay the Spire 1 install
make workshop-upload # upload build to the Steam Workshop (SteamCMD in Podman, asks for Steam login)
```

Set `STS2_DIR` if the game is not in the default Steam library. `make sts1-data` also needs
Slay the Spire 1 and its decompiled card classes (`STS1_DECOMP`, `STS1_JAR`; see the `Makefile`).

## License

MIT for the code in this repository (see `LICENSE`). Game assets are Mega Crit's.
