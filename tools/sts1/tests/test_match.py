import copy
import json

from conftest import FIXTURES
import extract
import match


def sts1():
    return extract.extract(FIXTURES, "red")


def table(previous=None):
    return match.build_table(sts1(), FIXTURES / "sts2cards", previous or [])


def row(rows, sts1_class):
    return next(r for r in rows if r["sts1"] == sts1_class)


def test_name_normalization():
    assert match.normalize("ThunderClap") == match.normalize("Thunderclap")
    assert match.normalize("Strike_Red") == "strike"
    names = ["Thunderclap", "StrikeIronclad", "Other"]
    assert match.find_sts2("ThunderClap", names) == "Thunderclap"
    assert match.find_sts2("Strike_Red", names) == "StrikeIronclad"
    assert match.find_sts2("Missing", names) is None


def test_parses_sts2_card():
    c = match.parse_sts2(FIXTURES / "sts2cards" / "FakeSlash.cs")
    assert (c["cost"], c["type"], c["target"]) == (2, "attack", "enemy")
    assert c["vars"] == {"Damage": 9, "VulnerablePower": 2}
    assert c["upgrade"] == {"Damage": 4, "VulnerablePower": 1}
    assert c["cost_upgraded"] == 1
    s = match.parse_sts2(FIXTURES / "sts2cards" / "FakeStorm.cs")
    assert s["cost"] == -1
    assert s["keywords"] == ["innate"] and s["keywords_upgraded"] == ["innate", "retain"]


def test_unmatched_is_unique():
    assert row(table(), "FakeCurse")["relation"] == "unique"
    assert row(table(), "FakeCurse")["sts2"] is None


def test_identical_card_drafts_same_sts2():
    r = row(table(), "FakeSlash")
    assert r["sts2"] == "FakeSlash"
    assert r["relation"] == "same-sts2"
    assert r["draft"] is True


def test_numbers_only_weaker_sts1_drafts_same_sts1():
    # StS1 Fake Forge: 2 energy, 12 damage. StS2: 1 energy, 10 damage → mixed, flagged.
    # StS1 Fake Storm: 5 (+3) damage; StS2: 6 (+2) → mixed too. Build a clean case instead.
    cards = sts1()
    forge = next(c for c in cards if c["class"] == "FakeForge")
    forge["cost"], forge["damage"] = 1, 8
    rows = match.build_table(cards, FIXTURES / "sts2cards", [])
    assert row(rows, "FakeForge")["relation"] == "same-sts1"


def test_keyword_difference_drafts_duplicate():
    # StS1 Fake Guard exhausts, StS2 Fake Guard does not.
    assert row(table(), "FakeGuard")["relation"] == "duplicate"


def test_mixed_numbers_is_flagged():
    r = row(table(), "FakeStorm")
    assert r["relation"] == "same-sts2"
    assert "?" in r["note"]


def test_rerun_keeps_hand_edits():
    first = table()
    edited = copy.deepcopy(first)
    row(edited, "FakeSlash").update(relation="duplicate", note="checked by hand", checked=True)
    row(edited, "FakeGuard").update(relation="same-sts1", note="owner says so", draft=False)
    row(edited, "FakeStorm").update(relation="duplicate", note="stale draft")
    again = table(edited)
    assert row(again, "FakeSlash") == row(edited, "FakeSlash")
    assert row(again, "FakeGuard") == row(edited, "FakeGuard")
    assert row(again, "FakeStorm")["relation"] == "same-sts2"


def test_matches_file_lists_sorted_sts2_ids():
    text = match.matches_cs(table(), "Ironclad")
    assert '"FAKE_FORGE", "FAKE_GUARD", "FAKE_SLASH", "FAKE_STORM"' in text
    assert "do not edit" in text


def test_parses_expression_bodied_members():
    c = match.parse_sts2(FIXTURES / "sts2cards" / "FakeLone.cs")
    assert c["vars"] == {"Damage": 6}
    assert c["keywords"] == ["exhaust"]


def test_target_none_counts_as_self():
    cards = sts1()
    guard = next(c for c in cards if c["class"] == "FakeGuard")
    guard["target"] = "none"
    guard["keywords"], guard["keywords_upgraded"] = ["ethereal"], ["ethereal"]
    rows = match.build_table(cards, FIXTURES / "sts2cards", [])
    assert row(rows, "FakeGuard")["relation"] == "same-sts2"
