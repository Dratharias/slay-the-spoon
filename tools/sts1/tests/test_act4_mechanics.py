"""The hand-recorded Act 4 mechanics stay consistent and tied to the StS1 build they were read from."""
import hashlib
import json
import zipfile
from pathlib import Path

import pytest

MECHANICS = Path(__file__).resolve().parents[3] / "data/sts1/act4/mechanics.json"
JAR = Path("/sts1.jar")


@pytest.fixture(scope="module")
def mechanics():
    return json.loads(MECHANICS.read_text(encoding="utf-8"))


def scaled_values(node):
    """Every {base, ascended, at} object anywhere in the tree."""
    if isinstance(node, dict):
        if {"base", "ascended", "at"} <= node.keys():
            yield node
        for child in node.values():
            yield from scaled_values(child)
    elif isinstance(node, list):
        for child in node:
            yield from scaled_values(child)


def pattern_moves(pattern):
    if "first" in pattern:
        yield pattern["first"]
    for step in pattern["cycle"]:
        yield from step.get("random", [])
        for key in ("move", "unless_last", "else"):
            if key in step:
                yield step[key]


def test_ascension_thresholds_are_sts1_levels(mechanics):
    values = list(scaled_values(mechanics["monsters"]))
    assert values
    for value in values:
        assert 1 <= value["at"] <= 20


def test_patterns_name_only_defined_moves_and_cycle_every_three_turns(mechanics):
    for monster in mechanics["monsters"].values():
        assert len(monster["pattern"]["cycle"]) == 3
        assert set(pattern_moves(monster["pattern"])) <= monster["moves"].keys()


def test_encounters_name_recorded_monsters(mechanics):
    for encounter in mechanics["encounters"].values():
        assert set(encounter["monsters"]) <= mechanics["monsters"].keys()


def test_heart_values_the_plan_forbids_guessing(mechanics):
    heart = mechanics["monsters"]["CorruptHeart"]
    assert heart["pattern"]["first"] == "debilitate"
    start = {entry["power"]: entry["amount"] for entry in heart["start_of_combat"]}
    assert start["Invincible"] == {"base": 300, "ascended": 200, "at": 19}
    assert start["BeatOfDeath"] == {"base": 1, "ascended": 2, "at": 19}
    assert heart["hp"] == {"base": 750, "ascended": 800, "at": 9}


@pytest.mark.skipif(not JAR.exists(), reason="StS1 jar not mounted")
def test_recorded_classes_match_the_installed_jar(mechanics):
    with zipfile.ZipFile(JAR) as archive:
        for name, digest in mechanics["source"]["classes"].items():
            assert hashlib.sha256(archive.read(name)).hexdigest() == digest, name


def test_keys_record_the_three_sources_and_the_emerald_buffs(mechanics):
    keys = mechanics["keys"]
    assert keys["act4_requires"] == ["ruby", "emerald", "sapphire"]
    assert [keys[k]["source"] for k in keys["act4_requires"]] == ["rest_site_option", "marked_elite", "chest"]
    options = keys["emerald"]["buff"]["options"]
    assert [o.get("power") for o in options] == ["Strength", None, "Metallicize", "RegenerateMonster"]
    assert options[1]["max_hp_percent"] == 25
