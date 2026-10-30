import io
import json
import zipfile

from PIL import Image

import art
from conftest import FIXTURES


def make_jar(path, portraits):
    with zipfile.ZipFile(path, "w") as z:
        for name in portraits:
            buf = io.BytesIO()
            Image.new("RGB", (500, 380), (200, 30, 30)).save(buf, "PNG")
            z.writestr(f"images/1024Portraits/{name}.png", buf.getvalue())


CARDS = [
    {"class": "FakeSlash", "snake": "fake_slash", "portrait": "red/attack/fake_slash"},
    {"class": "FakeGuard", "snake": "fakeguard", "portrait": "red/skill/fakeguard"},
]


def test_resizes_to_card_atlas_size(tmp_path):
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/attack/fake_slash", "red/skill/fakeguard"])
    rows = [{"sts1": "FakeSlash", "relation": "unique"}, {"sts1": "FakeGuard", "relation": "duplicate"}]
    written = art.export(jar, CARDS, rows, tmp_path / "out")
    assert sorted(p.name for p in written) == ["sts1_fake_guard.png", "sts1_fake_slash.png"]
    img = Image.open(tmp_path / "out" / "sts1_fake_slash.png")
    assert img.size == (250, 190)
    assert img.mode == "RGBA"


def test_skips_same_sts2(tmp_path):
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/attack/fake_slash", "red/skill/fakeguard"])
    rows = [{"sts1": "FakeSlash", "relation": "same-sts2"}, {"sts1": "FakeGuard", "relation": "same-sts1"}]
    written = art.export(jar, CARDS, rows, tmp_path / "out")
    assert [p.name for p in written] == ["sts1_fake_guard.png"]


def test_named_after_the_model_id_not_the_sts1_art_name(tmp_path):
    # PortraitPath is built from the card's ModelDb id; StS1 art names don't always split words the same way.
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/skill/fakeguard"])
    written = art.export(jar, CARDS[1:], [{"sts1": "FakeGuard", "relation": "unique"}], tmp_path / "out")
    assert [p.name for p in written] == ["sts1_fake_guard.png"]


def test_events_export_named_after_the_event_model_id(tmp_path):
    jar = tmp_path / "game.jar"
    with zipfile.ZipFile(jar, "w") as z:
        for name in ("vampires", "drugDealer", "theNest"):
            buf = io.BytesIO()
            Image.new("RGB", (600, 600), (20, 20, 60)).save(buf, "JPEG")
            z.writestr(f"images/events/{name}.jpg", buf.getvalue())
    written = art.export_events(jar, tmp_path / "out")
    assert sorted(p.name for p in written) == ["sts1_augmenter.png", "sts1_nest.png", "sts1_vampires.png"]
    img = Image.open(tmp_path / "out" / "sts1_nest.png")
    assert img.size == (600, 600)


def _png(size, color=(120, 60, 200, 255), fmt="PNG"):
    buf = io.BytesIO()
    Image.new("RGBA" if fmt == "PNG" else "RGB", size, color[: 4 if fmt == "PNG" else 3]).save(buf, fmt)
    return buf.getvalue()


EYE_ATLAS = """
the_watcher_eye.png
size: 32,16
format: RGBA8888
filter: Linear,Linear
repeat: none
base
  rotate: false
  xy: 1, 1
  size: 5, 4
  orig: 5, 4
  offset: 0, 0
  index: -1
lid_upper
  rotate: true
  xy: 8, 1
  size: 6, 3
  orig: 6, 3
  offset: 0, 0
  index: -1
"""


def make_watcher_jar(path):
    """A fake StS1 jar holding every file art.py --watcher reads (the idle skeleton is the ending_art fixture)."""
    skeleton = json.loads((FIXTURES / "ending_art" / "skeleton.json").read_text(encoding="utf-8"))
    skeleton["bones"].append({"name": "eye_anchor", "parent": "root", "x": 2, "y": 5})
    idle = "images/characters/watcher/idle/"
    with zipfile.ZipFile(path, "w") as z:
        z.writestr(idle + "skeleton.json", json.dumps(skeleton))
        z.write(FIXTURES / "ending_art" / "skeleton.atlas", idle + "skeleton.atlas")
        z.write(FIXTURES / "ending_art" / "fixture.png", idle + "fixture.png")
        z.writestr("images/characters/watcher/eye_anim/skeleton.atlas", EYE_ATLAS)
        z.writestr("images/characters/watcher/eye_anim/the_watcher_eye.png", _png((32, 16)))
        z.writestr("images/characters/watcher/corpse.png", _png((512, 512)))
        z.writestr("images/ui/charSelect/watcherButton.png", _png((200, 200)))
        z.writestr("images/ui/charSelect/watcherPortrait.jpg", _png((1920, 1200), fmt="JPEG"))
        for layer in ("l1", "l2", "l3", "l4", "border"):
            z.writestr(f"images/ui/topPanel/purple/{layer}.png", _png((128, 128)))
        for portrait in art.WATCHER_STARTER_PORTRAITS.values():
            z.writestr(f"images/1024Portraits/{portrait}.png", _png((500, 380)))


# Sizes of the vanilla Ironclad counterparts, read from the StS2 PCK's .ctex headers.
WATCHER_SIZES = {
    "images/ui/top_panel/character_icon_sts1_watcher.png": (85, 85),
    "images/ui/top_panel/character_icon_sts1_watcher_outline.png": (85, 85),
    "images/packed/character_select/char_select_sts1_watcher.png": (132, 195),
    "images/packed/character_select/char_select_sts1_watcher_locked.png": (132, 195),
    "images/packed/map/icons/map_marker_sts1_watcher.png": (49, 64),
    **{f"images/ui/combat/energy_counters/sts1_watcher/sts1_watcher_orb_layer_{i}.png": (256, 256) for i in range(1, 6)},
    "images/creatures/sts1_watcher/corpse.png": (512, 512),
    "images/creatures/sts1_watcher/eye_base.png": (5, 4),
    "images/creatures/sts1_watcher/eye_lid_upper.png": (6, 3),
    "images/cards/watcher/sts1_strike_watcher.png": (250, 190),
    "images/cards/watcher/sts1_defend_watcher.png": (250, 190),
    "images/cards/watcher/sts1_eruption.png": (250, 190),
    "images/cards/watcher/sts1_vigilance.png": (250, 190),
}


def test_watcher_art_outputs(tmp_path):
    jar = tmp_path / "game.jar"
    make_watcher_jar(jar)
    out = tmp_path / "SlaySpoon"
    meta = art.export_watcher(jar, out)
    written = sorted(str(p.relative_to(out)) for p in out.rglob("*.png"))
    assert written == sorted([*WATCHER_SIZES, "images/creatures/sts1_watcher/body.png"])
    for rel, size in WATCHER_SIZES.items():
        img = Image.open(out / rel)
        assert img.size == size, rel
        assert img.mode == "RGBA", rel
    body = Image.open(out / "images/creatures/sts1_watcher/body.png")
    assert (meta["body"]["width"], meta["body"]["height"]) == body.size
    # The eye anchor sits 2 right of and 5 above the skeleton origin (Y up in skeleton space).
    assert meta["eye_anchor"] == (meta["body"]["origin_x"] + 2, meta["body"]["origin_y"] - 5)


def test_watcher_locked_select_icon_is_grey(tmp_path):
    jar = tmp_path / "game.jar"
    make_watcher_jar(jar)
    art.export_watcher(jar, tmp_path / "out")
    r, g, b, _ = Image.open(tmp_path / "out/images/packed/character_select/char_select_sts1_watcher_locked.png").getpixel((66, 97))
    assert r == g == b
