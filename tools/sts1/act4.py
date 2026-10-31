"""Import the StS1 Ending resources into a staging catalog, without shipping Java code.

The source atlas/skeleton files are preserved for conversion; they are not Godot-ready scenes.
Run with make sts1-act4-data. Only explicitly selected archive entries are extracted.
"""
import argparse
import hashlib
import json
import zipfile
from pathlib import Path

MONSTERS = ("CorruptHeart", "SpireShield", "SpireSpear")
POWERS = ("Invincible", "BeatOfDeath", "Painful Stabs", "Surrounded", "BackAttack")
LANGUAGES = ("eng", "fra")
ANIMATIONS = (
    "images/npcs/heart",
    "images/monsters/theEnding/shield",
    "images/monsters/theEnding/spear",
)
ASSETS = (
    "images/ui/topPanel/key_red.png",
    "images/ui/topPanel/key_green.png",
    "images/ui/topPanel/key_blue.png",
    "images/ui/topPanel/key_slots.png",
    "images/ui/map/boss/heart.png",
    "images/ui/map/bossOutline/heart.png",
    "images/largeRelics/ruby_key.png",
    "images/largeRelics/emerald_key.png",
    "images/largeRelics/sapphire_key.png",
    "endingScene/scene.atlas",
    "endingScene/scene.jpg",
    "endingScene/scene2.jpg",
    "endingScene/scene3.jpg",
    "audio/music/STS_EndingStinger_v1.ogg",
    "audio/music/STS_Act4_BGM_v2.ogg",
    "audio/music/STS_Boss4_v6.ogg",
    "images/npcs/heart/skeleton.json",
    "images/npcs/heart/skeleton.atlas",
    "images/npcs/heart/skeleton.png",
    "images/monsters/theEnding/shield/skeleton.json",
    "images/monsters/theEnding/shield/skeleton.atlas",
    "images/monsters/theEnding/shield/shield.png",
    "images/monsters/theEnding/spear/skeleton.json",
    "images/monsters/theEnding/spear/skeleton.atlas",
    "images/monsters/theEnding/spear/spear.png",
)


def json_bytes(value):
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def import_act4(jar: Path, out: Path):
    """Validate all required inputs before writing; return a deterministic provenance manifest."""
    with zipfile.ZipFile(jar) as archive:
        payloads = {name: archive.read(name) for name in ASSETS}
        localization = {}
        for lang in LANGUAGES:
            tables = {
                table: json.loads(archive.read(f"localization/{lang}/{table}.json").decode("utf-8-sig"))
                for table in ("monsters", "powers", "ui")
            }
            localization[lang] = {
                "monsters": {key: tables["monsters"][key] for key in MONSTERS},
                "powers": {key: tables["powers"][key] for key in POWERS},
                "ui": {"TheEnding": tables["ui"]["TheEnding"]},
            }
        animations = []
        for directory in ANIMATIONS:
            skeleton = json.loads(payloads[directory + "/skeleton.json"])
            animations.append({
                "source": directory,
                "spine_version": skeleton["skeleton"]["spine"],
                "animations": sorted(skeleton["animations"]),
                "status": "requires_sts2_conversion",
            })

    manifest = {
        "schema_version": 1,
        "source": "Slay the Spire / desktop-1.0.jar",
        "status": "source_import_only_not_playable",
        "monsters": list(MONSTERS),
        "powers": list(POWERS),
        "animations": animations,
        "assets": [
            {"source": name, "path": "assets/" + name, "bytes": len(data),
             "sha256": hashlib.sha256(data).hexdigest()}
            for name, data in sorted(payloads.items())
        ],
        "localization": [
            {"path": f"localization/{lang}.json",
             "sha256": hashlib.sha256(json_bytes(localization[lang])).hexdigest()}
            for lang in LANGUAGES
        ],
    }
    for name, data in payloads.items():
        target = out / "assets" / name
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(data)
    for lang, data in localization.items():
        target = out / "localization" / f"{lang}.json"
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(json_bytes(data))
    (out / "manifest.json").write_bytes(json_bytes(manifest))
    return manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--jar", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args()
    manifest = import_act4(args.jar, args.out)
    print(f"Act 4: {len(manifest['assets'])} assets, 3 monsters, 5 powers, EN/FR; source import only")


if __name__ == "__main__":
    main()
