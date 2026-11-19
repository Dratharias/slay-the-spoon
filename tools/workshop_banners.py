"""Draw the Workshop images (workshop/images/): one slim banner per party mode and section, card and relic showcases
and the preview (thumbnail), in English and French.

Usage: python3 -I tools/workshop_banners.py [--cards DIR]

Without --cards only the banners are redrawn. The showcases and the preview need card renders from the game:
with the mod installed (make up) and the game closed, launch it from its folder with
  SLAYSPOON_RENDER_DIR=DIR SLAYSPOON_RENDER_CARDS="<CARDS below, comma-separated>" SteamAppId=2868840 ./SlayTheSpire2
which writes DIR/{eng,fra}/<Class>.png and quits (src/DevConsole/CardRenderer.cs; "Name+9" renders a Forge level,
"relic:Name" a relic's icon and tooltip, "gif:Name" the frames of a card's Forge climb from +0 to +9 in
DIR/{eng,fra}/gif_<Name>/, which GIFS turns into an animated GIF).

The descriptions show the images with [img] from the public GitHub repo (raw.githubusercontent.com), so commit the
PNGs and push before the upload. Banner icons are the mod's own art or drawn symbols; the showcases show rendered cards
and creature sprites. Fonts: Cinzel Bold when installed (~/.local/share/fonts), else Liberation Serif Bold; symbols
from Symbola.
"""
import argparse
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

REPO = Path(__file__).resolve().parents[1]
IMAGES = REPO / "godot/SlaySpoon/images"
OUT = REPO / "workshop/images"
W, H, ICON = 640, 52, 42
BG_TOP, BG_BOTTOM = (36, 32, 52), (20, 22, 33)
GOLD, GREY = (242, 210, 122), (190, 190, 200)

# key: (English title, French title, art: image path under godot/SlaySpoon/images, or "glyph:<char>", or "dolls").
# Party modes, then the section banners (StS1 sits under Slay the Spoon, Custom Runs and Co-op close the page).
MODES = {
    "spoon": ("Slay the Spoon", "Slay the Spoon", "cards/spoon/spoon_spoonful.png"),
    "sts1": ("Slay the Spire 1", "Slay the Spire 1", "packed/character_select/char_select_sts1_watcher.png"),
    "folie": ("Folie à deux", "Folie à deux", "glyph:⚔"),
    "matriochka": ("Matriochka", "Matriochka", "dolls"),
    "midas": ("King Midas", "Roi Midas", "glyph:♛"),
    "forge": ("The Forge", "La Forge", "glyph:⚒"),
    "cursed": ("Cursed Deck", "Deck maudit", "relics/spoon/spoon_unholy_grimoire_large.png"),
    "achilles": ("Achilles' Heel", "Talon d’Achille", "relics/spoon/spoon_achilles_heel_large.png"),
    "glass": ("Glass Cannon", "Canon de verre", "relics/spoon/spoon_glass_heart_large.png"),
    "custom": ("Custom Runs", "Parties personnalisées", "glyph:⚙"),
    "coop": ("Co-op and tuning", "Coop et réglages", "glyph:⚖"),
}

# Showcase rows: card classes (rendered) and creature sprites under godot/SlaySpoon/images ("img:<path>").
SHOWCASES = {
    "spoon-cards": ["SpoonSpoonful", "SpoonTeaspoon", "SpoonStolenSpoonful", "img:spoon/spoon.png"],
    "cursed-cards": ["Sts1Parasite", "Sts1Necronomicurse", "Sts1Pride"],
    "sts1-cards": ["img:creatures/sts1_watcher/body.png", "Sts1Ragnarok", "Sts1Wish", "img:ending/heart.png"],
    # One mode item each: The Forge's showpiece card at +9, the other modes' relic (icon and tooltip).
    "forge-card": ["Sts1LimitBreak+9"],
    "matriochka-relic": ["relic:DollysMirror"],
    "midas-relic": ["relic:GoldenPearl"],
    "cursed-relic": ["relic:SpoonUnholyGrimoire"],
    "achilles-relic": ["relic:SpoonAchillesHeel"],
    "glass-relic": ["relic:SpoonGlassHeart"],
}
# Animated GIFs: key -> card class whose "gif:<Class>" frames the game renders (12 fps).
GIFS = {"forge-climb": "Sts1LimitBreak"}
GIF_FPS = 12
GIF_W = 320  # the renders are 500 px wide; at full size the climb weighs about 4 MB
CARDS = sorted({c for row in SHOWCASES.values() for c in row if not c.startswith("img:")} | {"Sts1Blasphemy"}
               | {f"gif:{c}" for c in GIFS.values()})
ROW_W, ROW_H = 1280, 420
LANG_DIR = {"en": "eng", "fr": "fra"}


def _font(candidates: list[Path], size: int) -> ImageFont.FreeTypeFont:
    for path in candidates:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    raise FileNotFoundError(candidates[-1])


TITLE_FONTS = [Path.home() / ".local/share/fonts/Cinzel-Bold.ttf",
               Path("/usr/share/fonts/liberation-serif-fonts/LiberationSerif-Bold.ttf")]
CAPTION_FONTS = [Path("/usr/share/fonts/liberation-sans-fonts/LiberationSans-Regular.ttf")]
GLYPH_FONTS = [Path("/usr/share/fonts/gdouros-symbola/Symbola.ttf"),
               Path("/usr/share/fonts/google-noto/NotoSansSymbols2-Regular.ttf")]


def _background() -> Image.Image:
    img = Image.new("RGB", (W, H))
    d = ImageDraw.Draw(img)
    for y in range(H):
        t = y / (H - 1)
        d.line([(0, y), (W, y)], fill=tuple(round(a + (b - a) * t) for a, b in zip(BG_TOP, BG_BOTTOM)))
    d.line([(0, H - 3), (W, H - 3)], fill=GOLD, width=2)
    return img


def _disc_mask(size: int) -> Image.Image:
    big = Image.new("L", (size * 4, size * 4), 0)
    ImageDraw.Draw(big).ellipse([0, 0, size * 4 - 1, size * 4 - 1], fill=255)
    return big.resize((size, size), Image.LANCZOS)


def _art_icon(path: Path) -> Image.Image:
    src = Image.open(path).convert("RGBA")
    side = min(src.size)
    left, top = (src.width - side) // 2, (src.height - side) // 2
    src = src.crop((left, top, left + side, top + side)).resize((ICON, ICON), Image.LANCZOS)
    plate = Image.new("RGBA", (ICON, ICON), (14, 15, 24, 255))
    plate.alpha_composite(src)
    plate.putalpha(_disc_mask(ICON))
    return plate


def _glyph_icon(char: str) -> Image.Image:
    icon = Image.new("RGBA", (ICON, ICON), (0, 0, 0, 0))
    ImageDraw.Draw(icon).ellipse([0, 0, ICON - 1, ICON - 1], fill=(14, 15, 24, 255))
    font = _font(GLYPH_FONTS, 62)
    d = ImageDraw.Draw(icon)
    box = d.textbbox((0, 0), char, font=font)
    x = (ICON - (box[2] - box[0])) / 2 - box[0]
    y = (ICON - (box[3] - box[1])) / 2 - box[1]
    d.text((x, y), char, font=font, fill=GOLD)
    return icon


def _dolls_icon() -> Image.Image:
    """Three nesting dolls, big to small: an egg body and a round head each."""
    s = 4
    big = Image.new("RGBA", (ICON * s, ICON * s), (0, 0, 0, 0))
    d = ImageDraw.Draw(big)
    d.ellipse([0, 0, ICON * s - 1, ICON * s - 1], fill=(14, 15, 24, 255))
    base = 74 * s
    for cx, h, color in ((30, 46, (196, 64, 58)), (55, 34, (226, 150, 60)), (72, 24, GOLD)):
        w = h * 0.62
        cx *= s
        hs, ws = h * s, w * s
        d.ellipse([cx - ws / 2, base - hs * 0.68, cx + ws / 2, base], fill=color)
        r = ws * 0.36
        d.ellipse([cx - r, base - hs * 0.68 - r * 1.5, cx + r, base - hs * 0.68 + r * 0.5], fill=color)
        fr = r * 0.62
        fy = base - hs * 0.68 - r * 0.5
        d.ellipse([cx - fr, fy - fr, cx + fr, fy + fr], fill=(246, 226, 200))
    return big.resize((ICON, ICON), Image.LANCZOS)


def _icon(art: str) -> Image.Image:
    if art.startswith("glyph:"):
        return _glyph_icon(art.removeprefix("glyph:"))
    if art == "dolls":
        return _dolls_icon()
    return _art_icon(IMAGES / art)


def banner(title: str, art: str) -> Image.Image:
    img = _background().convert("RGBA")
    icon = _icon(art)
    x0, y0 = 10, (H - ICON) // 2 - 1
    glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse([x0 - 3, y0 - 3, x0 + ICON + 2, y0 + ICON + 2], fill=GOLD + (150,))
    img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(2)))
    ImageDraw.Draw(img).ellipse([x0 - 1, y0 - 1, x0 + ICON, y0 + ICON], outline=GOLD, width=1)
    img.alpha_composite(icon, (x0, y0))
    d = ImageDraw.Draw(img)
    font = _font(TITLE_FONTS, 28)
    box = d.textbbox((0, 0), title, font=font)
    d.text((x0 + ICON + 16, (H - 2 - (box[3] - box[1])) / 2 - box[1]), title, font=font, fill=GOLD)
    return img.convert("RGB")


def _piece(cards: Path, lang: str, name: str) -> Image.Image:
    if name.startswith("img:"):
        src = Image.open(IMAGES / name.removeprefix("img:")).convert("RGBA")
    else:
        file = name.removeprefix("relic:").replace("+", "Plus")
        src = Image.open(cards / LANG_DIR[lang] / f"{file}.png").convert("RGBA")
    return src.crop(src.getbbox())


def _fit(img: Image.Image, height: int) -> Image.Image:
    return img.resize((round(img.width * height / img.height), height), Image.LANCZOS)


def showcase(cards: Path, lang: str, names: list[str]) -> Image.Image:
    """One row on a transparent background, items scaled to the same height and spaced evenly."""
    if all(n.startswith("relic:") for n in names):
        # Relic tooltips keep one scale (70 % of the render) whatever their text length.
        pieces = [_fit(p, round(p.height * 0.7)) for p in (_piece(cards, lang, n) for n in names)]
        height = max(p.height for p in pieces) + 20
    else:
        height = ROW_H
        pieces = [_fit(_piece(cards, lang, n), height - 20) for n in names]
    scale = min(1.0, (ROW_W - 30 * (len(pieces) + 1)) / sum(p.width for p in pieces))
    if scale < 1:
        pieces = [_fit(p, round(p.height * scale)) for p in pieces]
    gap = (ROW_W - sum(p.width for p in pieces)) / (len(pieces) + 1)
    row = Image.new("RGBA", (ROW_W, height), (0, 0, 0, 0))
    x = gap
    for p in pieces:
        row.alpha_composite(p, (round(x), (height - p.height) // 2))
        x += p.width + gap
    return row


def climb_gif(cards: Path, lang: str, card: str, out: Path) -> int:
    """The rendered climb frames over the banners' gradient (GIF has no soft transparency), looped forever."""
    files = sorted((cards / LANG_DIR[lang] / f"gif_{card}").glob("*.png"))
    if not files:
        raise SystemExit(f"no gif:{card} frames in {cards / LANG_DIR[lang]}")
    first = Image.open(files[0])
    size = (GIF_W, round(first.height * GIF_W / first.width))
    bg = Image.new("RGBA", size)
    d = ImageDraw.Draw(bg)
    for y in range(size[1]):
        t = y / (size[1] - 1)
        d.line([(0, y), (size[0], y)], fill=tuple(round(a + (b - a) * t) for a, b in zip(BG_TOP, BG_BOTTOM)))
    frames = []
    for f in files:
        frame = bg.copy()
        frame.alpha_composite(Image.open(f).convert("RGBA").resize(size, Image.LANCZOS))
        frames.append(frame.convert("RGB").quantize(colors=64, method=Image.MEDIANCUT, dither=Image.Dither.NONE))
    frames[0].save(out, save_all=True, append_images=frames[1:], duration=round(1000 / GIF_FPS), loop=0, optimize=True,
                   disposal=1)
    return len(frames)


def preview(cards: Path) -> Image.Image:
    """512x512 Workshop thumbnail: the title over a fan of three cards, the Spoonful (English) in front."""
    size = 512
    img = Image.new("RGBA", (size, size))
    d = ImageDraw.Draw(img)
    for y in range(size):
        t = y / (size - 1)
        d.line([(0, y), (size, y)], fill=tuple(round(a + (b - a) * t) for a, b in zip((44, 36, 62), BG_BOTTOM)))
    glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse([96, 150, 416, 470], fill=GOLD + (70,))
    img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(40)))
    for name, angle, cx in (("Sts1Ragnarok", 14, 150), ("Sts1Parasite", -14, 362)):
        card = _fit(_piece(cards, "en", name), 250).rotate(angle, expand=True, resample=Image.BICUBIC)
        img.alpha_composite(card, (cx - card.width // 2, 300 - card.height // 2))
    front = _fit(_piece(cards, "en", "SpoonSpoonful"), 330)
    shadow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    sx, sy = (size - front.width) // 2, 168
    shadow.paste((0, 0, 0, 160), (sx + 6, sy + 10), front.getchannel("A"))
    img.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(8)))
    img.alpha_composite(front, (sx, sy))
    d = ImageDraw.Draw(img)
    font = _font(TITLE_FONTS, 58)
    title = "Slay the Spoon"
    box = d.textbbox((0, 0), title, font=font, stroke_width=3)
    tx, ty = (size - (box[2] - box[0])) / 2 - box[0], 22 - box[1]
    d.text((tx, ty), title, font=font, fill=GOLD, stroke_width=3, stroke_fill=(14, 15, 24))
    sub = "Slay the Spire 1 · party modes"
    sfont = _font(CAPTION_FONTS, 20)
    sb = d.textbbox((0, 0), sub, font=sfont)
    d.text(((size - (sb[2] - sb[0])) / 2, 98), sub, font=sfont, fill=GREY)
    return img.convert("RGB")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", type=Path, help="card renders from the game (DIR/{eng,fra}/<Class>.png)")
    args = parser.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    for key, (en, fr, art) in MODES.items():
        for lang, title in (("en", en), ("fr", fr)):
            banner(title, art).save(OUT / f"{key}-{lang}.png", optimize=True)
    print(f"{len(MODES) * 2} banners in {OUT.relative_to(REPO)}")
    if args.cards:
        for key, names in SHOWCASES.items():
            for lang in ("en", "fr"):
                showcase(args.cards, lang, names).save(OUT / f"{key}-{lang}.png", optimize=True)
        for key, card in GIFS.items():
            for lang in ("en", "fr"):
                climb_gif(args.cards, lang, card, OUT / f"{key}-{lang}.gif")
        preview(args.cards).save(OUT / "preview.png", optimize=True)
        print(f"{len(SHOWCASES) * 2} showcases, {len(GIFS) * 2} GIFs and preview.png in {OUT.relative_to(REPO)}")


if __name__ == "__main__":
    main()
