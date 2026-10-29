# Slay the Spoon

A Slay the Spire 2 mod for chaotic co-op and solo runs: it brings Slay the Spire 1 cards into
Slay the Spire 2 and adds run options that the lobby host sets in the **Custom Run** screen.
Standard and Daily runs are left untouched.

> **Unofficial.** This mod is not affiliated with, endorsed by or sponsored by Mega Crit.
> Slay the Spire, Slay the Spire 2 and their assets (art, card text) belong to Mega Crit.
> The mod is free; there is no paid content.

> **Vibe coded.** The code in this repository was written by an AI (Claude, by Anthropic) under
> the direction of the author, who designed the mod and tests it in game.

> **Found a bug?** The code is public. I patch the bugs that annoy me, when I have tokens to spare.
> You are entirely free to fork the mod and make your own version.

## Features

Current (0.2.0):

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
  **Cards From Both Games** mixes the two. With neither, cards are vanilla. Cards that exist in
  both games under the same name are the Slay the Spire 2 version.

- **Ironclad's Slay the Spire 1 cards:** the 31 cards that Slay the Spire 2 dropped (Heavy Blade,
  Searing Blow, Limit Break, Berserk…), with their Slay the Spire 1 art and numbers. Other
  characters keep their own cards in both modes for now. The Ironclad's starting deck is the same
  in both games.

Planned: the Ironclad's three cards that differ between the games (Juggernaut, Rampage, Rupture),
a way to swap between both versions of a card in the mixed mode, then Silent, Defect and
Colorless cards, and merchant loans between players.

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
