"""Export StS1 card portraits for the cards that ship as StS1 classes.

Usage: python art.py --jar desktop-1.0.jar --sts1 data/sts1/ironclad.json
           --table data/duplicates/ironclad.json --out godot/SlaySpoon/images/cards/ironclad

StS1's 1024Portraits are 500×380; StS2 draws card art from 250×190 atlas sprites (same aspect).
"""
import argparse
import io
import json
import zipfile
from pathlib import Path

from PIL import Image

SIZE = (250, 190)


def export(jar: Path, cards: list, rows: list, out: Path) -> list:
    """Write sts1_<snake>.png for every card whose relation is not same-sts2; return the paths."""
    shipped = {r["sts1"] for r in rows if r["relation"] != "same-sts2"}
    out.mkdir(parents=True, exist_ok=True)
    written = []
    with zipfile.ZipFile(jar) as z:
        for card in sorted(cards, key=lambda c: c["snake"]):
            if card["class"] not in shipped:
                continue
            data = z.read(f"images/1024Portraits/{card['portrait']}.png")
            img = Image.open(io.BytesIO(data)).convert("RGBA").resize(SIZE, Image.LANCZOS)
            path = out / f"sts1_{card['snake']}.png"
            img.save(path, optimize=True)
            written.append(path)
    return written


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--jar", type=Path, required=True)
    ap.add_argument("--sts1", type=Path, required=True)
    ap.add_argument("--table", type=Path, required=True)
    ap.add_argument("--out", type=Path, required=True)
    args = ap.parse_args()
    cards = json.loads(args.sts1.read_text(encoding="utf-8"))
    rows = json.loads(args.table.read_text(encoding="utf-8"))
    print(f"{len(export(args.jar, cards, rows, args.out))} portraits")


if __name__ == "__main__":
    main()
