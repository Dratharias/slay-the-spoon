"""Read decompiled StS1 card classes and cards.json into plain card data.

Usage: python extract.py --decomp <dir> --color red --out data/sts1/ironclad.json

<dir> holds the CFR output (com/megacrit/cardcrawl/cards/<color>/*.java) and
localization/{eng,fra}/cards.json copied from the StS1 jar.
"""
import argparse
import json
import re
from pathlib import Path

LANGS = ("eng", "fra")
KEYWORD_ORDER = ("ethereal", "exhaust", "innate", "retain", "unplayable")

SUPER = re.compile(
    r"super\(ID,\s*[^,]+,\s*\"([^\"]+)\",\s*(-?\d+),\s*[^,]+,\s*"
    r"AbstractCard\.CardType\.(\w+),\s*AbstractCard\.CardColor\.\w+,\s*"
    r"AbstractCard\.CardRarity\.(\w+),\s*AbstractCard\.CardTarget\.(\w+)\)"
)
CARD_ID = re.compile(r'public static final String ID = "([^"]+)";')
BASE = {
    "damage": re.compile(r"this\.baseDamage = (\d+);"),
    "block": re.compile(r"this\.baseBlock = (\d+);"),
    "magic": re.compile(r"this\.(?:magicNumber = this\.)?baseMagicNumber = (\d+);"),
}
UPGRADE = {
    "damage": re.compile(r"this\.upgradeDamage\((-?\d+)\);"),
    "block": re.compile(r"this\.upgradeBlock\((-?\d+)\);"),
    "magic": re.compile(r"this\.upgradeMagicNumber\((-?\d+)\);"),
}
UPGRADE_COST = re.compile(r"this\.upgradeBaseCost\((-?\d+)\);")
KEYWORD_FIELDS = {
    "exhaust": "exhaust",
    "isEthereal": "ethereal",
    "isInnate": "innate",
    "selfRetain": "retain",
}
KEYWORD_SET = re.compile(r"this\.(exhaust|isEthereal|isInnate|selfRetain) = (true|false);")


def cards_dir(decomp: Path, color: str) -> Path:
    real = decomp / "com/megacrit/cardcrawl/cards" / color
    return real if real.is_dir() else decomp / "cards" / color


def method_body(source: str, signature: str) -> str:
    """Text of the first method whose header contains `signature`, braces balanced."""
    start = source.find(signature)
    if start < 0:
        return ""
    open_at = source.find("{", start)
    depth = 0
    for i in range(open_at, len(source)):
        if source[i] == "{":
            depth += 1
        elif source[i] == "}":
            depth -= 1
            if depth == 0:
                return source[open_at + 1:i]
    return source[open_at + 1:]


def constructor_with_super(source: str, cls: str) -> str:
    """Body of the constructor that calls super(ID, ...); others may just delegate with this(...)."""
    for m in re.finditer(rf"public {cls}\(", source):
        body = method_body(source[m.start():], f"public {cls}(")
        if "super(ID" in body:
            return body
    return ""


def keyword_changes(body: str) -> dict:
    return {KEYWORD_FIELDS[f]: v == "true" for f, v in KEYWORD_SET.findall(body)}


def ordered(keywords: set) -> list:
    return [k for k in KEYWORD_ORDER if k in keywords]


def load_text(decomp: Path) -> dict:
    return {
        lang: json.loads((decomp / "localization" / lang / "cards.json").read_text(encoding="utf-8"))
        for lang in LANGS
    }


def parse_card(path: Path, text: dict) -> dict:
    source = path.read_text(encoding="utf-8")
    cls = path.stem
    warnings = []
    card_id = CARD_ID.search(source).group(1)
    ctor = constructor_with_super(source, cls)
    upgrade = method_body(source, "public void upgrade()")

    sup = SUPER.search(ctor)
    if not sup:
        raise ValueError(f"{cls}: constructor super(...) not recognised")
    portrait, cost, ctype, rarity, target = sup.groups()
    cost = int(cost)

    values = {}
    for key, rx in BASE.items():
        m = rx.search(ctor)
        values[key] = int(m.group(1)) if m else None
    deltas = {}
    for key, rx in UPGRADE.items():
        m = rx.findall(upgrade)
        deltas[key] = int(m[0]) if m else None
        if len(m) > 1:
            warnings.append(f"several upgrade{key} calls")
    cost_up = UPGRADE_COST.search(upgrade)

    keywords = {k for k, on in keyword_changes(ctor).items() if on}
    if cost == -2:
        keywords.add("unplayable")
    upgraded = set(keywords)
    for k, on in keyword_changes(upgrade).items():
        (upgraded.add if on else upgraded.discard)(k)

    if "timesUpgraded" in upgrade:
        warnings.append("upgrade depends on timesUpgraded")

    texts = {}
    for lang in LANGS:
        entry = text[lang].get(card_id)
        if entry is None:
            warnings.append(f"no {lang} text")
            entry = {}
        texts[lang] = {
            "name": entry.get("NAME"),
            "description": entry.get("DESCRIPTION"),
            "upgrade_description": entry.get("UPGRADE_DESCRIPTION"),
        }

    card = {
        "class": cls,
        "id": card_id,
        "snake": portrait.rsplit("/", 1)[-1],
        "cost": cost,
        "cost_upgraded": int(cost_up.group(1)) if cost_up else None,
        "type": ctype.lower(),
        "rarity": rarity.lower(),
        "target": target.lower(),
        **values,
        "upgrade": deltas,
        "keywords": ordered(keywords),
        "keywords_upgraded": ordered(upgraded),
        "portrait": portrait,
        "text": texts,
    }
    if warnings:
        card["warnings"] = warnings
    return card


WATCHER_TOKENS = ("Miracle", "Insight", "Smite", "Safety", "ThroughViolence", "Expunger", "Beta", "Omega")
WATCHER_OPTIONS = ("BecomeAlmighty", "FameAndFortune", "LiveForever", "ChooseCalm", "ChooseWrath")


def purple_allowlist(decomp: Path) -> set:
    source = (decomp / "com/megacrit/cardcrawl/helpers/CardLibrary.java").read_text(encoding="utf-8")
    body = method_body(source, "private static void addPurpleCards()")
    names = set(re.findall(r"CardLibrary\.add\(new (\w+)\(", body))
    if len(names) != 75:
        raise ValueError(f"Expected 75 registered purple cards, found {len(names)}")
    return names


def extract_generated(decomp: Path) -> list:
    text = load_text(decomp)
    return sorted([parse_card(cards_dir(decomp, folder) / (name + ".java"), text)
                   for folder, names in (("tempCards", WATCHER_TOKENS), ("optionCards", WATCHER_OPTIONS))
                   for name in names], key=lambda c: c["class"])


def extract(decomp: Path, color: str, only: set | None = None) -> list:
    text = load_text(decomp)
    paths = sorted(cards_dir(decomp, color).glob("*.java"))
    if color == "purple":
        only = purple_allowlist(decomp) if only is None else only & purple_allowlist(decomp)
    if only is not None:
        paths = [p for p in paths if p.stem in only]
    cards = (parse_card(p, text) for p in paths)
    # A class with no English name is a card StS1 never shipped (the Defect's Impulse).
    return sorted((c for c in cards if c["text"]["eng"]["name"]), key=lambda c: c["class"])


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--decomp", type=Path, required=True)
    ap.add_argument("--color", required=True)
    ap.add_argument("--out", type=Path, required=True)
    ap.add_argument("--only", help="comma-separated class allowlist")
    args = ap.parse_args()
    cards = extract_generated(args.decomp) if args.color == "watcher-generated" else extract(args.decomp, args.color, set(args.only.split(",")) if args.only else None)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(cards, indent=1, ensure_ascii=False) + "\n", encoding="utf-8")
    warned = [c for c in cards if "warnings" in c]
    print(f"{len(cards)} cards, {len(warned)} with warnings")
    for c in warned:
        print(f"  {c['class']}: {'; '.join(c['warnings'])}")


if __name__ == "__main__":
    main()
