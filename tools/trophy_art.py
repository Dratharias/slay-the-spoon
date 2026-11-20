"""Trophy card portraits from the game's elite monsters.

The game has no static monster art (they are Spine skeletons), so the portraits are rendered in game first:

  SLAYSPOON_RENDER_DIR=DIR SLAYSPOON_RENDER_CARDS=elites SteamAppId=2868840 ./SlayTheSpire2

writes DIR/elites/<MONSTER_ID>.png (bestiary pose, transparent background; src/DevConsole/CardRenderer.cs). This script
trims each one, fits it into a 250x190 card portrait over a dark backdrop and writes
godot/SlaySpoon/images/cards/trophies/<monster_id>.png, the path SpoonTrophyCard reads (TrophyRule.PortraitPath).

  python3 tools/trophy_art.py DIR/elites
"""

import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

WIDTH, HEIGHT = 250, 190
MARGIN = 8
OUT = Path(__file__).resolve().parent.parent / "godot/SlaySpoon/images/cards/trophies"


def backdrop() -> Image.Image:
    """Dark red at the top to near-black at the bottom, with a soft light behind the monster."""
    img = Image.new("RGBA", (WIDTH, HEIGHT))
    draw = ImageDraw.Draw(img)
    top, bottom = (74, 28, 30), (18, 10, 12)
    for y in range(HEIGHT):
        t = y / (HEIGHT - 1)
        draw.line([(0, y), (WIDTH, y)], fill=tuple(round(a + (b - a) * t) for a, b in zip(top, bottom)) + (255,))
    glow = Image.new("L", (WIDTH, HEIGHT), 0)
    ImageDraw.Draw(glow).ellipse([WIDTH * 0.15, HEIGHT * 0.1, WIDTH * 0.85, HEIGHT * 1.0], fill=90)
    glow = glow.filter(ImageFilter.GaussianBlur(28))
    img.paste(Image.new("RGBA", (WIDTH, HEIGHT), (190, 90, 60, 255)), (0, 0), glow)
    return img


def portrait(src: Path) -> Image.Image | None:
    monster = Image.open(src).convert("RGBA")
    box = monster.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
    if box is None:
        return None
    monster = monster.crop(box)
    scale = min((WIDTH - 2 * MARGIN) / monster.width, (HEIGHT - 2 * MARGIN) / monster.height)
    monster = monster.resize((max(1, round(monster.width * scale)), max(1, round(monster.height * scale))), Image.LANCZOS)
    out = backdrop()
    x = (WIDTH - monster.width) // 2
    y = HEIGHT - MARGIN - monster.height
    out.alpha_composite(monster, (x, y))
    return out


def main(argv: list[str]) -> int:
    if len(argv) != 2:
        print(__doc__)
        return 2
    OUT.mkdir(parents=True, exist_ok=True)
    count = 0
    for src in sorted(Path(argv[1]).glob("*.png")):
        img = portrait(src)
        if img is None:
            print(f"skip {src.name}: empty render")
            continue
        img.save(OUT / (src.stem.lower() + ".png"))
        count += 1
    print(f"{count} trophy portraits in {OUT}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
