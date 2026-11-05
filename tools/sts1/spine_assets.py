"""StS1 Spine skeletons for the mod's creatures, converted to Spine 4.2 for StS2's spine-godot runtime.

Usage: python spine_assets.py --jar desktop-1.0.jar --out godot/SlaySpoon/spine

Each creature gets <out>/<key>/skeleton.spine-json, skeleton.atlas and the atlas page images, copied unchanged from the
jar (spine-godot reads 3.4's libgdx atlas format). Animations are renamed to the names StS2's creature animator plays,
with StS1's track time scales baked in (see CREATURES).
"""
import argparse
import json
import sys
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import spine34to42

# key -> (jar folder, [(StS2 name, StS1 name, StS1 time scale)]); None keeps every animation as it is.
CREATURES = {
    # CorruptHeart: idle at 1.5.
    "heart": ("images/npcs/heart", [("idle_loop", "idle", 1.5)]),
    # SpireSpear: Idle at 0.7; both attacks play Attack_2 ("ATTACK"); Hit then Idle at 0.7.
    "spear": ("images/monsters/theEnding/spear",
              [("idle_loop", "Idle", 0.7), ("attack", "Attack_2", 1.0), ("hurt", "Hit", 1.0)]),
    # SpireShield: Bash plays Attack ("ATTACK"), Smash plays old_attack ("OLD_ATTACK").
    "shield": ("images/monsters/theEnding/shield",
               [("idle_loop", "Idle", 1.0), ("attack", "Attack", 1.0), ("attack_smash", "old_attack", 1.0),
                ("hurt", "Hit", 1.0)]),
    # Watcher: Idle at 0.7, Hit then Idle. Attacks and casts have no animation in StS1.
    "watcher": ("images/characters/watcher/idle", [("idle_loop", "Idle", 0.7), ("hurt", "Hit", 1.0)]),
    # The staff eye, drawn at the body's eye_anchor bone: None, Calm, Wrath, Divinity.
    "watcher_eye": ("images/characters/watcher/eye_anim", None),
}


def atlas_pages(atlas_text: str) -> list:
    """Page image names: in the libgdx format a page starts after a blank line (or at the top) with its file name."""
    pages, start = [], True
    for line in atlas_text.splitlines():
        if not line.strip():
            start = True
            continue
        if start:
            pages.append(line.strip())
            start = False
    return pages


def export(jar: Path, out: Path, creatures=CREATURES) -> list:
    written = []
    with zipfile.ZipFile(jar) as z:
        for key, (folder, renames) in creatures.items():
            dst = out / key
            dst.mkdir(parents=True, exist_ok=True)
            atlas = z.read(f"{folder}/skeleton.atlas").decode("utf-8")
            data = spine34to42.convert(json.loads(z.read(f"{folder}/skeleton.json")), renames)
            (dst / "skeleton.spine-json").write_text(json.dumps(data, separators=(",", ":")), encoding="utf-8")
            (dst / "skeleton.atlas").write_text(atlas, encoding="utf-8")
            written += [dst / "skeleton.spine-json", dst / "skeleton.atlas"]
            for page in atlas_pages(atlas):
                (dst / page).write_bytes(z.read(f"{folder}/{page}"))
                written.append(dst / page)
    return written


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--jar", type=Path, required=True)
    ap.add_argument("--out", type=Path, required=True)
    args = ap.parse_args(argv)
    for path in export(args.jar, args.out):
        print(path)
    return 0


if __name__ == "__main__":
    sys.exit(main())
