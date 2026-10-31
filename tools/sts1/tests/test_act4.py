import hashlib
import json
import zipfile

import pytest

import act4


def make_jar(path, missing=None):
    with zipfile.ZipFile(path, "w") as archive:
        for name in act4.ASSETS:
            if name == missing:
                continue
            data = b"test asset"
            if name.endswith("/skeleton.json"):
                data = act4.json_bytes({
                    "skeleton": {"spine": "3.4.02"},
                    "animations": {"idle": {}, "attack": {}},
                })
            archive.writestr(name, data)
        for lang in act4.LANGUAGES:
            for table, keys in (("monsters", act4.MONSTERS), ("powers", act4.POWERS), ("ui", ("TheEnding",))):
                archive.writestr(f"localization/{lang}/{table}.json",
                                 act4.json_bytes({key: {"NAME": f"{lang} {key}"} for key in keys}))
        archive.writestr("com/megacrit/cardcrawl/monsters/ending/CorruptHeart.class", b"not for export")
        archive.writestr("../escape.txt", b"not for export")


def test_only_curated_resources_are_imported_with_verified_provenance(tmp_path):
    jar, out = tmp_path / "game.jar", tmp_path / "out"
    make_jar(jar)
    manifest = act4.import_act4(jar, out)
    assert len(manifest["assets"]) == 25
    for item in manifest["assets"] + manifest["localization"]:
        data = (out / item["path"]).read_bytes()
        assert hashlib.sha256(data).hexdigest() == item["sha256"]
    assert not list(out.rglob("*.class"))
    assert not (tmp_path / "escape.txt").exists()
    assert manifest["animations"][0]["animations"] == ["attack", "idle"]
    assert {a["spine_version"] for a in manifest["animations"]} == {"3.4.02"}
    for lang in act4.LANGUAGES:
        data = json.loads((out / "localization" / f"{lang}.json").read_text())
        assert data["monsters"]["CorruptHeart"]["NAME"] == f"{lang} CorruptHeart"


def test_reimport_is_byte_identical(tmp_path):
    jar, out = tmp_path / "game.jar", tmp_path / "out"
    make_jar(jar)
    act4.import_act4(jar, out)
    before = {p.relative_to(out): p.read_bytes() for p in out.rglob("*") if p.is_file()}
    act4.import_act4(jar, out)
    after = {p.relative_to(out): p.read_bytes() for p in out.rglob("*") if p.is_file()}
    assert before == after


def test_missing_resource_leaves_existing_import_untouched(tmp_path):
    jar, out = tmp_path / "game.jar", tmp_path / "out"
    make_jar(jar)
    act4.import_act4(jar, out)
    before = {p.relative_to(out): p.read_bytes() for p in out.rglob("*") if p.is_file()}
    make_jar(jar, missing=act4.ASSETS[-1])
    with pytest.raises(KeyError):
        act4.import_act4(jar, out)
    assert before == {p.relative_to(out): p.read_bytes() for p in out.rglob("*") if p.is_file()}


def test_key_sprites_match_the_import():
    root = __import__("pathlib").Path(__file__).resolve().parents[3]
    source = root / "data/sts1/act4/assets/images"
    keys = root / "godot/SlaySpoon/images/keys"
    sprites = {f"key_{c}.png": source / "ui/topPanel" for c in ("red", "green", "blue")}
    sprites.update({f"{k}_key.png": source / "largeRelics" for k in ("ruby", "emerald", "sapphire")})
    for name, folder in sprites.items():
        assert (keys / name).read_bytes() == (folder / name).read_bytes(), name
