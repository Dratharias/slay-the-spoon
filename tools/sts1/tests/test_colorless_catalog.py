"""The complete source catalog includes event cards without losing their rarity boundary."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]


def test_colorless_catalog_coverage_and_event_cards():
    cards = json.loads((ROOT / 'data/sts1/colorless.json').read_text())
    rows = json.loads((ROOT / 'data/duplicates/colorless.json').read_text())
    names = {c['class'] for c in cards}
    assert len(cards) == len(names) == 39
    assert {r['sts1'] for r in rows} == names
    assert len(rows) == 39
    assert {c['class'] for c in cards if c['rarity'] == 'special'} == {
        'Apparition', 'Bite', 'JAX', 'RitualDagger',
    }
    assert sum(c['rarity'] in ('uncommon', 'rare') for c in cards) == 35
    for card in cards:
        for lang in ('eng', 'fra'):
            assert card['text'][lang]['name']
            assert card['text'][lang]['description']
