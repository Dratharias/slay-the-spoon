"""The Cursed Deck's curse table (docs/superpowers/specs/2026-11-18-cursed-deck-design.md).

data/curses/curses.json is the hand review of every curse: what it does to you, what it does to the chosen enemy when
played with the Unholy Grimoire, and its upgraded (+) numbers. This tool checks that every curse of the game and the mod
has a row, validates the rows and writes the owner's review page docs/review/cursed-curses.md. Never writes game code.

    python tools/curse_table.py --sts2 <decompile>/MegaCrit.Sts2.Core.Models.Cards --mod src/Cards
"""

import argparse
import json
import re
from pathlib import Path

GRIMOIRE = 5
GRIMOIRE_NO_EFFECT_PLUS = 12
PLAYED_KINDS = {"None", "HpLoss", "Damage", "HpLossPerHandCard", "HpLossPerCardPlayed", "Weak", "Vulnerable", "GainGold", "MaxHpLoss"}
LABELS = {
    "HpLoss": "loses {} HP", "Damage": "takes {} damage", "HpLossPerHandCard": "loses {} HP per card in your hand",
    "HpLossPerCardPlayed": "loses {} HP per card you played this turn", "Weak": "gains {} Weak",
    "Vulnerable": "gains {} Vulnerable", "GainGold": "you gain {} gold", "MaxHpLoss": "loses {} max HP",
}


def card_id(class_name: str) -> str:
    return re.sub(r"(?<=[A-Za-z0-9])(?=[A-Z])", "_", class_name).upper()


def scan(directories) -> list[str]:
    """Ids of every class that builds a curse (CardType.Curse in its constructor's base call)."""
    found = set()
    for directory in directories:
        for path in Path(directory).rglob("*.cs"):
            source = path.read_text(encoding="utf-8", errors="replace")
            if re.search(r"class\s+" + re.escape(path.stem) + r"\b[^{]*:\s*(CardModel|Sts1CurseCard)\b", source) and (
                    "CardType.Curse" in source or "Sts1CurseCard" in source):
                found.add(card_id(path.stem))
    return sorted(found)


def rows(table: dict) -> dict:
    return {k: v for k, v in table.items() if not k.startswith("_")}


def grimoire_plus(row: dict) -> int:
    return GRIMOIRE if row["played"] != "None" else GRIMOIRE_NO_EFFECT_PLUS


def cost_plus(row: dict) -> int:
    return row.get("costPlus", 0)


def validate(table: dict, scanned: list[str]) -> list[str]:
    errors = []
    curses = rows(table)
    for key in scanned:
        if key not in curses:
            errors.append(f"{key}: no row")
    for key, row in curses.items():
        if key not in scanned:
            errors.append(f"{key}: not a curse of the game or the mod")
        kind = row.get("played")
        if kind not in PLAYED_KINDS:
            errors.append(f"{key}: unknown played kind {kind}")
        has_amount = "amount" in row and "amountPlus" in row
        if kind != "None" and not has_amount:
            errors.append(f"{key}: amount and amountPlus needed")
        if kind == "None" and ("amount" in row or "amountPlus" in row):
            errors.append(f"{key}: no amount without a played effect")
        if has_amount and row["amountPlus"] <= row["amount"]:
            errors.append(f"{key}: amountPlus must be above amount")
        if "self" in row:
            if kind != "None":
                errors.append(f"{key}: only a curse with no played effect is nerfed on the player side")
            if len(row["self"]) != 2 or not row.get("inHandPlus"):
                errors.append(f"{key}: self needs [vanilla, upgraded] and inHandPlus")
        for field in ("keywords", "inHand", "note"):
            if not row.get(field):
                errors.append(f"{key}: missing {field}")
    return errors


def played_text(row: dict, plus: bool) -> str:
    if row["played"] == "None":
        return "-"
    return LABELS[row["played"]].format(row["amountPlus" if plus else "amount"])


def review(table: dict) -> str:
    lines = [
        "# Cursed Deck: every curse",
        "",
        "Hand-reviewed table `data/curses/curses.json`; `tools/curse_table.py` writes this page. With the Unholy Grimoire",
        f"a curse costs 1 (0 upgraded) and targets one enemy: it is exhausted (Grimoire: {GRIMOIRE} damage to ALL enemies),",
        "then **Played** hits the chosen enemy. Left in hand, it does **In hand** to you, as in vanilla. Upgraded (+), its",
        "number goes up on both sides; a curse with no played effect gets the Grimoire at "
        f"{GRIMOIRE_NO_EFFECT_PLUS} instead, and its effect on you is nerfed when it has one (**+ In hand**).",
        "",
        "| Curse | Keywords | In hand (you) | + In hand | Played (enemy) | + Played | + Grimoire | + Cost | Why |",
        "|-------|----------|---------------|-----------|----------------|----------|------------|--------|-----|",
    ]
    for key, row in rows(table).items():
        cells = [key, row["keywords"], row["inHand"], row.get("inHandPlus", "same" if row["played"] == "None" else "stronger, as Played"), played_text(row, False), played_text(row, True),
                 grimoire_plus(row), cost_plus(row), row["note"]]
        lines.append("| " + " | ".join(str(c).replace("|", "/") for c in cells) + " |")
    return "\n".join(lines) + "\n"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--sts2", required=True)
    parser.add_argument("--mod", default="src/Cards")
    parser.add_argument("--table", default="data/curses/curses.json")
    parser.add_argument("--review", default="docs/review/cursed-curses.md")
    args = parser.parse_args()
    table = json.loads(Path(args.table).read_text(encoding="utf-8"))
    errors = validate(table, sorted(set(scan([args.sts2, args.mod]))))
    if errors:
        raise SystemExit("\n".join(errors))
    Path(args.review).write_text(review(table), encoding="utf-8")
    print(f"{len(rows(table))} curses reviewed")


if __name__ == "__main__":
    main()
