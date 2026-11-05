"""Catalog and generation checks; source classes are read locally, never copied into the repo."""
import io
import json
import zipfile
from pathlib import Path

import pytest
from PIL import Image

import art
import codegen
import extract

ROOT = Path(__file__).resolve().parents[3]


def test_purple_follows_card_library():
    decomp = Path('/sts1')
    if not decomp.is_dir():
        pytest.skip('local StS1 decompile is not mounted')
    cards = extract.extract(decomp, 'purple')
    names = {c['class'] for c in cards}
    assert len(cards) == 75
    assert names == extract.purple_allowlist(decomp)
    assert not names & {'Discipline', 'Unraveling'}
    assert cards == json.loads((ROOT / 'data/sts1/watcher.json').read_text())


def test_watcher_generated_cards():
    cards = json.loads((ROOT / 'data/sts1/watcher-generated.json').read_text())
    assert {c['class'] for c in cards} == set(extract.WATCHER_TOKENS + extract.WATCHER_OPTIONS)
    assert len(cards) == 13
    assert 'Shiv' not in {c['class'] for c in cards}
    for c in cards:
        assert c['text']['eng']['name'] and c['text']['fra']['name']
        portrait = ROOT / 'godot/SlaySpoon/images/cards/watchertokens' / (art.match.slug(codegen.class_name(c['class'])).lower() + '.png')
        assert Image.open(portrait).size == art.SIZE


def test_registry_is_fail_closed(tmp_path):
    path = tmp_path / 'com/megacrit/cardcrawl/helpers/CardLibrary.java'
    path.parent.mkdir(parents=True)
    path.write_text('class CardLibrary { private static void addPurpleCards() { CardLibrary.add(new Fake()); } }')
    with pytest.raises(ValueError, match='75 registered'):
        extract.purple_allowlist(tmp_path)


def test_watcher_starter_names_keep_existing_model_ids():
    assert codegen.class_name('Strike_Purple') == 'Sts1StrikeWatcher'
    assert codegen.class_name('Defend_Watcher') == 'Sts1DefendWatcher'


def test_relic_icons_include_small_large_and_outline(tmp_path):
    jar = tmp_path / 'assets.jar'
    with zipfile.ZipFile(jar, 'w') as z:
        for stem in art.WATCHER_RELICS.values():
            for folder, size in [('relics', (128, 128)), ('relics/outline', (128, 128)), ('largeRelics', (256, 256))]:
                data = io.BytesIO()
                Image.new('RGBA', size, (20, 40, 60, 255)).save(data, 'PNG')
                z.writestr(f'images/{folder}/{stem}.png', data.getvalue())
    paths = art.export_relics(jar, tmp_path / 'out')
    assert len(paths) == 27
    assert len(set(paths)) == 27
    for path in paths:
        assert Image.open(path).size == ((256, 256) if '_large' in path.stem else (128, 128))


def test_every_watcher_card_is_ported():
    # Every Watcher card is ported: no staged skeleton is left and no port throws.
    pending = list((ROOT / 'data/sts1/watcher-skeletons').glob('*.cs'))
    assert pending == []
    assert all('NotImplementedException' not in p.read_text()
               for p in (ROOT / 'src/Cards/Sts1/Watcher').glob('*.cs'))


def test_energy_converter_supports_watcher_orbs():
    text, count = codegen.convert_text("Gain [W] [W].", "eng")
    assert text == "Gain {Energy:energyIcons()}."
    assert count == 2


def test_watcher_item_localization_and_art():
    # Check what the packaged items actually resolve, including fallback-free relic art.
    for lang in ('eng', 'fra'):
        relics = json.loads((ROOT / f'godot/SlaySpoon/localization/{lang}/relics.json').read_text())
        potions = json.loads((ROOT / f'godot/SlaySpoon/localization/{lang}/potions.json').read_text())
        for cls in art.WATCHER_RELICS:
            if cls == 'CloakClasp':
                continue  # Native shared relic, native text and art.
            key = art.match.slug('Sts1' + cls)
            for field in ('title', 'description', 'flavor'):
                assert relics[key + '.' + field].strip()
            for suffix in ('', '_outline', '_large'):
                path = ROOT / 'godot/SlaySpoon/images/relics/watcher' / (key.lower() + suffix + '.png')
                assert Image.open(path).mode == 'RGBA'
        for key in ('STS1_BOTTLED_MIRACLE', 'STS1_STANCE_POTION', 'STS1_AMBROSIA'):
            assert potions[key + '.title'] and potions[key + '.description']
