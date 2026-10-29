"""Generate C# skeletons and eng/fra text for StS1 cards that ship as their own class.

Usage: python codegen.py --sts1 data/sts1/ironclad.json --table data/duplicates/ironclad.json
           --character Ironclad --src src/Cards/Sts1/Ironclad --loc godot/SlaySpoon/localization
           [--relations unique,same-sts1,duplicate]

Never overwrites: a class file that exists is left alone (it is hand-ported), and only missing text
keys are added. OnPlay bodies are written by hand; the skeleton throws until then.
"""
import argparse
import json
import re
from pathlib import Path

import match

TYPES = {"attack": "Attack", "skill": "Skill", "power": "Power", "status": "Status", "curse": "Curse"}
RARITIES = {"basic": "Basic", "common": "Common", "uncommon": "Uncommon", "rare": "Rare", "special": "Token"}
TARGETS = {"enemy": "AnyEnemy", "all_enemy": "AllEnemies", "self": "Self", "none": "Self",
           "self_and_enemy": "AnyEnemy", "all": "AllEnemies"}
KEYWORDS = {"ethereal": "Ethereal", "exhaust": "Exhaust", "innate": "Innate", "retain": "Retain",
            "unplayable": "Unplayable"}
# Keyword sentences StS2 prints by itself from the card's keywords.
KEYWORD_SENTENCES = {
    "eng": ["Ethereal", "Exhaust", "Innate", "Retain", "Unplayable"],
    "fra": ["Éthérée", "Éthéré", "Épuisement", "Inné", "Innée", "Conserver", "Injouable"],
}
GOLD = {
    "eng": ["Block", "Vulnerable", "Weak", "Strength", "Dexterity", "Exhausted", "Exhaust", "Frail",
            "Wounds", "Wound", "Burn", "Dazed", "Status", "Curse"],
    "fra": ["Armure", "Vulnérable", "Vulnerabilité", "Faiblesse", "Force", "Dextérité", "Épuisée",
            "Épuisez", "Plaie", "Brûlure", "Hébétement", "Statut", "Malédiction"],
}


def class_name(sts1_class: str) -> str:
    # Acronyms such as JAX need PascalCase: the game's slugger splits adjacent capitals differently.
    name = sts1_class.title() if sts1_class.isupper() else sts1_class
    return "Sts1" + name.replace("_", "")


def convert_text(text: str, lang: str) -> tuple:
    """StS1 card text → (StS2 SmartFormat text, number of energy icons)."""
    sentences = "|".join(re.escape(w) for w in KEYWORD_SENTENCES[lang])
    text = re.sub(rf"(^|\bNL\s+|\s)(?:{sentences})\s*\.(?=\s|$)", r"\1", text)
    energy = len(re.findall(r"\[R\]", text))
    text = re.sub(r"\[R\](?:\s*\[R\])*", "{Energy:energyIcons()}", text)
    text = text.replace("!D!", "{Damage:diff()}").replace("!B!", "{Block:diff()}").replace("!M!", "{Magic:diff()}")
    text = re.sub(r"\*(\w+)", r"[gold]\1[/gold]", text)
    words = "|".join(GOLD[lang])
    text = re.sub(rf"(?<![\w\[{{])({words})(?![\w\]])(?![^\[]*\[/gold\])", r"[gold]\1[/gold]", text)
    text = re.sub(r"\s*\bNL\b\s*", "\n", text)
    text = re.sub(r"[ \t]+", " ", text)
    text = re.sub(r" ([.,])", r"\1", text)
    lines = [line.strip() for line in text.split("\n")]
    return "\n".join(line for line in lines if line), energy


def energy(card: dict) -> tuple:
    """(base energy icons, upgrade delta) read from the English text."""
    t = card["text"]["eng"]
    _, base = convert_text(t["description"], "eng")
    if not t["upgrade_description"]:
        return base, 0
    _, upgraded = convert_text(t["upgrade_description"], "eng")
    return base, upgraded - base


def card_texts(card: dict, lang: str) -> dict:
    t = card["text"][lang]
    base, _ = convert_text(t["description"] or "", lang)
    description = base
    if t["upgrade_description"]:
        upgraded, _ = convert_text(t["upgrade_description"], lang)
        if upgraded != base:
            description = "{IfUpgraded:show:" + upgraded + "|" + base + "}"
    return {"title": t["name"], "description": description}


def skeleton(card: dict, character: str, row: dict | None = None) -> str:
    name = class_name(card["class"])
    cost = card["cost"]
    x_cost = cost == -1
    keywords = [k for k in card["keywords"] if k != "unplayable" or cost == -2]
    variables = []
    if card["damage"] is not None:
        variables.append(f"new DamageVar({card['damage']}m, ValueProp.Move)")
    if card["block"] is not None:
        variables.append(f"new BlockVar({card['block']}m, ValueProp.Move)")
    if card["magic"] is not None:
        variables.append(f'new DynamicVar("Magic", {card["magic"]}m)')
    base_energy, energy_up = energy(card)
    if base_energy:
        variables.append(f"new EnergyVar({base_energy})")

    upgrade = []
    if card["upgrade"]["damage"]:
        upgrade.append(f"DynamicVars.Damage.UpgradeValueBy({card['upgrade']['damage']}m);")
    if card["upgrade"]["block"]:
        upgrade.append(f"DynamicVars.Block.UpgradeValueBy({card['upgrade']['block']}m);")
    if card["upgrade"]["magic"]:
        upgrade.append(f'DynamicVars["Magic"].UpgradeValueBy({card["upgrade"]["magic"]}m);')
    if energy_up:
        upgrade.append(f"DynamicVars.Energy.UpgradeValueBy({energy_up}m);")
    if card["cost_upgraded"] is not None and cost >= 0:
        upgrade.append(f"EnergyCost.UpgradeBy({card['cost_upgraded'] - cost});")
    for k in card["keywords"]:
        if k not in card["keywords_upgraded"]:
            upgrade.append(f"RemoveKeyword(CardKeyword.{KEYWORDS[k]});")
    for k in card["keywords_upgraded"]:
        if k not in card["keywords"]:
            upgrade.append(f"AddKeyword(CardKeyword.{KEYWORDS[k]});")

    body = []
    if row and row.get("sts2") and row["relation"] in ("same-sts1", "duplicate"):
        relation = "SameKeepSts1" if row["relation"] == "same-sts1" else "Duplicate"
        body.append(f"\tpublic override SlaySpoon.Core.Sts1Relation Relation => SlaySpoon.Core.Sts1Relation.{relation};\n")
        body.append(f"\tpublic override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.{row['sts2']});\n")
    if "Strike" in (card["text"]["eng"]["name"] or ""):
        body.append("\tprotected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };\n")
    if keywords:
        listed = ", ".join(f"CardKeyword.{KEYWORDS[k]}" for k in keywords)
        body.append(f"\tpublic override IEnumerable<CardKeyword> CanonicalKeywords => new[] {{ {listed} }};\n")
    if x_cost:
        body.append("\tprotected override bool HasEnergyCostX => true;\n")
    if variables:
        listed = "".join(f"\t\t{v},\n" for v in variables)
        body.append(f"\tprotected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]\n\t{{\n{listed}\t}};\n")
    ctor_cost = 0 if x_cost or cost < 0 else cost
    body.append(
        f"\tpublic {name}()\n"
        f"\t\t: base({ctor_cost}, CardType.{TYPES[card['type']]}, CardRarity.{RARITIES[card['rarity']]}, "
        f"TargetType.{TARGETS[card['target']]})\n\t{{\n\t}}\n"
    )
    body.append(
        "\tprotected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)\n"
        "\t{\n\t\tthrow new NotImplementedException(\"STS1 port\");\n\t}\n"
    )
    if upgrade:
        lines = "".join(f"\t\t{u}\n" for u in upgrade)
        body.append(f"\tprotected override void OnUpgrade()\n\t{{\n{lines}\t}}\n")

    return (
        "using MegaCrit.Sts2.Core.Entities.Cards;\n"
        "using MegaCrit.Sts2.Core.GameActions.Multiplayer;\n"
        "using MegaCrit.Sts2.Core.Localization.DynamicVars;\n"
        "using MegaCrit.Sts2.Core.ValueProps;\n\n"
        f"namespace SlaySpoon.Cards.Sts1.{character};\n\n"
        f"public sealed class {name} : Sts1{character}Card\n{{\n"
        + "\n".join(body)
        + "}\n"
    )


def generate(cards: list, rows: list, character: str, src: Path, loc: Path, relations: set) -> list:
    wanted = {r["sts1"] for r in rows if r["relation"] in relations}
    by_class = {r["sts1"]: r for r in rows}
    src.mkdir(parents=True, exist_ok=True)
    created = []
    tables = {}
    for lang in ("eng", "fra"):
        path = loc / lang / "cards.json"
        tables[lang] = json.loads(path.read_text(encoding="utf-8")) if path.exists() else {}
    for card in cards:
        if card["class"] not in wanted:
            continue
        name = class_name(card["class"])
        path = src / f"{name}.cs"
        if not path.exists():
            path.write_text(skeleton(card, character, by_class[card["class"]]), encoding="utf-8")
            created.append(path)
        key = match.slug(name)
        for lang in ("eng", "fra"):
            for field, value in card_texts(card, lang).items():
                tables[lang].setdefault(f"{key}.{field}", value)
    for lang, table in tables.items():
        path = loc / lang / "cards.json"
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(dict(sorted(table.items())), indent="\t", ensure_ascii=False) + "\n",
                        encoding="utf-8")
    return created


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--sts1", type=Path, required=True)
    ap.add_argument("--table", type=Path, required=True)
    ap.add_argument("--character", required=True)
    ap.add_argument("--src", type=Path, required=True)
    ap.add_argument("--loc", type=Path, required=True)
    ap.add_argument("--relations", default="unique")
    args = ap.parse_args()
    cards = json.loads(args.sts1.read_text(encoding="utf-8"))
    rows = json.loads(args.table.read_text(encoding="utf-8"))
    created = generate(cards, rows, args.character, args.src, args.loc, set(args.relations.split(",")))
    print(f"{len(created)} new skeletons")


if __name__ == "__main__":
    main()
