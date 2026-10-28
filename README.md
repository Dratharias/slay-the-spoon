# Slay the Spoon

A Slay the Spire 2 mod for chaotic co-op and solo runs: it brings Slay the Spire 1 cards into
Slay the Spire 2 and adds run options that the lobby host sets in the **Custom Run** screen.
Standard and Daily runs are left untouched.

> **Unofficial.** This mod is not affiliated with, endorsed by or sponsored by Mega Crit.
> Slay the Spire, Slay the Spire 2 and their assets (art, card text) belong to Mega Crit.
> The mod is free; there is no paid content.

> **Vibe coded.** The code in this repository was written by an AI (Claude, by Anthropic) under
> the direction of the author, who designed the mod and tests it in game.

## Features

Current (0.1.0):

- **Monster HP** and **Monster Damage** options in Custom Run, 25% to 500% in 25% steps,
  synced to every player in co-op and kept in saves.

- **Progress sync (optional):** with mods active, Slay the Spire 2 uses a separate "modded" profile
  that starts with nothing unlocked. The first time you launch with the mod, it asks whether to copy
  your regular progress into that profile on every launch. One way only: your regular save is read,
  never written; progress made only in modded runs is replaced at the next launch. The answer is
  stored in `mod_configs/SlaySpoon.cfg` in the game's user folder (on Linux
  `~/.local/share/SlayTheSpire2/mod_configs/`); delete that file to be asked again.

Planned: Slay the Spire 1 cards (Ironclad, Silent, Defect, Colorless) with StS1-only / StS2-only /
hybrid card sets, StS1 starting decks, and merchant loans between players.

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
```

Set `STS2_DIR` if the game is not in the default Steam library.

## License

MIT for the code in this repository (see `LICENSE`). Game assets are Mega Crit's.
