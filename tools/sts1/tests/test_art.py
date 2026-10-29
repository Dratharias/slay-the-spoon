import io
import zipfile

from PIL import Image

import art


def make_jar(path, portraits):
    with zipfile.ZipFile(path, "w") as z:
        for name in portraits:
            buf = io.BytesIO()
            Image.new("RGB", (500, 380), (200, 30, 30)).save(buf, "PNG")
            z.writestr(f"images/1024Portraits/{name}.png", buf.getvalue())


CARDS = [
    {"class": "FakeSlash", "snake": "fake_slash", "portrait": "red/attack/fake_slash"},
    {"class": "FakeGuard", "snake": "fakeguard", "portrait": "red/skill/fakeguard"},
]


def test_resizes_to_card_atlas_size(tmp_path):
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/attack/fake_slash", "red/skill/fakeguard"])
    rows = [{"sts1": "FakeSlash", "relation": "unique"}, {"sts1": "FakeGuard", "relation": "duplicate"}]
    written = art.export(jar, CARDS, rows, tmp_path / "out")
    assert sorted(p.name for p in written) == ["sts1_fake_guard.png", "sts1_fake_slash.png"]
    img = Image.open(tmp_path / "out" / "sts1_fake_slash.png")
    assert img.size == (250, 190)
    assert img.mode == "RGBA"


def test_skips_same_sts2(tmp_path):
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/attack/fake_slash", "red/skill/fakeguard"])
    rows = [{"sts1": "FakeSlash", "relation": "same-sts2"}, {"sts1": "FakeGuard", "relation": "same-sts1"}]
    written = art.export(jar, CARDS, rows, tmp_path / "out")
    assert [p.name for p in written] == ["sts1_fake_guard.png"]


def test_named_after_the_model_id_not_the_sts1_art_name(tmp_path):
    # PortraitPath is built from the card's ModelDb id; StS1 art names don't always split words the same way.
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/skill/fakeguard"])
    written = art.export(jar, CARDS[1:], [{"sts1": "FakeGuard", "relation": "unique"}], tmp_path / "out")
    assert [p.name for p in written] == ["sts1_fake_guard.png"]


def test_events_export_named_after_the_event_model_id(tmp_path):
    jar = tmp_path / "game.jar"
    with zipfile.ZipFile(jar, "w") as z:
        for name in ("vampires", "drugDealer", "theNest"):
            buf = io.BytesIO()
            Image.new("RGB", (600, 600), (20, 20, 60)).save(buf, "JPEG")
            z.writestr(f"images/events/{name}.jpg", buf.getvalue())
    written = art.export_events(jar, tmp_path / "out")
    assert sorted(p.name for p in written) == ["sts1_augmenter.png", "sts1_nest.png", "sts1_vampires.png"]
    img = Image.open(tmp_path / "out" / "sts1_nest.png")
    assert img.size == (600, 600)
