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
    {"class": "FakeGuard", "snake": "fake_guard", "portrait": "red/skill/fake_guard"},
]


def test_resizes_to_card_atlas_size(tmp_path):
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/attack/fake_slash", "red/skill/fake_guard"])
    rows = [{"sts1": "FakeSlash", "relation": "unique"}, {"sts1": "FakeGuard", "relation": "duplicate"}]
    written = art.export(jar, CARDS, rows, tmp_path / "out")
    assert sorted(p.name for p in written) == ["sts1_fake_guard.png", "sts1_fake_slash.png"]
    img = Image.open(tmp_path / "out" / "sts1_fake_slash.png")
    assert img.size == (250, 190)
    assert img.mode == "RGBA"


def test_skips_same_sts2(tmp_path):
    jar = tmp_path / "game.jar"
    make_jar(jar, ["red/attack/fake_slash", "red/skill/fake_guard"])
    rows = [{"sts1": "FakeSlash", "relation": "same-sts2"}, {"sts1": "FakeGuard", "relation": "same-sts1"}]
    written = art.export(jar, CARDS, rows, tmp_path / "out")
    assert [p.name for p in written] == ["sts1_fake_guard.png"]
