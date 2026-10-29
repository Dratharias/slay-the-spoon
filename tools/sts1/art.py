"""Export StS1 card portraits for the cards that ship as StS1 classes.

Usage: python art.py --jar desktop-1.0.jar --sts1 data/sts1/ironclad.json
           --table data/duplicates/ironclad.json --out godot/SlaySpoon/images/cards/ironclad
       python art.py --jar desktop-1.0.jar --events godot/SlaySpoon/images/events

StS1's 1024Portraits are 500×380; StS2 draws card art from 250×190 atlas sprites (same aspect).
Event art (step 3d) keeps StS1's 600×600 size.
"""
import argparse
import io
import json
import zipfile
from pathlib import Path

from PIL import Image

import codegen
import match

SIZE = (250, 190)

# StS1 event image -> mod event ModelDb id (src/Events/), which the event portrait patch looks up.
EVENTS = {"vampires": "sts1_vampires", "drugDealer": "sts1_augmenter", "theNest": "sts1_nest"}


def export(jar: Path, cards: list, rows: list, out: Path) -> list:
    """Write <model id>.png for every card whose relation is not same-sts2; return the paths."""
    shipped = {r["sts1"] for r in rows if r["relation"] != "same-sts2"}
    out.mkdir(parents=True, exist_ok=True)
    written = []
    with zipfile.ZipFile(jar) as z:
        for card in sorted(cards, key=lambda c: c["class"]):
            if card["class"] not in shipped:
                continue
            data = z.read(f"images/1024Portraits/{card['portrait']}.png")
            img = Image.open(io.BytesIO(data)).convert("RGBA").resize(SIZE, Image.LANCZOS)
            # Named after the ModelDb id, which is what Sts1Card.PortraitPath uses.
            path = out / f"{match.slug(codegen.class_name(card['class'])).lower()}.png"
            img.save(path, optimize=True)
            written.append(path)
    return written


def export_events(jar: Path, out: Path) -> list:
    """Write <event id>.png for the StS1 card events; return the paths."""
    out.mkdir(parents=True, exist_ok=True)
    written = []
    with zipfile.ZipFile(jar) as z:
        for name, model_id in sorted(EVENTS.items()):
            img = Image.open(io.BytesIO(z.read(f"images/events/{name}.jpg"))).convert("RGBA")
            path = out / f"{model_id}.png"
            img.save(path, optimize=True)
            written.append(path)
    return written


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--jar", type=Path, required=True)
    ap.add_argument("--events", type=Path)
    ap.add_argument("--sts1", type=Path)
    ap.add_argument("--table", type=Path)
    ap.add_argument("--out", type=Path)
    args = ap.parse_args()
    if args.events:
        print(f"{len(export_events(args.jar, args.events))} event images")
        return
    if not (args.sts1 and args.table and args.out):
        ap.error("--sts1, --table and --out are required without --events")
    cards = json.loads(args.sts1.read_text(encoding="utf-8"))
    rows = json.loads(args.table.read_text(encoding="utf-8"))
    print(f"{len(export(args.jar, cards, rows, args.out))} portraits")


if __name__ == "__main__":
    main()
