"""The Forge's curve table (docs/superpowers/specs/2026-11-18-forge-design.md).

Without a row, a card past +1 replays its +1 var deltas up to +9 (src/Forge/ForgeLevels.cs). This tool scans every card's
OnUpgrade (vanilla decompile, read locally, and the mod's own cards) and caps the cards whose +1 raises a var that breaks
the game when repeated: energy, extra plays, draw, gold, Intangible... The lowest cap among the vars touched wins.
The hand-reviewed table data/forge/cards.json (every card: what its +1 does, what it does, and its per-level calls)
wins; a scanned card missing from it gets the automatic proposal and is listed as not reviewed. Writes
data/forge/curves.json and the owner's review page docs/review/forge-curves.md. Never writes game code.

    python tools/forge_curves.py --sts2 <decompile>/MegaCrit.Sts2.Core.Models.Cards --mod src/Cards
"""

import argparse
import json
import re
from pathlib import Path

# Var name -> (cap, reason). Names are matched exactly, then by the ending ("XPower" also matches "X").
RISKY = {
    "Energy": (3, "energy"),
    "Stars": (3, "stars"),
    "StarsPerTurn": (3, "stars"),
    "StarNextTurnPower": (3, "stars"),
    "Repeat": (3, "extra hits or plays"),
    "Replay": (3, "extra hits or plays"),
    "Loop": (3, "extra hits or plays"),
    "PlayMax": (3, "extra hits or plays"),
    "CalculationExtra": (5, "scaling multiplier"),
    "Increase": (5, "growth per play"),
    "Cards": (5, "draw or card count"),
    "Skills": (5, "draw or card count"),
    "Attacks": (5, "draw or card count"),
    "Shivs": (5, "draw or card count"),
    "Summon": (5, "summons"),
    "Intangible": (3, "Intangible"),
    "Buffer": (3, "Buffer"),
    "Gold": (3, "economy"),
    "MaxHp": (3, "economy"),
    "Heal": (5, "healing"),
    "Orbs": (5, "orb count"),
    "Multiplier": (5, "scaling multiplier"),
    "Focus": (5, "Focus"),
}

_UPGRADE = re.compile(r'DynamicVars(?:\.(\w+)|\["(\w+)"\])\.UpgradeValueBy')
_ON_UPGRADE = re.compile(r"\bvoid\s+OnUpgrade\s*\(\s*\)\s*\{")


def card_id(class_name: str) -> str:
    """ModelDb id entry of a card class: Sts1LimitBreak -> STS1_LIMIT_BREAK."""
    return re.sub(r"(?<=[A-Za-z0-9])(?=[A-Z])", "_", class_name).upper()


def on_upgrade_body(source: str) -> str:
    """The OnUpgrade method's body, or "" when the class has none."""
    m = _ON_UPGRADE.search(source)
    if not m:
        return ""
    depth, i = 1, m.end()
    while i < len(source) and depth:
        depth += {"{": 1, "}": -1}.get(source[i], 0)
        i += 1
    return source[m.end():i - 1]


def upgraded_vars(source: str) -> list[str]:
    body = on_upgrade_body(source)
    return sorted({a or b for a, b in _UPGRADE.findall(body)})


def risk(var: str):
    if var in RISKY:
        return RISKY[var]
    if var.endswith("Power") and var[:-5] in RISKY:
        return RISKY[var[:-5]]
    return None


def safe(variables) -> list[str]:
    """The vars that keep growing past the cap up to +9: every var that is not risky."""
    return sorted(v for v in variables if not risk(v))


def propose(variables: list[str]):
    """The row for a card whose +1 raises these vars, or None when +9 is fine."""
    hits = [(r[0], r[1], v) for v in variables if (r := risk(v))]
    if not hits:
        return None
    cap = min(h[0] for h in hits)
    reasons = sorted({f"{h[1]} ({h[2]})" for h in hits if h[0] == cap})
    row = {"cap": cap, "note": "auto: " + ", ".join(reasons)}
    if beyond := safe(variables):
        row["beyond"] = beyond
    return row


def scan_vars(directories) -> dict:
    """Card id -> the vars its +1 raises."""
    found = {}
    for directory in directories:
        for path in sorted(Path(directory).rglob("*.cs")):
            source = path.read_text(encoding="utf-8", errors="replace")
            if not re.search(r"class\s+" + re.escape(path.stem) + r"\b", source):
                continue
            found[card_id(path.stem)] = upgraded_vars(source)
    return found


def scan(directories) -> dict:
    return {key: row for key, variables in scan_vars(directories).items() if (row := propose(variables))}


RUNTIME = ("cap", "fusable", "perLevel", "costPerLevel", "beyond", "powerBonus", "play")
PLAY_KINDS = {"Block", "Damage", "Draw", "Energy", "Strength", "Dexterity", "Vigor", "Poison", "Weak", "Vulnerable"}


def validate(cards: dict) -> list[str]:
    """Problems in the hand-reviewed table."""
    errors = []
    for key, row in cards.items():
        if key.startswith("_"):
            continue
        if not 1 <= row.get("cap", 9) <= 9:
            errors.append(f"{key}: cap out of 1..9")
        if row.get("costPerLevel", 0) > 0:
            errors.append(f"{key}: costPerLevel must be <= 0")
        for kind in row.get("play", {}):
            if kind not in PLAY_KINDS:
                errors.append(f"{key}: unknown play kind {kind}")
        for field in ("plus1", "effect", "note"):
            if not row.get(field):
                errors.append(f"{key}: missing {field}")
    return errors


def runtime(cards: dict, proposed: dict) -> dict:
    """The runtime table: reviewed rows with a non-default field, plus auto proposals for cards nobody reviewed."""
    rows = {k: v for k, v in proposed.items() if k not in cards}
    for key, row in cards.items():
        if key.startswith("_"):
            continue
        kept = {f: row[f] for f in RUNTIME if f in row}
        if kept.get("cap") == 9:
            del kept["cap"]
        if kept:
            kept["note"] = row.get("note", "")
            rows[key] = kept
    return dict(sorted(rows.items()))


def _fmt(d: dict) -> str:
    return ", ".join(f"{k} {v:+g}" for k, v in d.items())


def review(cards: dict, missing: list[str]) -> str:
    lines = [
        "# The Forge: every card",
        "",
        "Hand-reviewed table `data/forge/cards.json` (StS2 and the mod's StS1 cards); `tools/forge_curves.py` turns it",
        "into the runtime table `data/forge/curves.json`. Each level past +1 replays the card's +1 numbers (cost and",
        "keywords stay as at +1) up to **Cap**; past the cap only the **Past the cap** vars keep growing. Cards whose +1",
        "only lowers the cost keep lowering it until 0. **Power bonus** and **Play** are added per level past +1",
        "(so +3 gives twice the amount). Every card also gets Retain at +6, cost -1 at +7 and Innate at +9.",
        "",
    ]
    if missing:
        lines += ["**Not reviewed yet (auto proposal used):** " + ", ".join(missing), ""]
    lines += [
        "| Card | +1 | Effect | Cap | Per level | Past the cap | Power bonus | Play | Why |",
        "|------|----|--------|-----|-----------|--------------|-------------|------|-----|",
    ]
    for key, row in cards.items():
        if key.startswith("_"):
            continue
        cap = "no fusion" if row.get("fusable") is False else str(row.get("cap", 9))
        per = _fmt(row.get("perLevel", {}))
        if row.get("costPerLevel"):
            per = (per + ", " if per else "") + f"cost {row['costPerLevel']}"
        cells = [key, row.get("plus1", ""), row.get("effect", ""), cap, per or "-",
                 ", ".join(row.get("beyond", [])) or "-", _fmt(row.get("powerBonus", {})) or "-",
                 _fmt(row.get("play", {})) or "-", row.get("note", "")]
        lines.append("| " + " | ".join(str(c).replace("|", "/").replace("\n", " ") for c in cells) + " |")
    return "\n".join(lines) + "\n"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--sts2", required=True)
    parser.add_argument("--mod", default="src/Cards")
    parser.add_argument("--cards", default="data/forge/cards.json")
    parser.add_argument("--out", default="data/forge/curves.json")
    parser.add_argument("--review", default="docs/review/forge-curves.md")
    args = parser.parse_args()
    cards = json.loads(Path(args.cards).read_text(encoding="utf-8"))
    errors = validate(cards)
    if errors:
        raise SystemExit("\n".join(errors))
    variables = scan_vars([args.sts2, args.mod])
    proposed = {key: row for key, v in variables.items() if (row := propose(v))}
    # Curses, statuses, powers and helper classes have no OnUpgrade vars and never fuse.
    missing = sorted(k for k, v in variables.items() if k not in cards and v)
    rows = runtime(cards, proposed)
    table = {"_comment": "Generated by tools/forge_curves.py from data/forge/cards.json; edit that file instead."} | rows
    Path(args.out).write_text(json.dumps(table, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    Path(args.review).write_text(review(cards, missing), encoding="utf-8")
    print(f"{len(rows)} runtime rows, {len(cards)} reviewed cards, {len(missing)} not reviewed")
    for key in missing:
        print("not reviewed:", key)


if __name__ == "__main__":
    main()
