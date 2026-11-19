import ctypes
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import workshop_dependency

REPO = Path(__file__).resolve().parents[2]


def test_callback_struct_matches_steam_packing():
    # AddUGCDependencyResult_t: int32 + two uint64 with 4-byte packing = 20 bytes on Linux.
    assert ctypes.sizeof(workshop_dependency.AddDependencyResult) == 20


def test_manifest_dependency_has_a_required_item():
    manifest = json.loads((REPO / "mod/SlaySpoon.json").read_text(encoding="utf-8"))
    deps = {d["id"]: d for d in manifest["dependencies"]}
    assert "BaseLib" in deps and deps["BaseLib"]["min_version"]
    lines = (REPO / "workshop/required-items").read_text(encoding="utf-8").splitlines()
    items = {m.group(2): m.group(1) for line in lines if (m := re.match(r"^(\d+)\s+(\S+)", line))}
    assert set(items) == set(deps)
    assert items["BaseLib"] == "3737335127"


def test_submit_result_struct_matches_steam_packing():
    # SubmitItemUpdateResult_t: int32 + bool (+3 pad) + uint64 with 4-byte packing = 16 bytes on Linux.
    assert ctypes.sizeof(workshop_dependency.SubmitItemUpdateResult) == 16


# The game's Workshop tags (Steam app 2868840, 2026-11-19): categories and languages.
GAME_TAGS = {"Acts", "Ancients", "Audio", "Cards", "Characters", "Cosmetics", "Events", "Expansion", "Extensions",
             "Humor", "Modifiers", "Monsters", "Potions", "QoL", "Relics", "Rooms", "Tools & APIs", "Utility", "Misc",
             "English", "French"}


def test_tags_file_uses_game_tags_and_both_languages():
    tags = workshop_dependency.read_tags(REPO / "workshop/tags")
    assert tags and set(tags) <= GAME_TAGS
    assert {"English", "French"} <= set(tags)
    assert len(tags) == len(set(tags))


def test_descriptions_fit_steam_limit():
    # Steam caps a Workshop description at 8000 characters, per language.
    for name in ("description.txt", "description-fr.txt"):
        text = (REPO / "workshop" / name).read_text(encoding="utf-8")
        assert 0 < len(text) <= 8000, name
