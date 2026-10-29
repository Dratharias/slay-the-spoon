# StS1 Act 4 source import

Regenerate with `make sts1-act4-data` from the locally installed StS1 archive.
The importer is `tools/sts1/act4.py`; `manifest.json` records each selected asset's source
archive path, byte count and SHA-256, plus the generated localization hashes.

This directory is a staging catalog, outside the Godot PCK. It contains original source assets,
not playable StS2 monsters or scenes. The three skeletons use Spine 3.4.02 and still require
compatibility work. The EN/FR text retains StS1 formatting and needs adaptation to StS2.

`mechanics.json` is written by hand, not by the importer: enemy HP, moves, patterns, ascension values and
powers, read from the local StS1 classes whose SHA-256 it records (see `docs/review/act4-mechanics.md`).

Included: Heart, Shield and Spear animation sources; key icons; Heart map icons; Ending background;
ending music stinger; selected names and power descriptions.
Not yet imported: executable enemy behavior, key acquisition, act routing, combat music integration.
See `docs/superpowers/plans/2026-10-29-step-6-act4.md`.
