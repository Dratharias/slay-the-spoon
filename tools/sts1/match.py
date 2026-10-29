"""Draft the StS1 ↔ StS2 duplicate table and the generated C# list of namesakes.

Usage: python match.py --sts1 data/sts1/ironclad.json --sts2-cards <decomp>/MegaCrit.Sts2.Core.Models.Cards
           --sts2-loc <dir with eng/cards.json> --table data/duplicates/ironclad.json
           --review docs/review/ironclad-duplicates.md --matches src/CardSets/Sts1Matches.g.cs --character Ironclad

Rows marked `"checked": true` (reviewed by hand) or `"draft": false` (approved by the owner) keep
their `relation` and `note` when the script re-runs; other rows are re-drafted.
"""
import argparse
import json
import re
from pathlib import Path

RELATIONS = ("unique", "same-sts2", "same-sts1", "duplicate")
# StS1 starters are named Strike_Red / Defend_Red; StS2 calls them StrikeIronclad / DefendIronclad.
STARTER_SUFFIX = {"red": "ironclad", "green": "silent", "blue": "defect"}
TARGETS = {
    "AnyEnemy": "enemy", "AllEnemies": "all_enemy", "Self": "self", "None": "none",
    "RandomEnemy": "enemy", "AnyAlly": "self_and_enemy", "AnyPlayer": "self",
}
STS1_TARGETS = {"self_and_enemy": "enemy", "all": "all_enemy", "none": "self"}

BASE = re.compile(r"base\((-?\d+), CardType\.(\w+), CardRarity\.(\w+), TargetType\.(\w+)\)")
VAR = re.compile(r"new (\w*?)Var(?:<(\w+)>)?\(([^()]*)\)")
UPGRADE = re.compile(r"DynamicVars(?:\.(\w+)|\[\"(\w+)\"\])\.UpgradeValueBy\((-?[\d.]+)m\)")
ENERGY_UPGRADE = re.compile(r"EnergyCost\.UpgradeBy\((-?\d+)\)")
KEYWORD = re.compile(r"CardKeyword\.(\w+)")
KEYWORD_EDIT = re.compile(r"(Add|Remove)Keyword\(CardKeyword\.(\w+)\)")
KEYWORD_ORDER = ("ethereal", "exhaust", "innate", "retain", "unplayable")


def normalize(name: str) -> str:
    name = re.sub(r"_(Red|Green|Blue|Purple)$", "", name)
    return re.sub(r"[^a-z]", "", name.lower())


def slug(class_name: str) -> str:
    """ModelDb id entry, as StringHelper.Slugify builds it."""
    return re.sub(r"(?<=[A-Za-z0-9])([A-Z])", r"_\1", class_name).upper()


def find_sts2(sts1_class: str, sts2_names: list, color: str = "red"):
    key = normalize(sts1_class)
    if key in ("strike", "defend"):
        key += STARTER_SUFFIX[color]
    for name in sts2_names:
        if normalize(name) == key:
            return name
    return None


def block_after(source: str, header: str) -> str:
    start = source.find(header)
    if start < 0:
        return ""
    open_at = source.find("{", start)
    depth = 0
    for i in range(open_at, len(source)):
        depth += {"{": 1, "}": -1}.get(source[i], 0)
        if depth == 0:
            return source[open_at + 1:i]
    return ""


def member(source: str, name: str) -> str:
    """Body of a property, whether written `=> expr;` or `{ ... }`."""
    start = source.find(name)
    if start < 0:
        return ""
    arrow, brace = source.find("=>", start), source.find("{", start)
    if arrow < 0 or (0 <= brace < arrow):
        return block_after(source, name)
    depth = 0
    for i in range(arrow + 2, len(source)):
        depth += {"(": 1, "{": 1, "[": 1, ")": -1, "}": -1, "]": -1}.get(source[i], 0)
        if source[i] == ";" and depth == 0:
            return source[arrow + 2:i]
    return ""


def var_name(kind: str, generic, args: str) -> str:
    if generic:
        return generic
    named = re.match(r'\s*"(\w+)"', args)
    if named:
        return named.group(1)
    return kind or "Dynamic"


def first_number(args: str):
    m = re.search(r"(-?\d+(?:\.\d+)?)m?", re.sub(r'"[^"]*"', "", args))
    if not m:
        return None
    value = float(m.group(1))
    return int(value) if value.is_integer() else value


def ordered(keywords) -> list:
    return [k for k in KEYWORD_ORDER if k in keywords]


def parse_sts2(path: Path) -> dict:
    source = path.read_text(encoding="utf-8")
    cost, ctype, rarity, target = BASE.search(source).groups()
    cost = int(cost)
    if "HasEnergyCostX => true" in source:
        cost = -1
    variables = {}
    for kind, generic, args in VAR.findall(member(source, "CanonicalVars")):
        variables[var_name(kind, generic, args)] = first_number(args)
    on_upgrade = block_after(source, "void OnUpgrade()")
    upgrade = {a or b: first_number(v) for a, b, v in UPGRADE.findall(on_upgrade)}
    energy = ENERGY_UPGRADE.search(on_upgrade)
    keywords = {k.lower() for k in KEYWORD.findall(member(source, "CanonicalKeywords"))}
    upgraded = set(keywords)
    for op, k in KEYWORD_EDIT.findall(on_upgrade):
        (upgraded.add if op == "Add" else upgraded.discard)(k.lower())
    return {
        "class": path.stem,
        "cost": cost,
        "cost_upgraded": cost + int(energy.group(1)) if energy and cost >= 0 else None,
        "type": ctype.lower(),
        "rarity": rarity.lower(),
        "target": TARGETS.get(target, target.lower()),
        "vars": variables,
        "upgrade": upgrade,
        "keywords": ordered(keywords),
        "keywords_upgraded": ordered(upgraded),
    }


def magic_of(sts2: dict):
    """The one StS2 var that plays the role of StS1's magic number, if there is exactly one."""
    others = [v for k, v in sts2["vars"].items() if k not in ("Damage", "Block")]
    return others[0] if len(others) == 1 else None


def magic_upgrade_of(sts2: dict):
    others = [v for k, v in sts2["upgrade"].items() if k not in ("Damage", "Block")]
    return others[0] if len(others) == 1 else None


def draft(sts1: dict, sts2: dict) -> tuple:
    """(relation, note) from structure and numbers; a human reviews every row."""
    reasons = []
    if sts1["type"] != sts2["type"]:
        reasons.append(f"type {sts1['type']} vs {sts2['type']}")
    if STS1_TARGETS.get(sts1["target"], sts1["target"]) != sts2["target"]:
        reasons.append(f"target {sts1['target']} vs {sts2['target']}")
    if sts1["keywords"] != sts2["keywords"] or sts1["keywords_upgraded"] != sts2["keywords_upgraded"]:
        reasons.append(f"keywords {sts1['keywords']}→{sts1['keywords_upgraded']} vs "
                       f"{sts2['keywords']}→{sts2['keywords_upgraded']}")
    has_magic = sts1["magic"] is not None
    other_vars = [k for k in sts2["vars"] if k not in ("Damage", "Block")]
    if has_magic != bool(other_vars) or len(other_vars) > 1:
        reasons.append(f"values: StS1 magic={sts1['magic']} vs StS2 vars {sorted(sts2['vars'])}")
    if (sts1["damage"] is None) != ("Damage" not in sts2["vars"]) or \
            (sts1["block"] is None) != ("Block" not in sts2["vars"]):
        reasons.append("damage/block presence differs")
    if reasons:
        return "duplicate", "; ".join(reasons)

    # Same structure: compare numbers, base and upgraded. Higher is better except cost.
    def up(base, delta):
        return None if base is None else base + (delta or 0)

    pairs = []
    s1u, s2u = sts1["upgrade"], sts2["upgrade"]
    for key, s1, s2, s1d, s2d in (
        ("damage", sts1["damage"], sts2["vars"].get("Damage"), s1u["damage"], s2u.get("Damage")),
        ("block", sts1["block"], sts2["vars"].get("Block"), s1u["block"], s2u.get("Block")),
        ("magic", sts1["magic"], magic_of(sts2), s1u["magic"], magic_upgrade_of(sts2)),
    ):
        if s1 is not None:
            pairs.append((key, s1, s2, 1))
            pairs.append((key + "+", up(s1, s1d), up(s2, s2d), 1))
    pairs.append(("cost", sts1["cost"], sts2["cost"], -1))
    s1c = sts1["cost_upgraded"] if sts1["cost_upgraded"] is not None else sts1["cost"]
    s2c = sts2["cost_upgraded"] if sts2["cost_upgraded"] is not None else sts2["cost"]
    pairs.append(("cost+", s1c, s2c, -1))

    sts1_better, sts2_better = [], []
    for key, s1, s2, sign in pairs:
        if s1 is None or s2 is None or s1 == s2:
            continue
        (sts1_better if (s1 - s2) * sign > 0 else sts2_better).append(f"{key} {s1} vs {s2}")
    magic_changed = any(k.startswith("magic") for k in sts1_better + sts2_better)
    if not sts1_better and not sts2_better:
        return "same-sts2", "identical numbers"
    if sts1_better and sts2_better:
        return "same-sts2", "? mixed: StS1 better on " + ", ".join(sts1_better) + \
            "; StS2 better on " + ", ".join(sts2_better)
    note = ("StS1 weaker: " + ", ".join(sts2_better)) if sts2_better else \
        ("StS2 weaker: " + ", ".join(sts1_better))
    if magic_changed:
        note += " (? check the magic number's direction)"
    return ("same-sts1" if sts2_better else "same-sts2"), note


def build_table(sts1_cards: list, sts2_dir: Path, previous: list, color: str = "red") -> list:
    names = sorted(p.stem for p in sts2_dir.glob("*.cs"))
    kept = {r["sts1"]: r for r in previous}
    rows = []
    for card in sts1_cards:
        sts2_name = find_sts2(card["class"], names, color)
        if sts2_name is None:
            relation, note = "unique", ""
        else:
            relation, note = draft(card, parse_sts2(sts2_dir / f"{sts2_name}.cs"))
        row = {"sts1": card["class"], "sts2": sts2_name, "relation": relation, "draft": True, "note": note}
        old = kept.get(card["class"])
        if old and old.get("sts2") == sts2_name and (old.get("checked") or not old.get("draft", True)):
            row.update({k: old[k] for k in ("relation", "draft", "note", "checked") if k in old})
        rows.append(row)
    return rows


def matches_cs(rows: list, character: str) -> str:
    ids = sorted(slug(r["sts2"]) for r in rows if r["sts2"])
    listed = ", ".join(f'"{i}"' for i in ids)
    return (
        "// Generated by tools/sts1/match.py from data/duplicates/ — do not edit.\n"
        "namespace SlaySpoon.CardSets;\n\n"
        "/// <summary>StS2 card ids that have a StS1 namesake, per character.</summary>\n"
        "public static partial class Sts1Matches\n{\n"
        f"\tpublic static readonly string[] {character} = {{ {listed} }};\n"
        "}\n"
    )


def render_sts2_text(text: str, sts2: dict) -> str:
    values = dict(sts2["vars"])

    def sub(m):
        name = m.group(1)
        if name == "IfUpgraded":
            return "{" + m.group(0)[1:-1] + "}"
        if name in values:
            up = sts2["upgrade"].get(name) or sts2["upgrade"].get(name.replace("Power", ""))
            return f"{values[name]}" + (f" (+{up})" if up else "")
        return m.group(0)

    return re.sub(r"\{(\w+):[^{}]*\}", sub, text).replace("\n", " ")


def review_md(rows: list, sts1_cards: list, sts2_dir: Path, sts2_text: dict) -> str:
    by_class = {c["class"]: c for c in sts1_cards}

    def sts1_cell(c):
        t = c["text"]["eng"]
        desc = t["description"].replace(" NL ", " ").replace("!D!", str(c["damage"])) \
            .replace("!B!", str(c["block"])).replace("!M!", str(c["magic"]))
        ups = [f"{k} +{v}" for k, v in c["upgrade"].items() if v]
        if c["cost_upgraded"] is not None:
            ups.append(f"cost → {c['cost_upgraded']}")
        if c["keywords_upgraded"] != c["keywords"]:
            ups.append(f"keywords → {c['keywords_upgraded'] or 'none'}")
        if t["upgrade_description"]:
            ups.append("text: " + t["upgrade_description"].replace(" NL ", " "))
        cost = "X" if c["cost"] == -1 else c["cost"]
        return f"**{t['name']}** ({cost}, {c['rarity']}) {desc}<br>Upgrade: {'; '.join(ups) or '—'}"

    def sts2_cell(name):
        s = parse_sts2(sts2_dir / f"{name}.cs")
        key = slug(name)
        desc = render_sts2_text(sts2_text.get(f"{key}.description", "?"), s)
        ups = [f"{k} +{v}" for k, v in s["upgrade"].items()]
        if s["cost_upgraded"] is not None:
            ups.append(f"cost → {s['cost_upgraded']}")
        if s["keywords_upgraded"] != s["keywords"]:
            ups.append(f"keywords → {s['keywords_upgraded'] or 'none'}")
        cost = "X" if s["cost"] == -1 else s["cost"]
        return f"**{sts2_text.get(key + '.title', name)}** ({cost}, {s['rarity']}) {desc}<br>Upgrade: {'; '.join(ups) or '—'}"

    out = [
        "# Ironclad — StS1 / StS2 duplicate table (review)",
        "",
        "Generated by `tools/sts1/match.py`; the decisions live in `data/duplicates/ironclad.json`.",
        "Relations (spec §3.4):",
        "",
        "- **same-sts2**: same effect, only numbers differ, StS2 is weaker or equal → the vanilla card is used in every mode.",
        "- **same-sts1**: same effect, StS1 is weaker → the StS1 card replaces the vanilla one in StS1-only and Hybrid.",
        "- **duplicate**: different effect or keyword → both exist; in Hybrid the player picks the version.",
        "- **unique**: no StS2 namesake → the StS1 card is added (not listed below).",
        "",
        "Numbers in the StS2 column are the card's values with the upgrade in parentheses.",
        "",
    ]
    for relation in ("duplicate", "same-sts1", "same-sts2"):
        chosen = [r for r in rows if r["relation"] == relation]
        out += [f"## {relation} ({len(chosen)})", "", "| StS1 | StS2 | Note |", "|---|---|---|"]
        for r in chosen:
            mark = " ⚠ draft" if r["draft"] else ""
            out.append(f"| {sts1_cell(by_class[r['sts1']])} | {sts2_cell(r['sts2'])} | {r['note']}{mark} |")
        out.append("")
    unique = [r["sts1"] for r in rows if r["relation"] == "unique"]
    out += [f"## unique ({len(unique)})", "", ", ".join(unique), ""]
    return "\n".join(out)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--sts1", type=Path, required=True)
    ap.add_argument("--sts2-cards", type=Path, required=True)
    ap.add_argument("--sts2-loc", type=Path, required=True)
    ap.add_argument("--table", type=Path, required=True)
    ap.add_argument("--review", type=Path, required=True)
    ap.add_argument("--matches", type=Path, required=True)
    ap.add_argument("--character", required=True)
    ap.add_argument("--color", default="red")
    args = ap.parse_args()

    sts1_cards = json.loads(args.sts1.read_text(encoding="utf-8"))
    previous = json.loads(args.table.read_text(encoding="utf-8")) if args.table.exists() else []
    rows = build_table(sts1_cards, args.sts2_cards, previous, args.color)
    args.table.parent.mkdir(parents=True, exist_ok=True)
    args.table.write_text(json.dumps(rows, indent=1, ensure_ascii=False) + "\n", encoding="utf-8")
    args.matches.write_text(matches_cs(rows, args.character), encoding="utf-8")
    sts2_text = json.loads((args.sts2_loc / "eng" / "cards.json").read_text(encoding="utf-8"))
    args.review.parent.mkdir(parents=True, exist_ok=True)
    args.review.write_text(review_md(rows, sts1_cards, args.sts2_cards, sts2_text), encoding="utf-8")
    counts = {r: sum(1 for x in rows if x["relation"] == r) for r in RELATIONS}
    print(" ".join(f"{k}={v}" for k, v in counts.items()),
          f"flagged={sum(1 for x in rows if '?' in x['note'])}")


if __name__ == "__main__":
    main()
