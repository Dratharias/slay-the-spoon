import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import curse_table as c

REPO = Path(__file__).resolve().parents[2]
TABLE = json.loads((REPO / "data/curses/curses.json").read_text(encoding="utf-8"))


def test_repo_table_is_valid_and_covers_the_mod_curses():
    curses = c.rows(TABLE)
    assert c.validate(TABLE, sorted(curses)) == []
    assert {"STS1_PAIN", "STS1_NECRONOMICURSE"} <= set(c.scan([REPO / "src/Cards"]))
    assert len(curses) == 22


def test_owner_examples():
    bad_luck = TABLE["BAD_LUCK"]
    assert (bad_luck["played"], bad_luck["amount"], bad_luck["amountPlus"]) == ("HpLoss", 13, 20)
    assert c.grimoire_plus(bad_luck) == 5
    assert c.grimoire_plus(TABLE["CLUMSY"]) == 12


def test_necronomicurse_never_costs_zero():
    assert c.cost_plus(TABLE["STS1_NECRONOMICURSE"]) == 1


def test_validate_catches_missing_rows_and_bad_amounts():
    table = {"A": {"keywords": "k", "inHand": "i", "note": "n", "played": "HpLoss", "amount": 5, "amountPlus": 5}}
    errors = c.validate(table, ["A", "B"])
    assert "B: no row" in errors
    assert "A: amountPlus must be above amount" in errors


def test_review_lists_every_curse():
    page = c.review(TABLE)
    assert all(f"| {k} |" in page for k in c.rows(TABLE))


def test_player_only_curses_are_nerfed_when_upgraded():
    assert TABLE["NORMALITY"]["self"] == [3, 4]
    assert TABLE["GUILTY"]["self"] == [5, 3]
    bad = {"A": {"keywords": "k", "inHand": "i", "note": "n", "played": "Weak", "amount": 1, "amountPlus": 2, "self": [1, 2]}}
    assert "A: only a curse with no played effect is nerfed on the player side" in c.validate(bad, ["A"])


def test_every_sts1_curse_has_a_row():
    sts1 = {"ASCENDERS_BANE", "CLUMSY", "CURSE_OF_THE_BELL", "DECAY", "DOUBT", "INJURY", "STS1_NECRONOMICURSE",
            "NORMALITY", "STS1_PAIN", "STS1_PARASITE", "STS1_PRIDE", "REGRET", "SHAME", "WRITHE"}
    assert sts1 <= set(c.rows(TABLE))
