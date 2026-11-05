"""Tests for the Spine 3.4 -> 4.2 converter and the StS1 skeleton export."""
import pytest

from spine34to42 import (Unsupported, _curve, _wrap, convert, convert_bone, convert_rotate, convert_skins,
                         parse_anim, rename_animations)
from spine_assets import CREATURES, atlas_pages


def test_wrap_brings_deltas_into_half_turn():
    assert _wrap(270) == -90
    assert _wrap(-270) == 90
    assert _wrap(190) == -170
    assert _wrap(10) == 10


def test_rotate_keys_unwrap_along_shortest_arc():
    keys = convert_rotate("b", [{"time": 0, "angle": 170}, {"time": 1, "angle": -170}])
    assert [k["value"] for k in keys] == [170, 190]
    assert "angle" not in keys[0] and keys[1]["time"] == 1


def test_curve_becomes_absolute_control_points_per_channel():
    assert _curve("stepped", 0, 1, [(0, 10)]) == "stepped"
    with pytest.raises(Unsupported):
        _curve(None, 0, 1, [(0, 10)])
    assert _curve([0.25, 0, 0.75, 1], 1, 3, [(0, 10), (5, 5)]) == [1.5, 0, 2.5, 10, 1.5, 5, 2.5, 5]


def test_bone_inherit_flags():
    assert "inherit" not in convert_bone({"name": "a"})
    assert convert_bone({"name": "a", "inheritRotation": False})["inherit"] == "noRotationOrReflection"
    assert convert_bone({"name": "a", "inheritScale": False})["inherit"] == "noScale"
    assert convert_bone({"name": "a", "inheritRotation": False, "inheritScale": False})["inherit"] == "onlyTranslation"


def test_skins_become_a_list():
    skins = convert_skins({"default": {"slot": {"img": {"x": 1}}}})
    assert skins == [{"name": "default", "attachments": {"slot": {"img": {"x": 1}}}}]


def test_unknown_input_is_refused():
    with pytest.raises(Unsupported):
        convert_bone({"name": "a", "transform": "x"})


def test_rename_scales_times_and_drops_unlisted():
    anims = {"Idle": {"bones": {"b": {"rotate": [{"time": 1.4, "value": 0}]}}}, "Other": {}}
    out = rename_animations(anims, [("idle_loop", "Idle", 0.7)])
    assert list(out) == ["idle_loop"]
    assert out["idle_loop"]["bones"]["b"]["rotate"][0]["time"] == pytest.approx(2.0)
    assert anims["Idle"]["bones"]["b"]["rotate"][0]["time"] == 1.4


def test_parse_anim():
    assert parse_anim("idle_loop=Idle@0.7") == ("idle_loop", "Idle", 0.7)
    assert parse_anim("hurt=Hit") == ("hurt", "Hit", 1.0)


def test_convert_minimal_skeleton():
    data = {"skeleton": {"spine": "3.4.02", "width": 1, "height": 1}, "bones": [{"name": "root"}],
            "slots": [], "skins": {"default": {}}, "animations": {"Idle": {}}}
    out = convert(data, [("idle_loop", "Idle", 1.0)])
    assert out["skeleton"]["spine"].startswith("4.2")
    assert list(out["animations"]) == ["idle_loop"]
    assert out["skins"] == [{"name": "default", "attachments": {}}]


def test_atlas_pages():
    text = "\nheart.png\nsize: 2,2\nformat: RGBA8888\nregion\n  rotate: false\n\nheart2.png\nsize: 2,2\n"
    assert atlas_pages(text) == ["heart.png", "heart2.png"]


def test_animator_names_exposed():
    for key, (_, renames) in CREATURES.items():
        if renames is None:
            continue
        names = {new for new, _, _ in renames}
        assert "idle_loop" in names, key
