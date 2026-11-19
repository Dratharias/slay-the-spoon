"""Draw the Workshop description banners: one per party mode, English and French (workshop/images/).

Usage: python3 -I tools/workshop_banners.py

The descriptions show them with [img] from the public GitHub repo (raw.githubusercontent.com), so commit the PNGs.
Only the mod's own art is used (Pixabay placeholders, the drawn Glass Heart) or a drawn symbol; no Mega Crit art.
Fonts: Cinzel Bold when installed (~/.local/share/fonts), else Liberation Serif Bold; symbols from Symbola.
"""
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

REPO = Path(__file__).resolve().parents[1]
IMAGES = REPO / "godot/SlaySpoon/images"
OUT = REPO / "workshop/images"
W, H, ICON = 640, 112, 92
BG_TOP, BG_BOTTOM = (36, 32, 52), (20, 22, 33)
GOLD, GREY = (242, 210, 122), (190, 190, 200)

# key: (English title, French title, art: image path under godot/SlaySpoon/images, or "glyph:<char>", or "dolls")
MODES = {
    "spoon": ("Slay the Spoon", "Slay the Spoon", "cards/spoon/spoon_spoonful.png"),
    "folie": ("Folie à deux", "Folie à deux", "glyph:⚔"),
    "matriochka": ("Matriochka", "Matriochka", "dolls"),
    "midas": ("King Midas", "Roi Midas", "glyph:♛"),
    "forge": ("The Forge", "La Forge", "glyph:⚒"),
    "cursed": ("Cursed Deck", "Deck maudit", "relics/spoon/spoon_unholy_grimoire_large.png"),
    "achilles": ("Achilles' Heel", "Talon d’Achille", "relics/spoon/spoon_achilles_heel_large.png"),
    "glass": ("Glass Cannon", "Canon de verre", "relics/spoon/spoon_glass_heart_large.png"),
}
CAPTION = {"en": "Slay the Spoon · party mode", "fr": "Slay the Spoon · mode de jeu"}


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


def banner(title: str, caption: str, art: str) -> Image.Image:
    img = _background().convert("RGBA")
    icon = _icon(art)
    x0, y0 = 14, (H - ICON) // 2 - 1
    glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse([x0 - 4, y0 - 4, x0 + ICON + 3, y0 + ICON + 3], fill=GOLD + (150,))
    img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(3)))
    ImageDraw.Draw(img).ellipse([x0 - 2, y0 - 2, x0 + ICON + 1, y0 + ICON + 1], outline=GOLD, width=2)
    img.alpha_composite(icon, (x0, y0))
    d = ImageDraw.Draw(img)
    tx = x0 + ICON + 24
    d.text((tx, 22), title, font=_font(TITLE_FONTS, 42), fill=GOLD)
    d.text((tx, 74), caption, font=_font(CAPTION_FONTS, 17), fill=GREY)
    return img.convert("RGB")


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for key, (en, fr, art) in MODES.items():
        for lang, title in (("en", en), ("fr", fr)):
            banner(title, CAPTION[lang], art).save(OUT / f"{key}-{lang}.png", optimize=True)
    print(f"{len(MODES) * 2} banners in {OUT.relative_to(REPO)}")


if __name__ == "__main__":
    main()
