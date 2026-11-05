"""Export StS1 card portraits for the cards that ship as StS1 classes.

Usage: python art.py --jar desktop-1.0.jar --sts1 data/sts1/ironclad.json
           --table data/duplicates/ironclad.json --out godot/SlaySpoon/images/cards/ironclad
       python art.py --jar desktop-1.0.jar --events godot/SlaySpoon/images/events
       python art.py --jar desktop-1.0.jar --watcher godot/SlaySpoon
       python art.py --jar desktop-1.0.jar --powers godot/SlaySpoon/images/powers

StS1's 1024Portraits are 500×380; StS2 draws card art from 250×190 atlas sprites (same aspect).
Event art (step 3d) keeps StS1's 600×600 size.

--watcher (step 7) writes the Watcher's static visuals under the mod's PCK root, at the vanilla relative paths
with the id sts1_watcher: the idle skeleton's setup pose (body.png), her corpse and eye frames as-is, the top bar
head icon (cut from a 2x render of that pose) and its outline, the character select icons, the map marker, the five energy orb layers and the starter card
portraits. It prints one JSON line with the body's origin and the eye anchor, in body.png pixels.
"""
import argparse
import io
import json
import tempfile
import zipfile
from pathlib import Path

from PIL import Image, ImageFilter, ImageOps

import codegen
import ending_art
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


WATCHER = "images/characters/watcher/"
WATCHER_ID = "sts1_watcher"
# Vanilla sizes, read from the .ctex headers of the Ironclad counterparts in SlayTheSpire2.pck.
ICON_SIZE = (85, 85)
SELECT_SIZE = (132, 195)
MAP_MARKER_SIZE = (49, 64)
ORB_LAYER_SIZE = (256, 256)
# StS1 purple orb layers, back to front, onto the Ironclad energy counter's Layer1..Layer5 (Layer2 and Layer3 spin).
ORB_LAYERS = ["l1", "l2", "l3", "l4", "border"]
# The top bar head: the idle setup pose rendered at HEAD_SCALE, then this box around her head, relative to the
# ground point at that scale (left, top, right, bottom; y grows down).
HEAD_SCALE = 2.0
HEAD_BOX = (-68, -544, 72, -404)
# The character select crop of watcherPortrait.jpg (1920×1200): her head, hands and staff head, 132:195.
SELECT_CROP = (560, 40, 1305, 1141)
# Mod card class -> StS1 portrait (the card constructor's image argument).
WATCHER_STARTER_PORTRAITS = {
    "Sts1StrikeWatcher": "purple/attack/strike",
    "Sts1DefendWatcher": "purple/skill/defend",
    "Sts1Eruption": "purple/attack/eruption",
    "Sts1Vigilance": "purple/skill/vigilance",
}


def _open(z: zipfile.ZipFile, name: str) -> Image.Image:
    return Image.open(io.BytesIO(z.read(name))).convert("RGBA")


def _save(img: Image.Image, path: Path) -> Path:
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path, optimize=True)
    return path


def _fit(img: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Scale to fit inside size, keep the aspect, centre on a transparent canvas."""
    img = img.crop(img.getbbox() or (0, 0, *img.size))
    scale = min(size[0] / img.width, size[1] / img.height)
    img = img.resize((max(1, round(img.width * scale)), max(1, round(img.height * scale))), Image.LANCZOS)
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    canvas.alpha_composite(img, ((size[0] - img.width) // 2, (size[1] - img.height) // 2))
    return canvas


def _outline(icon: Image.Image) -> Image.Image:
    """A white silhouette 2 px wider than the icon, drawn behind it on hover like the vanilla outlines."""
    alpha = icon.getchannel("A").filter(ImageFilter.MaxFilter(5))
    out = Image.new("RGBA", icon.size, (255, 255, 255, 0))
    out.putalpha(alpha)
    return out


def _region(page: Image.Image, region: ending_art.Region) -> Image.Image:
    if region.rotate:
        # Stored 90° counter-clockwise, so the page rectangle is height wide and width tall.
        tile = page.crop((region.x, region.y, region.x + region.height, region.y + region.width))
        return tile.rotate(-90, expand=True)
    return page.crop((region.x, region.y, region.x + region.width, region.y + region.height))


def _bake_closed_eye(z, eye_dir: str, tmp: Path, body_path: Path, ox: int, oy: int, ex: float, ey: float):
    """Draw the staff eye's neutral pose (StS1's "None" animation: both lids shut) at the eye anchor of body.png.

    StS1 draws the eye skeleton at the anchor bone's world position, unrotated. The canvas grows if the eye sticks
    out; return the body origin in the new canvas.
    """
    eye = json.loads(z.read(eye_dir + "skeleton.json"))
    for slot in eye["slots"]:
        if slot["name"] in ("lid_lower", "lid_upper"):
            slot["attachment"] = slot["name"]
    (tmp / "eye.json").write_text(json.dumps(eye), encoding="utf-8")
    for name in z.namelist():
        if name.startswith(eye_dir) and name.endswith(".png"):
            (tmp / Path(name).name).write_bytes(z.read(name))
    eox, eoy, _, _ = ending_art.render(tmp / "eye.json", tmp / "eye.atlas", tmp / "eye_pose.png")
    eye_img = Image.open(tmp / "eye_pose.png").convert("RGBA")
    body = Image.open(body_path).convert("RGBA")
    at = (round(ex - eox), round(ey - eoy))
    left, top = min(0, at[0]), min(0, at[1])
    right, bottom = max(body.width, at[0] + eye_img.width), max(body.height, at[1] + eye_img.height)
    canvas = Image.new("RGBA", (right - left, bottom - top), (0, 0, 0, 0))
    canvas.alpha_composite(body, (-left, -top))
    canvas.alpha_composite(eye_img, (at[0] - left, at[1] - top))
    _save(canvas, body_path)
    return ox - left, oy - top


def export_watcher(jar: Path, out: Path) -> dict:
    """Write the Watcher's static visuals under out (the PCK root); return the body metadata."""
    creature = out / "images/creatures" / WATCHER_ID
    with zipfile.ZipFile(jar) as z, tempfile.TemporaryDirectory() as tmp:
        idle = Path(tmp)
        for name in z.namelist():
            if name.startswith(WATCHER + "idle/") and not name.endswith("/"):
                (idle / Path(name).name).write_bytes(z.read(name))
        body_path = creature / "body.png"
        ox, oy, _, _ = ending_art.render(idle / "skeleton.json", idle / "skeleton.atlas", body_path)
        skeleton = json.loads((idle / "skeleton.json").read_text(encoding="utf-8"))
        anchor = ending_art.bone_world(skeleton)["eye_anchor"]
        # Y points up in skeleton space, down in pixels.
        ex, ey = ox + anchor.x, oy - anchor.y

        eye_dir = WATCHER + "eye_anim/"
        (idle / "eye.atlas").write_bytes(z.read(eye_dir + "skeleton.atlas"))
        if eye_dir + "skeleton.json" in z.namelist():
            ox, oy = _bake_closed_eye(z, eye_dir, idle, body_path, ox, oy, ex, ey)
            ex, ey = ox + anchor.x, oy - anchor.y
        body = Image.open(body_path)
        meta = {
            "body": {"origin_x": ox, "origin_y": oy, "width": body.width, "height": body.height},
            "eye_anchor": (round(ex), round(ey)),
        }

        _save(_open(z, WATCHER + "corpse.png"), creature / "corpse.png")
        pages = {}
        for name, region in sorted(ending_art.parse_atlas(idle / "eye.atlas").items()):
            if region.page not in pages:
                pages[region.page] = _open(z, eye_dir + region.page)
            _save(_region(pages[region.page], region), creature / f"eye_{name}.png")

        # Vanilla top bar icons are the character's head, so cut hers from a sharper render of the idle pose.
        hx, hy, _, _ = ending_art.render(idle / "skeleton.json", idle / "skeleton.atlas", idle / "head.png", scale=HEAD_SCALE)
        left, top, right, bottom = HEAD_BOX
        head = Image.open(idle / "head.png").convert("RGBA").crop((hx + left, hy + top, hx + right, hy + bottom))
        icon = _fit(head, ICON_SIZE)
        button = _open(z, "images/ui/charSelect/watcherButton.png")
        _save(icon, out / f"images/ui/top_panel/character_icon_{WATCHER_ID}.png")
        _save(_outline(icon), out / f"images/ui/top_panel/character_icon_{WATCHER_ID}_outline.png")
        _save(_fit(button, MAP_MARKER_SIZE), out / f"images/packed/map/icons/map_marker_{WATCHER_ID}.png")

        portrait = _open(z, "images/ui/charSelect/watcherPortrait.jpg")
        select = portrait.crop(SELECT_CROP).resize(SELECT_SIZE, Image.LANCZOS)
        select_dir = out / "images/packed/character_select"
        _save(select, select_dir / f"char_select_{WATCHER_ID}.png")
        locked = ImageOps.grayscale(select).point(lambda v: v // 2).convert("RGBA")
        locked.putalpha(select.getchannel("A"))
        _save(locked, select_dir / f"char_select_{WATCHER_ID}_locked.png")

        orb_dir = out / "images/ui/combat/energy_counters" / WATCHER_ID
        for i, layer in enumerate(ORB_LAYERS, start=1):
            img = _open(z, f"images/ui/topPanel/purple/{layer}.png").resize(ORB_LAYER_SIZE, Image.LANCZOS)
            _save(img, orb_dir / f"{WATCHER_ID}_orb_layer_{i}.png")

        for cls, portrait_name in WATCHER_STARTER_PORTRAITS.items():
            img = _open(z, f"images/1024Portraits/{portrait_name}.png").resize(SIZE, Image.LANCZOS)
            _save(img, out / "images/cards/watcher" / f"{match.slug(cls).lower()}.png")

        audio = out / "audio/watcher"
        audio.mkdir(parents=True, exist_ok=True)
        for source, target in WATCHER_STANCE_SOUNDS.items():
            (audio / f"{target}.ogg").write_bytes(z.read(f"audio/sound/watcher/{source}.ogg"))
    return meta


# StS1 stance sounds (SoundMaster keys STANCE_ENTER_* and STANCE_LOOP_*) -> audio/watcher/<name>.ogg
# (src/Core/WatcherAssets.cs builds the same paths).
WATCHER_STANCE_SOUNDS = {
    "STS_SFX_Watcher-Calm_v2": "stance_calm", "STS_SFX_Watcher-CalmLoop_v2": "stance_calm_loop",
    "STS_SFX_Watcher-Wrath_v2": "stance_wrath", "STS_SFX_Watcher-WrathLoop_v2": "stance_wrath_loop",
    "STS_SFX_Watcher-Divinity_v3": "stance_divinity", "STS_SFX_Watcher-DivinityLoop_v2": "stance_divinity_loop",
}


WATCHER_RELICS = {
    "PureWater": "clean_water", "HolyWater": "holy_water", "Damaru": "damaru",
    "TeardropLocket": "tear_drop_locket", "VioletLotus": "violet_lotus", "Melange": "melange",
    "GoldenEye": "golden_eye", "CloakClasp": "clasp", "Duality": "duality",
}


def export_relics(jar: Path, out: Path) -> list:
    written = []
    with zipfile.ZipFile(jar) as z:
        for cls, stem in sorted(WATCHER_RELICS.items()):
            model_id = match.slug("Sts1" + cls).lower()
            for source, suffix in (("relics", ""), ("relics/outline", "_outline"), ("largeRelics", "_large")):
                written.append(_save(_open(z, f"images/{source}/{stem}.png"), out / f"{model_id}{suffix}.png"))
    return written


# StS1 power atlas region -> mod power ModelDb id (src/Core/Sts1PowerIcons.cs lists the same ids).
POWERS = {"talk_to_hand": "sts1_block_return_power"}
POWER_BIG = 128
# Vanilla power atlas sprites are 64×64 frames (region plus margin).
POWER_PACKED = (64, 64)


def _atlas_props(atlas_text: str, name: str) -> dict[str, tuple[int, int]]:
    """orig and offset of one region; ending_art.parse_atlas keeps only xy and size."""
    lines = atlas_text.splitlines()
    start = lines.index(name)
    props = {}
    for line in lines[start + 1:]:
        if ":" not in line:
            break
        key, value = (v.strip() for v in line.split(":", 1))
        if key in ("orig", "offset"):
            props[key] = tuple(int(v) for v in value.split(","))
    return props


def export_powers(jar: Path, out: Path) -> list:
    """Write <power id>.png (128×128, the big icon) and <power id>_packed.png (64×64) for POWERS; return the paths."""
    written = []
    with zipfile.ZipFile(jar) as z, tempfile.TemporaryDirectory() as tmp:
        atlas_path = Path(tmp) / "powers.atlas"
        atlas_path.write_bytes(z.read("powers/powers.atlas"))
        text = atlas_path.read_text(encoding="utf-8")
        regions = ending_art.parse_atlas(atlas_path)
        page = None
        for stem, model_id in sorted(POWERS.items()):
            name = f"{POWER_BIG}/{stem}"
            region = regions[name]
            if page is None:
                page = _open(z, "powers/" + region.page)
            props = _atlas_props(text, name)
            ow, oh = props.get("orig", (region.width, region.height))
            dx, dy = props.get("offset", (0, 0))
            # libGDX offsets count from the bottom-left corner of the untrimmed image.
            canvas = Image.new("RGBA", (ow, oh), (0, 0, 0, 0))
            canvas.alpha_composite(_region(page, region), (dx, oh - dy - region.height))
            big = canvas.resize((POWER_BIG, POWER_BIG), Image.LANCZOS)
            written.append(_save(big, out / f"{model_id}.png"))
            written.append(_save(big.resize(POWER_PACKED, Image.LANCZOS), out / f"{model_id}_packed.png"))
    return written


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--jar", type=Path, required=True)
    ap.add_argument("--events", type=Path)
    ap.add_argument("--relics", type=Path)
    ap.add_argument("--powers", type=Path)
    ap.add_argument("--watcher", type=Path, help="the mod's PCK root (godot/SlaySpoon)")
    ap.add_argument("--sts1", type=Path)
    ap.add_argument("--table", type=Path)
    ap.add_argument("--out", type=Path)
    args = ap.parse_args()
    if args.powers:
        print(f"{len(export_powers(args.jar, args.powers))} power icons")
        return
    if args.relics:
        print(f"{len(export_relics(args.jar, args.relics))} relic images")
        return
    if args.events:
        print(f"{len(export_events(args.jar, args.events))} event images")
        return
    if args.watcher:
        print(json.dumps(export_watcher(args.jar, args.watcher)))
        return
    if not (args.sts1 and args.table and args.out):
        ap.error("--sts1, --table and --out are required without --events or --watcher")
    cards = json.loads(args.sts1.read_text(encoding="utf-8"))
    rows = json.loads(args.table.read_text(encoding="utf-8"))
    print(f"{len(export(args.jar, cards, rows, args.out))} portraits")


if __name__ == "__main__":
    main()
