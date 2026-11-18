import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import forge_curves as f

REPO = Path(__file__).resolve().parents[2]


def test_card_id_matches_the_games_snake_case():
    assert f.card_id("Sts1LimitBreak") == "STS1_LIMIT_BREAK"
    assert f.card_id("Sts1AThousandCuts") == "STS1_A_THOUSAND_CUTS"
    assert f.card_id("BattleTrance") == "BATTLE_TRANCE"


def test_only_on_upgrade_vars_count():
    source = """
    class Foo {
        protected override void OnUpgrade()
        {
            DynamicVars.Energy.UpgradeValueBy(1m);
            if (x) { DynamicVars["Cards"].UpgradeValueBy(1m); }
        }
        void Other() { DynamicVars.Repeat.UpgradeValueBy(1m); }
    }"""
    assert f.upgraded_vars(source) == ["Cards", "Energy"]


def test_lowest_cap_wins_and_safe_vars_need_no_row():
    assert f.propose(["Damage", "Block"]) is None
    row = f.propose(["Cards", "Energy", "Damage"])
    assert row["cap"] == 3
    assert "Energy" in row["note"]
    assert f.propose(["IntangiblePower"])["cap"] == 3


def test_committed_table_follows_the_parser_rules():
    table = json.loads((REPO / "data/forge/curves.json").read_text(encoding="utf-8"))
    for key, row in table.items():
        if key.startswith("_"):
            continue
        assert 1 <= row.get("cap", 9) <= 9, key
        assert row.get("costPerLevel", 0) <= 0, key
    limit = table["STS1_LIMIT_BREAK"]
    assert limit["cap"] == 5 and limit["perLevel"] == {"StrengthPower": 1}
    assert limit["beyond"] == ["StrengthPower"]


def test_safe_vars_keep_growing_past_the_cap():
    assert f.propose(["Cards", "Energy", "Damage"])["beyond"] == ["Damage"]
    assert "beyond" not in f.propose(["Energy"])


def test_validate_flags_bad_rows():
    errors = f.validate({"A": {"plus1": "x", "effect": "y", "note": "z", "cap": 12, "play": {"Gold": 1}}, "B": {"plus1": "x"}})
    assert any("A: cap" in e for e in errors)
    assert any("unknown play kind Gold" in e for e in errors)
    assert any("B: missing effect" in e for e in errors)


def test_runtime_keeps_reviewed_calls_and_auto_rows_for_the_rest():
    cards = {"A": {"plus1": "x", "effect": "y", "note": "n", "cap": 9}, "B": {"plus1": "x", "effect": "y", "note": "n", "play": {"Block": 2}}}
    rows = f.runtime(cards, {"A": {"cap": 3}, "C": {"cap": 5}})
    assert "A" not in rows
    assert rows["B"] == {"play": {"Block": 2}, "note": "n"}
    assert rows["C"] == {"cap": 5}


def test_committed_review_is_valid():
    cards = json.loads((REPO / "data/forge/cards.json").read_text(encoding="utf-8"))
    assert f.validate(cards) == []
    assert len([k for k in cards if not k.startswith("_")]) >= 780
