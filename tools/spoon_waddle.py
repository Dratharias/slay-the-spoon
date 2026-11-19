"""The Spoon's combat idle as a looping GIF with a transparent background: workshop/images/spoon-waddle.gif.

A livelier take on SpoonMotion.Idle with the combat sprite (godot/SlaySpoon/images/spoon/spoon.png): the body rocks from
+SWAY_DEG to -SWAY_DEG around the handle tip (the sprite's ground point, SpoonMonster.Origin) with Sine InOut easing,
hops HOP px as it crosses the middle (stretched), and squashes at each side. One cycle (two steps) is the whole GIF,
so it loops without a seam.

GIF transparency is one bit: edge pixels are blended onto MATTE (Steam's description background) and kept when their
alpha passes ALPHA_CUT, so the outline stays smooth on the Workshop page.

Usage: python3 -I tools/spoon_waddle.py
"""
import math
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[1]
SPRITE = REPO / "godot/SlaySpoon/images/spoon/spoon.png"
OUT = REPO / "workshop/images/spoon-waddle.gif"
PIVOT = (200, 593)  # SpoonMonster.Origin
SWAY_DEG = 8.0
HOP = 18.0  # sprite px at mid-swing
STRETCH = 0.05  # y stretch at mid-swing, x squash at the sides
HALF = 0.6  # seconds per step
FPS = 25
PAD_TOP = 64  # the breath lifts the bowl above the sprite's own frame
SCALE = 0.5  # output size
MATTE = (27, 40, 56)  # Steam's #1b2838
ALPHA_CUT = 96


def sine_in_out(t: float) -> float:
    return -(math.cos(math.pi * t) - 1) / 2


def pose(time: float) -> tuple[float, float, float, float]:
    """Rotation (degrees, positive leans right as in Godot), x/y scale and lift (px) at `time` within one cycle."""
    step, local = divmod(time, HALF)
    k = sine_in_out(local / HALF)
    start = SWAY_DEG if int(step) % 2 == 0 else -SWAY_DEG
    angle = start + (-2 * start) * k
    mid = math.sin(math.pi * local / HALF)  # 0 at the sides, 1 mid-swing
    side = 1 - mid
    return angle, 1 + STRETCH * side - STRETCH * 0.5 * mid, 1 + STRETCH * mid - STRETCH * side, HOP * mid


def frame(sprite: Image.Image, time: float) -> Image.Image:
    angle, sx, sy, lift = pose(time)
    w, h = sprite.size
    canvas = (w, h + PAD_TOP)
    px, py = PIVOT[0], PIVOT[1] + PAD_TOP - lift
    # Inverse map: output point -> sprite point, around the pivot (Godot's y points down, so +angle turns clockwise).
    a = math.radians(angle)
    cos, sin = math.cos(a), math.sin(a)
    # Forward: p' = pivot + R * S * (p - pivot_sprite); inverse: p = pivot_sprite + S^-1 * R^-1 * (p' - pivot).
    m = (cos / sx, sin / sx, 0.0, -sin / sy, cos / sy, 0.0)
    ox = PIVOT[0] - (m[0] * px + m[1] * py)
    oy = PIVOT[1] - (m[3] * px + m[4] * py)
    big = sprite.resize((w * 2, h * 2), Image.LANCZOS)  # transform at 2x, then scale down: smoother edges
    m2 = (m[0], m[1], 2 * ox, m[3], m[4], 2 * oy)
    out = big.transform((canvas[0] * 2, canvas[1] * 2), Image.AFFINE, m2, resample=Image.BICUBIC)
    return out.resize((round(canvas[0] * SCALE), round(canvas[1] * SCALE)), Image.LANCZOS)


def to_gif_frame(rgba: Image.Image) -> Image.Image:
    matte = Image.new("RGBA", rgba.size, MATTE + (255,))
    flat = Image.alpha_composite(matte, rgba).convert("RGB")
    pal = flat.quantize(colors=255, method=Image.MEDIANCUT, dither=Image.Dither.NONE)
    mask = rgba.getchannel("A").point(lambda v: 255 if v < ALPHA_CUT else 0)
    pal.paste(255, mask=mask)  # index 255: transparent
    return pal


def main() -> None:
    sprite = Image.open(SPRITE).convert("RGBA")
    count = round(2 * HALF * FPS)
    frames = [to_gif_frame(frame(sprite, i / FPS)) for i in range(count)]
    frames[0].save(OUT, save_all=True, append_images=frames[1:], duration=round(1000 / FPS), loop=0,
                   transparency=255, disposal=2, optimize=False)
    print(f"{count} frames, {frames[0].size[0]}x{frames[0].size[1]} -> {OUT.relative_to(REPO)}")


if __name__ == "__main__":
    main()
