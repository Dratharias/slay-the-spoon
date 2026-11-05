import json
import math
import shutil

import pytest
from PIL import Image

from conftest import FIXTURES

import ending_art

FIXTURE = FIXTURES / "ending_art"
SKELETON = FIXTURE / "skeleton.json"
ATLAS = FIXTURE / "skeleton.atlas"


def approx(a, b, tol=1e-6):
    if isinstance(a, tuple):
        return all(math.isclose(x, y, abs_tol=tol) for x, y in zip(a, b, strict=True))
    return math.isclose(a, b, abs_tol=tol)


def skel(bones):
    return {"bones": bones}


PARENT_ROT_90 = [
    {"name": "root"},
    {"name": "parent", "parent": "root", "rotation": 90},
    {"name": "child", "parent": "parent", "x": 10},
]


def test_child_bone_inherits_parent_rotation():
    world = ending_art.bone_world(skel(PARENT_ROT_90))
    assert approx(world["child"].x, 0) and approx(world["child"].y, 10)
    assert approx(world["child"].rotation, 90)


def test_child_bone_ignores_parent_rotation_when_inherit_rotation_false():
    bones = [dict(b) for b in PARENT_ROT_90]
    bones[2]["inheritRotation"] = False
    world = ending_art.bone_world(skel(bones))
    assert approx(world["child"].x, 0) and approx(world["child"].y, 10)
    assert approx(world["child"].rotation, 0)


def test_child_bone_ignores_parent_scale_when_inherit_scale_false():
    bones = [
        {"name": "root"},
        {"name": "parent", "parent": "root", "scaleX": 2},
        {"name": "child", "parent": "parent", "x": 10, "inheritScale": False},
    ]
    world = ending_art.bone_world(skel(bones))
    # The position still goes through the parent's scale; the child's own axes do not.
    assert approx((world["child"].x, world["child"].y), (20, 0))
    assert approx((world["child"].a, world["child"].c), (1, 0))


def test_weighted_mesh_vertex_is_weight_sum():
    data = json.loads(SKELETON.read_text())
    bones = list(ending_art.bone_world(data).values())
    # root is identity at (0, 0); arm sits at (10, 0) rotated 90°, so its local (1, 0) is world (10, 1).
    # Half of (1, 0) plus half of (10, 1) is (5.5, 0.5).
    assert approx(ending_art.mesh_world_vertex(bones, [2, 0, 1, 0, .5, 1, 1, 0, .5]), (5.5, 0.5))


def test_rotated_atlas_region_uvs():
    regions = ending_art.parse_atlas(ATLAS)
    bar = regions["bar"]
    assert bar.rotate and (bar.x, bar.y, bar.width, bar.height) == (9, 1, 4, 6)
    # Stored 90° counter-clockwise: the image's top-left corner sits at the page's bottom-left,
    # its top-right at the page's top-left. The page rectangle is 6 wide and 4 tall.
    assert ending_art.region_uv(bar, (0, 0)) == (9, 5)
    assert ending_art.region_uv(bar, (1, 0)) == (9, 1)
    assert ending_art.region_uv(bar, (1, 1)) == (15, 1)
    square = regions["square"]
    assert ending_art.region_uv(square, (1, 1)) == (7, 7)


def test_untrimmed_region_fills_its_attachment():
    root = ending_art.bone_world(skel([{"name": "root"}]))["root"]
    att = {"width": 10, "height": 20}
    full = ending_art.Region("r", "p", False, 0, 0, 10, 20, 10, 20, 0, 0)
    assert ending_art._region_world(root, att, full) == ending_art._region_world(root, att)
    expected = [(-5, -10), (-5, 10), (5, 10), (5, -10)]
    assert all(approx(c, e) for c, e in zip(ending_art._region_world(root, att), expected, strict=True))


def test_trimmed_region_covers_only_its_packed_area(tmp_path):
    atlas = tmp_path / "t.atlas"
    atlas.write_text("page.png\nsize: 64,64\nformat: RGBA8888\nfilter: Linear,Linear\nrepeat: none\n"
                     "trimmed\n  rotate: false\n  xy: 1, 1\n  size: 6, 4\n  orig: 10, 20\n  offset: 2, 3\n  index: -1\n")
    region = ending_art.parse_atlas(atlas)["trimmed"]
    assert (region.orig_width, region.orig_height, region.offset_x, region.offset_y) == (10, 20, 2, 3)
    root = ending_art.bone_world(skel([{"name": "root"}]))["root"]
    # Attachment drawn at twice the image size: the packed 6×4 area starts 2 right and 3 up from the bottom-left.
    corners = ending_art._region_world(root, {"width": 20, "height": 40}, region)
    assert all(approx(c, e) for c, e in zip(corners, [(-6, -14), (-6, -6), (6, -6), (6, -14)], strict=True))


def test_render_is_deterministic(tmp_path):
    a, b = tmp_path / "a.png", tmp_path / "b.png"
    ending_art.render(SKELETON, ATLAS, a)
    ending_art.render(SKELETON, ATLAS, b)
    assert a.read_bytes() == b.read_bytes()


def test_render_trims_to_alpha(tmp_path):
    out = tmp_path / "out.png"
    x, y, w, h = ending_art.render(SKELETON, ATLAS, out)
    img = Image.open(out)
    assert img.mode == "RGBA"
    assert img.size == (w, h) and img.getbbox() == (4, 4, w - 4, h - 4)


def test_render_places_parts_from_the_setup_pose(tmp_path):
    out = tmp_path / "out.png"
    x, y, w, h = ending_art.render(SKELETON, ATLAS, out)
    img = Image.open(out)

    def at(wx, wy):
        return img.getpixel((math.floor(x + wx), math.floor(y - wy)))

    # The region spans world x -3..3, y 0..6 (top half red, bottom half yellow: Y is flipped once).
    assert at(0, 4.5) == (255, 0, 0, 255)
    assert at(0, 1.5) == (255, 255, 0, 255)
    # The weighted mesh spans world x 8..12, y 0..6 upright (rotated atlas region: top green, bottom blue).
    assert at(10, 4.5) == (0, 255, 0, 255)
    assert at(10, 1.5) == (0, 0, 255, 255)
    # Nothing between the two parts.
    assert at(5.5, 3)[3] == 0


def test_render_scale_scales_the_image(tmp_path):
    x1, y1, w1, h1 = ending_art.render(SKELETON, ATLAS, tmp_path / "one.png")
    x2, y2, w2, h2 = ending_art.render(SKELETON, ATLAS, tmp_path / "two.png", scale=2.0)
    assert abs((w2 - 8) - 2 * (w1 - 8)) <= 2 and abs((h2 - 8) - 2 * (h1 - 8)) <= 2


def test_slot_color_alpha_multiplies(tmp_path):
    data = json.loads(SKELETON.read_text())
    data["slots"][0]["color"] = "ffffff80"
    folder = tmp_path / "skel"
    folder.mkdir()
    (folder / "skeleton.json").write_text(json.dumps(data))
    shutil.copy(ATLAS, folder / "skeleton.atlas")
    shutil.copy(FIXTURE / "fixture.png", folder / "fixture.png")
    out = tmp_path / "out.png"
    x, y, w, h = ending_art.render(folder / "skeleton.json", folder / "skeleton.atlas", out)
    alpha = Image.open(out).getpixel((math.floor(x), math.floor(y - 4.5)))[3]
    assert abs(alpha - 128) <= 1


def test_path_attachments_draw_nothing(tmp_path):
    data = {
        "skeleton": {"spine": "3.4.02"},
        "bones": [{"name": "root"}],
        "slots": [{"name": "rail", "bone": "root", "attachment": "rail"}],
        "skins": {"default": {"rail": {"rail": {"type": "path", "vertexCount": 3,
                                                "vertices": [0, 0, 5, 5, 10, 0], "lengths": [10]}}}},
    }
    folder = tmp_path / "skel"
    folder.mkdir()
    (folder / "skeleton.json").write_text(json.dumps(data))
    shutil.copy(ATLAS, folder / "skeleton.atlas")
    shutil.copy(FIXTURE / "fixture.png", folder / "fixture.png")
    with pytest.raises(ValueError, match="nothing to draw"):
        ending_art.render(folder / "skeleton.json", folder / "skeleton.atlas", tmp_path / "out.png")


def test_cli_renders_monsters_and_copies_map_icons(tmp_path, capsys):
    assets = tmp_path / "assets"
    for rel, name in [("images/npcs/heart", "heart"),
                      ("images/monsters/theEnding/shield", "shield"),
                      ("images/monsters/theEnding/spear", "spear")]:
        folder = assets / rel
        folder.mkdir(parents=True)
        shutil.copy(SKELETON, folder / "skeleton.json")
        shutil.copy(ATLAS, folder / "skeleton.atlas")
        shutil.copy(FIXTURE / "fixture.png", folder / "fixture.png")
    for rel in ["images/ui/map/boss", "images/ui/map/bossOutline"]:
        (assets / rel).mkdir(parents=True)
        Image.new("RGBA", (2, 2), (1, 2, 3, 4)).save(assets / rel / "heart.png")
    (assets / "endingScene").mkdir(parents=True)
    page = Image.new("RGB", (2048, 2048), (0, 0, 0))
    page.paste((9, 8, 7), (2, 2, 1922, 1138))
    page.save(assets / "endingScene" / "scene.jpg", quality=100, subsampling=0)
    for name in ["scene2.jpg", "scene3.jpg"]:
        page.save(assets / "endingScene" / name)
    audio = assets / "audio/music"
    audio.mkdir(parents=True)
    for name in ["STS_Act4_BGM_v2.ogg", "STS_Boss4_v6.ogg", "STS_EndingStinger_v1.ogg"]:
        (audio / name).write_bytes(name.encode())
    out = tmp_path / "SlaySpoon/images/ending"
    ending_art.main(["--assets", str(assets), "--out", str(out)])
    # StS1 packs the room image at (2, 2) of a 2048 page: the combat background is that 1920 x 1136 region.
    bg = Image.open(out / "combat_bg.jpg")
    assert bg.size == (1920, 1136)
    assert all(abs(a - b) <= 2 for a, b in zip(bg.getpixel((960, 568)), (9, 8, 7)))
    lines = [json.loads(line) for line in capsys.readouterr().out.splitlines()]
    assert [line["name"] for line in lines] == ["heart", "shield", "spear"]
    assert set(lines[0]) == {"name", "origin_x", "origin_y", "width", "height"}
    assert sorted(p.name for p in out.iterdir()) == [
        "campfire_bg.jpg", "combat_bg.jpg", "event_bg.jpg", "heart.png", "heart_boss.png", "heart_boss_outline.png", "shield.png", "spear.png"]
    assert Image.open(out / "shield.png").size == (lines[1]["width"], lines[1]["height"])

    for source, target in [("STS_Act4_BGM_v2.ogg", "ending_act"), ("STS_Boss4_v6.ogg", "ending_heart"), ("STS_EndingStinger_v1.ogg", "ending_stinger")]:
        assert (out.parents[1] / "audio" / f"{target}.ogg").read_bytes() == (audio / source).read_bytes()
