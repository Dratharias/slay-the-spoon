"""The waddling Spoon, from its combat sprite (godot/SlaySpoon/images/spoon/spoon.png), as two kinds of looping GIF:

- workshop/images/spoon-waddle.gif: the Spoon alone on a transparent background (README front page);
- workshop/images/spoon-cards-{en,fr}.gif: the Spoon showcase (spoon-cards-{en,fr}.png from workshop_banners.py) with
  its static Spoon replaced by the waddling one, for the Workshop descriptions.

The motion is a livelier take on SpoonMotion.Idle: the body rocks from +sway to -sway degrees around the handle tip (the
sprite's ground point, SpoonMonster.Origin) with Sine InOut easing, hops as it crosses the middle (stretched) and
squashes at each side. One cycle (two steps) is the whole GIF, so it loops without a seam.

The README GIF's transparency is one bit: edge pixels are blended onto MATTE (Steam's page blue) and kept when their
alpha passes ALPHA_CUT. The showcase GIF is opaque on MATTE, so frames only store the Spoon's moving area; on Steam
the blue is the page's own.

Run workshop_banners.py --cards first when the showcase cards change. Usage: python3 -I tools/spoon_waddle.py
"""
import math
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[1]
IMAGES = REPO / "workshop/images"
SPRITE = REPO / "godot/SlaySpoon/images/spoon/spoon.png"
PIVOT = (200, 593)  # SpoonMonster.Origin, in sprite pixels
HOP = 18.0  # sprite px at mid-swing
STRETCH = 0.05  # y stretch at mid-swing, x squash at the sides
HALF = 1.0  # seconds per step
FPS = 25
MATTE = (27, 40, 56)  # Steam's #1b2838
ALPHA_CUT = 96

# The README GIF.
ALONE_SWAY = 8.0
ALONE_PAD_TOP = 64  # room for the hop above the sprite's own frame
ALONE_SCALE = 0.3

# The showcase GIF: the static Spoon is the last item of the row; the rock stays inside the row's right margin.
SHOWCASE_SWAY = 6.0
SHOWCASE_PAD_TOP = 32


def sine_in_out(t: float) -> float:
    return -(math.cos(math.pi * t) - 1) / 2


def pose(time: float, sway: float) -> tuple[float, float, float, float]:
    """Rotation (degrees, positive leans right as in Godot), x/y scale and lift (sprite px) at `time` in one cycle."""
    step, local = divmod(time, HALF)
    k = sine_in_out(local / HALF)
    start = sway if int(step) % 2 == 0 else -sway
    angle = start + (-2 * start) * k
    mid = math.sin(math.pi * local / HALF)  # 0 at the sides, 1 mid-swing
    side = 1 - mid
    return angle, 1 + STRETCH * side - STRETCH * 0.5 * mid, 1 + STRETCH * mid - STRETCH * side, HOP * mid


def place(sprite: Image.Image, time: float, sway: float, size: tuple[int, int], pivot: tuple[float, float],
          scale: float) -> Image.Image:
    """The posed sprite on a transparent `size` canvas, its ground point at `pivot`, drawn at `scale`."""
    angle, sx, sy, lift = pose(time, sway)
    a = math.radians(angle)
    cos, sin = math.cos(a), math.sin(a)
    ss = 2  # supersampling
    px, py = pivot[0] * ss, (pivot[1] - lift * scale) * ss
    k = scale * ss
    # Forward: out = pivot + R * S * k * (p - PIVOT); PIL's AFFINE takes the inverse (output point -> sprite point).
    m = (cos / (sx * k), sin / (sx * k), -sin / (sy * k), cos / (sy * k))
    ox = PIVOT[0] - (m[0] * px + m[1] * py)
    oy = PIVOT[1] - (m[2] * px + m[3] * py)
    big = sprite.transform((size[0] * ss, size[1] * ss), Image.AFFINE, (m[0], m[1], ox, m[2], m[3], oy),
                           resample=Image.BICUBIC)
    return big.resize(size, Image.LANCZOS)


def times() -> list[float]:
    return [i / FPS for i in range(round(2 * HALF * FPS))]


def alone(sprite: Image.Image) -> None:
    w, h = sprite.size
    size = (round(w * ALONE_SCALE), round((h + ALONE_PAD_TOP) * ALONE_SCALE))
    pivot = (PIVOT[0] * ALONE_SCALE, (PIVOT[1] + ALONE_PAD_TOP) * ALONE_SCALE)
    out = []
    for t in times():
        rgba = place(sprite, t, ALONE_SWAY, size, pivot, ALONE_SCALE)
        flat = Image.alpha_composite(Image.new("RGBA", size, MATTE + (255,)), rgba).convert("RGB")
        pal = flat.quantize(colors=255, method=Image.MEDIANCUT, dither=Image.Dither.NONE)
        pal.paste(255, mask=rgba.getchannel("A").point(lambda v: 255 if v < ALPHA_CUT else 0))  # 255: transparent
        out.append(pal)
    path = IMAGES / "spoon-waddle.gif"
    out[0].save(path, save_all=True, append_images=out[1:], duration=round(1000 / FPS), loop=0, transparency=255,
                disposal=2, optimize=False)
    print(f"{len(out)} frames, {size[0]}x{size[1]} -> {path.relative_to(REPO)}")


def last_item(row: Image.Image) -> tuple[int, int, int, int]:
    """The bounding box of the row's rightmost item (items are separated by fully transparent columns)."""
    alpha = row.getchannel("A")
    x1 = alpha.getbbox()[2]
    x0 = x1
    while x0 > 0 and alpha.crop((x0 - 1, 0, x0, row.height)).getbbox():
        x0 -= 1
    box = alpha.crop((x0, 0, x1, row.height)).getbbox()
    return x0, box[1], x1, box[3]


def showcase(sprite: Image.Image, lang: str) -> None:
    row = Image.open(IMAGES / f"spoon-cards-{lang}.png").convert("RGBA")
    x0, y0, x1, y1 = last_item(row)
    trim = sprite.getbbox()
    scale = (y1 - y0) / (trim[3] - trim[1])
    base = Image.new("RGBA", (row.width, row.height + SHOWCASE_PAD_TOP), (0, 0, 0, 0))
    base.alpha_composite(row, (0, SHOWCASE_PAD_TOP))
    base.paste((0, 0, 0, 0), (x0, y0 + SHOWCASE_PAD_TOP, x1, y1 + SHOWCASE_PAD_TOP))  # the static Spoon
    pivot = (x0 + (PIVOT[0] - trim[0]) * scale, y0 + SHOWCASE_PAD_TOP + (PIVOT[1] - trim[1]) * scale)
    background = Image.alpha_composite(Image.new("RGBA", base.size, MATTE + (255,)), base)
    out = []
    palette = None
    for t in times():
        rgb = Image.alpha_composite(background, place(sprite, t, SHOWCASE_SWAY, base.size, pivot, scale)).convert("RGB")
        if palette is None:  # one palette for every frame, so unchanged pixels stay identical and frames stay small
            palette = rgb.quantize(colors=256, method=Image.MEDIANCUT, dither=Image.Dither.NONE)
        out.append(rgb.quantize(palette=palette, dither=Image.Dither.NONE))
    path = IMAGES / f"spoon-cards-{lang}.gif"
    out[0].save(path, save_all=True, append_images=out[1:], duration=round(1000 / FPS), loop=0, disposal=1,
                optimize=False)
    print(f"{len(out)} frames, {base.width}x{base.height} -> {path.relative_to(REPO)}")


def main() -> None:
    sprite = Image.open(SPRITE).convert("RGBA")
    alone(sprite)
    for lang in ("en", "fr"):
        showcase(sprite, lang)


if __name__ == "__main__":
    main()
