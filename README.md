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

Current (0.5.0):

- **Monster HP** and **Monster Damage** options in Custom Run, 25% to 500% in 25% steps,
  synced to every player in co-op and kept in saves. The top bar shows each one as its percent,
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
  version when it plays differently, with a button on card rewards to switch between the two.

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
  Bite, J.A.X. and Ritual Dagger have no event yet (console only).

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

Planned: the version switch at the merchant and on choose-a-card screens, and merchant loans
between players.

## Development status — 2026-10-29

Work is on `step-3`, not merged or published. The current version is **0.5.0**: Ironclad, Silent,
Defect and colorless cards are implemented and pass the automated checks (83 xUnit and 35 Python
tests, build with zero warnings). None of it has been tested in game yet; see the checklists
for [Silent](docs/testing/step-3a-checklist.md), [Defect](docs/testing/step-3b-checklist.md)
and [colorless cards](docs/testing/step-3c-checklist.md). Which cards count as the same card in
both games is still subject to the author's review in the comparison pages under
[docs/review/](docs/review/).

## Languages

English and French. Other game languages show the English text.

## Install

Every player in a co-op lobby needs the same version of the mod.

- Steam Workshop: coming later.
- Manually: copy the `SlaySpoon` folder (containing `SlaySpoon.json`, `SlaySpoon.dll`,
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
```

Set `STS2_DIR` if the game is not in the default Steam library. `make sts1-data` also needs
Slay the Spire 1 and its decompiled card classes (`STS1_DECOMP`, `STS1_JAR`; see the `Makefile`).

## License

MIT for the code in this repository (see `LICENSE`). Game assets are Mega Crit's.
