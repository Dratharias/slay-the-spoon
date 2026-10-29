from conftest import FIXTURES
import extract


def cards():
    return {c["class"]: c for c in extract.extract(FIXTURES, "red")}


def test_parses_constructor():
    c = cards()["FakeSlash"]
    assert (c["cost"], c["type"], c["rarity"], c["target"]) == (2, "attack", "common", "enemy")
    assert c["id"] == "Fake Slash"
    assert c["snake"] == "fake_slash"
    assert c["portrait"] == "red/attack/fake_slash"


def test_parses_base_values_and_upgrade_deltas():
    c = cards()["FakeSlash"]
    assert (c["damage"], c["block"], c["magic"]) == (9, None, 2)
    assert c["upgrade"] == {"damage": 4, "block": None, "magic": 1}
    assert c["cost_upgraded"] == 1


def test_keywords_and_upgrade_removal():
    c = cards()["FakeGuard"]
    assert c["block"] == 7 and c["upgrade"]["block"] == 3
    assert c["keywords"] == ["ethereal", "exhaust"]
    assert c["keywords_upgraded"] == ["ethereal"]
    s = cards()["FakeStorm"]
    assert s["keywords"] == ["innate"]
    assert s["keywords_upgraded"] == ["innate", "retain"]


def test_x_cost_and_unplayable():
    assert cards()["FakeStorm"]["cost"] == -1
    curse = cards()["FakeCurse"]
    assert curse["cost"] == -2
    assert curse["keywords"] == ["unplayable"]
    assert curse["rarity"] == "special"


def test_text_joined_from_both_languages():
    g = cards()["FakeGuard"]["text"]
    assert g["eng"]["name"] == "Fake Guard"
    assert g["eng"]["upgrade_description"] == "Ethereal. NL Gain !B! Block."
    assert g["fra"]["name"] == "Fausse Garde"
    assert cards()["FakeSlash"]["text"]["fra"]["upgrade_description"] is None


def test_sorted_by_class():
    names = [c["class"] for c in extract.extract(FIXTURES, "red")]
    assert names == sorted(names)


def test_delegating_constructor_and_scaling_upgrade():
    c = cards()["FakeForge"]
    assert (c["cost"], c["damage"]) == (2, 12)
    assert "upgrade depends on timesUpgraded" in c["warnings"]
