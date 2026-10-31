"""Render static sprites of StS1's Act 4 monsters from their Spine 3.4 setup pose.

Usage: python ending_art.py --assets data/sts1/act4/assets --out godot/SlaySpoon/images/ending

Writes combat_bg.jpg (StS1's Ending combat background), heart.png, shield.png and spear.png (trimmed RGBA, setup pose, no constraints applied), copies the
Heart's map icons to heart_boss.png and heart_boss_outline.png, and prints one JSON line per monster:
{"name", "origin_x", "origin_y", "width", "height"}. The origin is the pixel of the skeleton's origin
(where the monster stands), counted from the image's top-left corner.

Spine conventions: Y points up in skeleton space and is flipped once when writing pixels; bone rotations
are degrees counter-clockwise; an atlas region with `rotate: true` is stored 90° counter-clockwise.
"""
import argparse
import json
import math
import shutil
from dataclasses import dataclass
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw

MARGIN = 4

MONSTERS = [
    ("heart", "images/npcs/heart"),
    ("shield", "images/monsters/theEnding/shield"),
    ("spear", "images/monsters/theEnding/spear"),
]
MAP_ICONS = [
    ("images/ui/map/boss/heart.png", "heart_boss.png"),
    ("images/ui/map/bossOutline/heart.png", "heart_boss_outline.png"),
]


@dataclass
class Bone:
    """A bone's world transform: (a b; c d) maps bone-local axes to world, (x, y) is its world position."""
    name: str
    x: float
    y: float
    a: float
    b: float
    c: float
    d: float
    rotation_local: float = 0.0
    scale_x: float = 1.0
    scale_y: float = 1.0
    inherit_rotation: bool = True
    inherit_scale: bool = True
    parent: "Bone | None" = None

    @property
    def rotation(self) -> float:
        return math.degrees(math.atan2(self.c, self.a))

    def to_world(self, lx: float, ly: float) -> tuple[float, float]:
        return self.a * lx + self.b * ly + self.x, self.c * lx + self.d * ly + self.y


def _cos(deg):
    return math.cos(math.radians(deg))


def _sin(deg):
    return math.sin(math.radians(deg))


def bone_world(skeleton: dict) -> dict[str, Bone]:
    """World transforms of every bone in the setup pose, in skeleton order (parents come first)."""
    world: dict[str, Bone] = {}
    for data in skeleton["bones"]:
        x, y = data.get("x", 0.0), data.get("y", 0.0)
        rot = data.get("rotation", 0.0)
        sx, sy = data.get("scaleX", 1.0), data.get("scaleY", 1.0)
        shx, shy = data.get("shearX", 0.0), data.get("shearY", 0.0)
        la, lb = _cos(rot + shx) * sx, _cos(rot + 90 + shy) * sy
        lc, ld = _sin(rot + shx) * sx, _sin(rot + 90 + shy) * sy
        bone = Bone(data["name"], x, y, la, lb, lc, ld, rot, sx, sy,
                    data.get("inheritRotation", True), data.get("inheritScale", True))
        parent = world.get(data["parent"]) if "parent" in data else None
        bone.parent = parent
        if parent is not None:
            bone.x, bone.y = parent.to_world(x, y)
            if bone.inherit_rotation and bone.inherit_scale:
                pa, pb, pc, pd = parent.a, parent.b, parent.c, parent.d
            elif bone.inherit_rotation:
                # Rotation only: accumulate the ancestors' local rotations, stopping after the first
                # ancestor that does not inherit rotation itself.
                pa, pb, pc, pd = 1.0, 0.0, 0.0, 1.0
                p = parent
                while p is not None:
                    cos, sin = _cos(p.rotation_local), _sin(p.rotation_local)
                    pa, pb = pa * cos + pb * sin, pb * cos - pa * sin
                    pc, pd = pc * cos + pd * sin, pd * cos - pc * sin
                    if not p.inherit_rotation:
                        break
                    p = p.parent
            elif bone.inherit_scale:
                # Scale only: apply each ancestor's rotated scale, then undo its rotation.
                pa, pb, pc, pd = 1.0, 0.0, 0.0, 1.0
                p = parent
                while p is not None:
                    cos, sin = _cos(p.rotation_local), _sin(p.rotation_local)
                    psx, psy = p.scale_x, p.scale_y
                    za, zb, zc, zd = cos * psx, sin * psy, sin * psx, cos * psy
                    pa, pb = pa * za + pb * zc, pb * zd - pa * zb
                    pc, pd = pc * za + pd * zc, pd * zd - pc * zb
                    if psx >= 0:
                        sin = -sin
                    pa, pb = pa * cos + pb * sin, pb * cos - pa * sin
                    pc, pd = pc * cos + pd * sin, pd * cos - pc * sin
                    if not p.inherit_scale:
                        break
                    p = p.parent
            else:
                pa, pb, pc, pd = 1.0, 0.0, 0.0, 1.0
            bone.a, bone.b = pa * la + pb * lc, pa * lb + pb * ld
            bone.c, bone.d = pc * la + pd * lc, pc * lb + pd * ld
        world[bone.name] = bone
    return world


def mesh_world_vertex(bones: list[Bone], data: list[float]) -> tuple[float, float]:
    """One weighted vertex: boneCount, then (boneIndex, x, y, weight) per bone; the world sum."""
    wx = wy = 0.0
    for i in range(int(data[0])):
        index, lx, ly, weight = data[1 + 4 * i: 5 + 4 * i]
        px, py = bones[int(index)].to_world(lx, ly)
        wx += px * weight
        wy += py * weight
    return wx, wy


def mesh_world_vertices(bones: list[Bone], bone: Bone, attachment: dict) -> list[tuple[float, float]]:
    vertices, count = attachment["vertices"], len(attachment["uvs"]) // 2
    if len(vertices) == len(attachment["uvs"]):
        return [bone.to_world(vertices[2 * i], vertices[2 * i + 1]) for i in range(count)]
    out, i = [], 0
    for _ in range(count):
        n = int(vertices[i])
        out.append(mesh_world_vertex(bones, vertices[i: i + 1 + 4 * n]))
        i += 1 + 4 * n
    return out


@dataclass
class Region:
    name: str
    page: str
    rotate: bool
    x: int
    y: int
    width: int   # unrotated (as drawn) size
    height: int


def parse_atlas(path: Path) -> dict[str, Region]:
    """libGDX/Spine 3.4 text atlas: pages separated by blank lines, regions with indented properties."""
    regions: dict[str, Region] = {}
    page = None
    current = None
    expect_page = True
    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line:
            expect_page, current = True, None
            continue
        if ":" not in line:
            if expect_page:
                page, expect_page = line, False
            else:
                current = Region(line, page, False, 0, 0, 0, 0)
                regions[line] = current
            continue
        key, value = (s.strip() for s in line.split(":", 1))
        if current is None:
            continue  # page properties
        if key == "rotate":
            current.rotate = value == "true"
        elif key == "xy":
            current.x, current.y = (int(v) for v in value.split(","))
        elif key == "size":
            current.width, current.height = (int(v) for v in value.split(","))
    return regions


def region_uv(region: Region, uv: tuple[float, float]) -> tuple[float, float]:
    """Page pixel of a point given in the region's own normalized coordinates (0..1, v down)."""
    u, v = uv
    if region.rotate:
        # The page rectangle is height wide and width tall; the image was turned 90° counter-clockwise.
        return region.x + v * region.height, region.y + (1 - u) * region.width
    return region.x + u * region.width, region.y + v * region.height


def _color(hex_rgba: str | None) -> tuple[float, float, float, float]:
    if not hex_rgba:
        return 1.0, 1.0, 1.0, 1.0
    return tuple(int(hex_rgba[i: i + 2], 16) / 255 for i in range(0, 8, 2))


def _region_world(bone: Bone, att: dict) -> list[tuple[float, float]]:
    """Corners BL, TL, TR, BR of a region attachment, in world space."""
    w, h = att["width"] * att.get("scaleX", 1.0), att["height"] * att.get("scaleY", 1.0)
    cos, sin = _cos(att.get("rotation", 0.0)), _sin(att.get("rotation", 0.0))
    ox, oy = att.get("x", 0.0), att.get("y", 0.0)
    corners = []
    for lx, ly in [(-w / 2, -h / 2), (-w / 2, h / 2), (w / 2, h / 2), (w / 2, -h / 2)]:
        corners.append(bone.to_world(lx * cos - ly * sin + ox, lx * sin + ly * cos + oy))
    return corners


def _parts(skeleton: dict, regions: dict[str, Region]) -> list[dict]:
    """Drawable slots in draw order: world vertices, page vertices, triangles, tint, blend."""
    world = bone_world(skeleton)
    bones = list(world.values())
    skin = skeleton.get("skins", {}).get("default", {})
    parts = []
    for slot in skeleton["slots"]:
        name = slot.get("attachment")
        if not name:
            continue
        att = skin.get(slot["name"], {}).get(name)
        if att is None:
            continue
        kind = att.get("type", "region")
        if kind not in ("region", "mesh"):
            continue  # path, boundingbox and other non-drawn attachments
        region = regions[att.get("path", name)]
        bone = world[slot["bone"]]
        if kind == "region":
            verts = _region_world(bone, att)
            uvs = [region_uv(region, uv) for uv in [(0, 1), (0, 0), (1, 0), (1, 1)]]
            triangles = [(0, 1, 2), (2, 3, 0)]
        else:
            verts = mesh_world_vertices(bones, bone, att)
            raw = att["uvs"]
            uvs = [region_uv(region, (raw[2 * i], raw[2 * i + 1])) for i in range(len(raw) // 2)]
            t = att["triangles"]
            triangles = [tuple(t[i: i + 3]) for i in range(0, len(t), 3)]
        sr, sg, sb, sa = _color(slot.get("color"))
        ar, ag, ab, aa = _color(att.get("color"))
        parts.append({"page": region.page, "verts": verts, "uvs": uvs, "triangles": triangles,
                      "tint": (sr * ar, sg * ag, sb * ab, sa * aa), "blend": slot.get("blend", "normal")})
    return parts


def _affine(dst, src):
    """Coefficients (a, b, c, d, e, f) with src = (a*x + b*y + c, d*x + e*y + f) for each dst point."""
    (x0, y0), (x1, y1), (x2, y2) = dst
    det = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0)
    if abs(det) < 1e-9:
        return None
    coeffs = []
    for k in (0, 1):
        s0, s1, s2 = src[0][k], src[1][k], src[2][k]
        a = ((s1 - s0) * (y2 - y0) - (s2 - s0) * (y1 - y0)) / det
        b = ((s2 - s0) * (x1 - x0) - (s1 - s0) * (x2 - x0)) / det
        coeffs += [a, b, s0 - a * x0 - b * y0]
    return tuple(coeffs)


def _draw_part(part, page: Image.Image, size, to_px) -> Image.Image:
    """The part alone on a transparent premultiplied (RGBa) layer; triangles replace, never stack."""
    layer = Image.new("RGBa", size, (0, 0, 0, 0))
    px = [to_px(v) for v in part["verts"]]
    for tri in part["triangles"]:
        dst = [px[i] for i in tri]
        src = [part["uvs"][i] for i in tri]
        x0, y0 = math.floor(min(p[0] for p in dst)), math.floor(min(p[1] for p in dst))
        x1, y1 = math.ceil(max(p[0] for p in dst)), math.ceil(max(p[1] for p in dst))
        local = [(p[0] - x0, p[1] - y0) for p in dst]
        coeffs = _affine(local, src)
        if coeffs is None:
            continue
        w, h = max(1, x1 - x0), max(1, y1 - y0)
        tile = page.transform((w, h), Image.AFFINE, coeffs, resample=Image.BILINEAR)
        mask = Image.new("L", (w, h), 0)
        ImageDraw.Draw(mask).polygon(local, fill=255)
        layer.paste(tile, (x0, y0), mask)
    r, g, b, a = part["tint"]
    if (r, g, b, a) != (1, 1, 1, 1):
        # Premultiplied: every channel scales by the alpha too.
        bands = [band.point(lambda v, f=f: round(v * f)) for band, f in
                 zip(layer.split(), (r * a, g * a, b * a, a))]
        layer = Image.merge("RGBa", bands)
    return layer


def render(skeleton: Path, atlas: Path, out: Path, scale: float = 1.0) -> tuple[int, int, int, int]:
    """Draw the setup pose to a trimmed RGBA PNG; return (origin x, origin y, width, height) in pixels."""
    data = json.loads(Path(skeleton).read_text(encoding="utf-8"))
    regions = parse_atlas(Path(atlas))
    parts = _parts(data, regions)
    if not parts:
        raise ValueError(f"{skeleton}: nothing to draw")
    xs = [v[0] * scale for p in parts for v in p["verts"]]
    ys = [v[1] * scale for p in parts for v in p["verts"]]
    pad = MARGIN + 2
    # World (0, 0) lands on an integer pixel corner so the origin stays exact.
    ox, oy = math.ceil(-min(xs)) + pad, math.ceil(max(ys)) + pad
    size = (ox + math.ceil(max(xs)) + pad, oy + math.ceil(-min(ys)) + pad)

    def to_px(v):
        return ox + v[0] * scale, oy - v[1] * scale

    pages = {}
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    for part in parts:
        if part["page"] not in pages:
            pages[part["page"]] = Image.open(Path(atlas).parent / part["page"]).convert("RGBA").convert("RGBa")
        layer = _draw_part(part, pages[part["page"]], size, to_px)
        if part["blend"] == "additive":
            # Additive: premultiplied colour adds onto the canvas; coverage is the larger alpha.
            base = canvas.convert("RGBa").split()
            add = layer.split()
            rgb = [ImageChops.add(base[i], add[i]) for i in range(3)]
            alpha = ImageChops.lighter(base[3], add[3])
            canvas = Image.merge("RGBa", (*rgb, alpha)).convert("RGBA")
        else:
            canvas.alpha_composite(layer.convert("RGBA"))
    bbox = canvas.getbbox()
    if bbox is None:
        raise ValueError(f"{skeleton}: nothing to draw")
    left, top = bbox[0] - MARGIN, bbox[1] - MARGIN
    trimmed = canvas.crop((left, top, bbox[2] + MARGIN, bbox[3] + MARGIN))
    out = Path(out)
    out.parent.mkdir(parents=True, exist_ok=True)
    trimmed.save(out, optimize=True)
    return ox - left, oy - top, trimmed.width, trimmed.height


# StS1's TheEndingScene reads three regions of endingScene/scene*.jpg; "bg" is the combat background, packed at
# (2, 2) of a 2048 page and 1920 x 1136 big (scene.atlas).
SCENE_SRC = "endingScene/scene.jpg"
SCENE_BOX = (2, 2, 1922, 1138)


def write_combat_bg(assets: Path, out: Path):
    with Image.open(assets / SCENE_SRC) as page:
        page.convert("RGB").crop(SCENE_BOX).save(out / "combat_bg.jpg", quality=95)


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("--assets", type=Path, required=True)
    ap.add_argument("--out", type=Path, required=True)
    args = ap.parse_args(argv)
    args.out.mkdir(parents=True, exist_ok=True)
    for name, folder in MONSTERS:
        src = args.assets / folder
        x, y, w, h = render(src / "skeleton.json", src / "skeleton.atlas", args.out / f"{name}.png")
        print(json.dumps({"name": name, "origin_x": x, "origin_y": y, "width": w, "height": h}))
    for src, dst in MAP_ICONS:
        shutil.copyfile(args.assets / src, args.out / dst)
    write_combat_bg(args.assets, args.out)
    for page, name in [("scene2.jpg", "campfire_bg"), ("scene3.jpg", "event_bg")]:
        with Image.open(args.assets / "endingScene" / page) as source:
            source.convert("RGB").crop(SCENE_BOX).save(args.out / f"{name}.jpg", quality=95)
    audio = args.out.parents[1] / "audio"
    audio.mkdir(parents=True, exist_ok=True)
    for source, target in [("STS_Act4_BGM_v2.ogg", "ending_act"), ("STS_Boss4_v6.ogg", "ending_heart"),
                           ("STS_EndingStinger_v1.ogg", "ending_stinger")]:
        shutil.copyfile(args.assets / "audio/music" / source, audio / f"{target}.ogg")


if __name__ == "__main__":
    main()
