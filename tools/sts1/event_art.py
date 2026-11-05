"""Export the art of the StS1 relic events and their relics (backlog wave, after step 3d).

Usage: python event_art.py --jar desktop-1.0.jar --out godot/SlaySpoon

Writes images/events/<event model id>.png (StS1's 600×600 portraits, as art.py --events does for the card events)
and images/relics/events/<relic model id>{,_outline,_large}.png. StS1 ships no large art for some special relics:
those get the icon upscaled 2× instead. Relic-granted curses get their 250×190 portrait in images/cards/curses/.
"""
import argparse
import io
import zipfile
from pathlib import Path

from PIL import Image

# StS1 image name -> mod event model id.
EVENTS = {
    "goldenIdol": "sts1_golden_idol_event",
    "forgottenAltar": "sts1_forgotten_altar",
    "moaiHead": "sts1_moai_head",
    "cursedTome": "sts1_cursed_tome",
    "blacksmith": "sts1_accursed_blacksmith",
    "bonfire": "sts1_bonfire",
    "mindBloom": "sts1_mind_bloom",
}

# StS1 relic image stem -> mod relic model id.
RELICS = {
    "goldenIdolRelic": "sts1_golden_idol",
    "bloodyChalice": "sts1_bloody_idol",
    "necronomicon": "sts1_necronomicon",
    "enchiridion": "sts1_enchiridion",
    "codex": "sts1_nilrys_codex",
    "tongs": "sts1_warped_tongs",
    "spiritPoop": "sts1_spirit_poop",
    "bloom": "sts1_mark_of_the_bloom",
}

# StS1 card portrait (1024Portraits) -> mod card model id, for the curses a relic hands out.
CURSES = {
    "curse/necronomicurse": "sts1_necronomicurse",
    "curse/pain": "sts1_pain",
}
CARD_SIZE = (250, 190)


def _read(z: zipfile.ZipFile, name: str) -> Image.Image:
    return Image.open(io.BytesIO(z.read(name))).convert("RGBA")


def _save(img: Image.Image, path: Path) -> Path:
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path, optimize=True)
    return path


def export(jar: Path, out: Path) -> list:
    written = []
    names = set()
    with zipfile.ZipFile(jar) as z:
        names = set(z.namelist())
        for name, model_id in sorted(EVENTS.items()):
            written.append(_save(_read(z, f"images/events/{name}.jpg"), out / "images/events" / f"{model_id}.png"))
        for stem, model_id in sorted(RELICS.items()):
            base = out / "images/relics/events" / model_id
            icon = _read(z, f"images/relics/{stem}.png")
            written.append(_save(icon, Path(f"{base}.png")))
            written.append(_save(_read(z, f"images/relics/outline/{stem}.png"), Path(f"{base}_outline.png")))
            large_name = f"images/largeRelics/{stem}.png"
            large = _read(z, large_name) if large_name in names else icon.resize((icon.width * 2, icon.height * 2), Image.LANCZOS)
            written.append(_save(large, Path(f"{base}_large.png")))
        for portrait, model_id in sorted(CURSES.items()):
            img = _read(z, f"images/1024Portraits/{portrait}.png").resize(CARD_SIZE, Image.LANCZOS)
            written.append(_save(img, out / "images/cards/curses" / f"{model_id}.png"))
    return written


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--jar", type=Path, required=True)
    ap.add_argument("--out", type=Path, required=True)
    args = ap.parse_args()
    print(f"{len(export(args.jar, args.out))} images")


if __name__ == "__main__":
    main()
