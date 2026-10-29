import json

from conftest import FIXTURES
import codegen
import extract


def card(cls):
    return next(c for c in extract.extract(FIXTURES, "red") if c["class"] == cls)


def test_text_conversion():
    text, energy = codegen.convert_text("Lose 3 HP. NL Gain [R] [R].", "eng")
    assert text == "Lose 3 HP.\nGain {Energy:energyIcons()}."
    assert energy == 2


def test_values_and_keyword_sentences():
    text, energy = codegen.convert_text("Ethereal. NL Deal !D! damage. NL Apply !M! Vulnerable. NL Exhaust.", "eng")
    assert text == "Deal {Damage:diff()} damage.\nApply {Magic:diff()} [gold]Vulnerable[/gold]."
    assert energy == 0
    text, _ = codegen.convert_text("Éthérée. NL Gagnez !B! d'Armure. NL Épuisement.", "fra")
    assert text == "Gagnez {Block:diff()} d'[gold]Armure[/gold]."


def test_gold_words_skip_placeholders():
    text, _ = codegen.convert_text("Gain !B! Block.", "eng")
    assert text == "Gain {Block:diff()} [gold]Block[/gold]."


def test_status_star_markup():
    text, _ = codegen.convert_text("Shuffle a *Wound into your draw pile.", "eng")
    assert text == "Shuffle a [gold]Wound[/gold] into your draw pile."


def test_upgrade_text_switch():
    texts = codegen.card_texts(card("FakeGuard"), "eng")
    # Only the keyword sentence differs, and keywords are stripped: no switch needed.
    assert "IfUpgraded" not in texts["description"]
    c = card("FakeSlash")
    c["text"]["eng"]["upgrade_description"] = "Deal !D! damage twice."
    assert codegen.card_texts(c, "eng")["description"] == \
        "{IfUpgraded:show:Deal {Damage:diff()} damage twice.|Deal {Damage:diff()} damage.\nApply {Magic:diff()} [gold]Vulnerable[/gold].}"


def test_energy_upgrade_becomes_var_delta():
    c = card("FakeSlash")
    c["text"]["eng"]["description"] = "Gain [R] [R]."
    c["text"]["eng"]["upgrade_description"] = "Gain [R] [R] [R]."
    assert codegen.card_texts(c, "eng")["description"] == "Gain {Energy:energyIcons()}."
    assert codegen.energy(c) == (2, 1)


def test_skeleton_shape():
    src = codegen.skeleton(card("FakeSlash"), "Ironclad")
    assert "public sealed class Sts1FakeSlash : Sts1IroncladCard" in src
    assert "base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)" in src
    assert "new DamageVar(9m, ValueProp.Move)" in src
    assert 'new DynamicVar("Magic", 2m)' in src
    assert "DynamicVars.Damage.UpgradeValueBy(4m);" in src
    assert 'DynamicVars["Magic"].UpgradeValueBy(1m);' in src
    assert "EnergyCost.UpgradeBy(-1);" in src
    assert 'throw new NotImplementedException("STS1 port");' in src
    guard = codegen.skeleton(card("FakeGuard"), "Ironclad")
    assert "CardKeyword.Ethereal, CardKeyword.Exhaust" in guard
    assert "RemoveKeyword(CardKeyword.Exhaust);" in guard
    assert "TargetType.Self" in guard
    storm = codegen.skeleton(card("FakeStorm"), "Ironclad")
    assert "HasEnergyCostX => true" in storm and "base(0," in storm
    assert "AddKeyword(CardKeyword.Retain);" in storm


def test_strike_tag():
    c = card("FakeSlash")
    c["text"]["eng"]["name"] = "Wild Strike"
    assert "CardTag.Strike" in codegen.skeleton(c, "Ironclad")
    assert "CardTag.Strike" not in codegen.skeleton(card("FakeGuard"), "Ironclad")


def test_never_overwrites_existing_file(tmp_path):
    src_dir = tmp_path / "src"
    loc = tmp_path / "loc"
    for lang in ("eng", "fra"):
        (loc / lang).mkdir(parents=True)
        (loc / lang / "cards.json").write_text(json.dumps({"STS1_FAKE_SLASH.title": "Kept"}))
    src_dir.mkdir()
    (src_dir / "Sts1FakeSlash.cs").write_text("// hand ported")
    rows = [{"sts1": "FakeSlash", "relation": "unique"}, {"sts1": "FakeGuard", "relation": "same-sts2"}]
    created = codegen.generate(extract.extract(FIXTURES, "red"), rows, "Ironclad", src_dir, loc, {"unique"})
    assert created == []
    assert (src_dir / "Sts1FakeSlash.cs").read_text() == "// hand ported"
    eng = json.loads((loc / "eng" / "cards.json").read_text())
    assert eng["STS1_FAKE_SLASH.title"] == "Kept"
    assert eng["STS1_FAKE_SLASH.description"].startswith("Deal {Damage:diff()} damage.")
    assert not (src_dir / "Sts1FakeGuard.cs").exists()
