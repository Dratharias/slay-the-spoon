"""Watcher data pipeline: catalog all registered cards, stage pending ports outside src."""
import argparse
import json
from pathlib import Path
import art
import codegen
import extract


def generate(decomp, jar, root):
    for name, cards in (("watcher", extract.extract(decomp, "purple")),
                        ("watcher-generated", extract.extract_generated(decomp))):
        path = root / "data/sts1" / (name + ".json")
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(cards, indent=1, ensure_ascii=False) + "\n", encoding="utf-8")
        rows = [{"sts1": c["class"], "relation": "unique"} for c in cards]
        folder = "watcher" if name == "watcher" else "watchertokens"
        art.export(jar, cards, rows, root / "godot/SlaySpoon/images/cards" / folder)
        if name == "watcher":
            pending = [c for c in cards if not (root / "src/Cards/Sts1/Watcher" / (codegen.class_name(c["class"]) + ".cs")).exists()]
            codegen.generate(pending, rows, "Watcher", root / "data/sts1/watcher-skeletons",
                             root / "godot/SlaySpoon/localization", {"unique"})
    art.export_relics(jar, root / "godot/SlaySpoon/images/relics/watcher")


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--decomp", type=Path, required=True)
    ap.add_argument("--jar", type=Path, required=True)
    ap.add_argument("--root", type=Path, default=Path("."))
    args = ap.parse_args()
    generate(args.decomp, args.jar, args.root)
