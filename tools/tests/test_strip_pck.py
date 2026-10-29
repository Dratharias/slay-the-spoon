import struct
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import strip_pck

REL_FILEBASE = 2


def write_pck(path, files, version=3, flags=REL_FILEBASE):
    """A Godot 4.5 PCK: header, 16-byte aligned file data, directory at the end."""
    header_size = 0x70
    data = bytearray()
    entries = []
    for name, content in files:
        while len(data) % 16:
            data.append(0)
        entries.append((name, len(data), len(content)))
        data += content
    while len(data) % 16:
        data.append(0)
    dir_offset = header_size + len(data)
    head = struct.pack("<4sIIIII", b"GDPC", version, 4, 5, 1, flags)
    head += struct.pack("<QQ", header_size, dir_offset)
    head += bytes(header_size - len(head))  # reserved words, padded to 16 bytes like Godot's
    directory = struct.pack("<I", len(entries))
    for name, offset, size in entries:
        raw = name.encode()
        raw += bytes(-len(raw) % 4)
        directory += struct.pack("<I", len(raw)) + raw + struct.pack("<QQ", offset, size) + bytes(16)
        directory += struct.pack("<I", 0)
    path.write_bytes(head + bytes(data) + directory)


def read_files(path):
    blob = path.read_bytes()
    file_base, dir_offset = struct.unpack_from("<QQ", blob, 0x18)
    (count,) = struct.unpack_from("<I", blob, dir_offset)
    pos = dir_offset + 4
    files = {}
    for _ in range(count):
        (length,) = struct.unpack_from("<I", blob, pos)
        name = blob[pos + 4:pos + 4 + length].rstrip(b"\0").decode()
        pos += 4 + length
        offset, size = struct.unpack_from("<QQ", blob, pos)
        pos += 16 + 16 + 4
        files[name] = blob[file_base + offset:file_base + offset + size]
    assert pos == len(blob)
    return files


FILES = [
    (".godot/global_script_class_cache.cfg", b"list=[]\n"),
    (".godot/imported/card.png-abc.ctex", b"GST2" + bytes(range(200))),
    (".godot/uid_cache.bin", b"\x01\x02\x03"),
    ("SlaySpoon/localization/eng/cards.json", b'{"A": "B"}'),
    ("project.binary", b"ECFG" + bytes(50)),
]


def test_removes_godot_project_files_and_keeps_the_rest(tmp_path):
    pck = tmp_path / "mod.pck"
    write_pck(pck, FILES)
    before = pck.stat().st_size

    removed = strip_pck.strip(pck, strip_pck.GODOT_PROJECT_FILES)

    assert removed == [".godot/global_script_class_cache.cfg", "project.binary"]
    assert read_files(pck) == {name: content for name, content in FILES
                               if name not in ("project.binary", ".godot/global_script_class_cache.cfg")}
    assert pck.stat().st_size < before


def test_kept_files_stay_16_byte_aligned(tmp_path):
    pck = tmp_path / "mod.pck"
    write_pck(pck, FILES)
    strip_pck.strip(pck, strip_pck.GODOT_PROJECT_FILES)
    blob = pck.read_bytes()
    _, dir_offset = struct.unpack_from("<QQ", blob, 0x18)
    (count,) = struct.unpack_from("<I", blob, dir_offset)
    pos = dir_offset + 4
    for _ in range(count):
        (length,) = struct.unpack_from("<I", blob, pos)
        offset, _ = struct.unpack_from("<QQ", blob, pos + 4 + length)
        assert offset % 16 == 0
        pos += 4 + length + 36


def test_nothing_to_remove_leaves_the_file_unchanged(tmp_path):
    pck = tmp_path / "mod.pck"
    write_pck(pck, FILES[1:4])
    before = pck.read_bytes()
    assert strip_pck.strip(pck, strip_pck.GODOT_PROJECT_FILES) == []
    assert pck.read_bytes() == before


@pytest.mark.parametrize("version,flags", [(2, REL_FILEBASE), (3, REL_FILEBASE | 1), (3, 0)])
def test_refuses_formats_it_does_not_rewrite(tmp_path, version, flags):
    pck = tmp_path / "mod.pck"
    write_pck(pck, FILES, version=version, flags=flags)
    before = pck.read_bytes()
    with pytest.raises(ValueError):
        strip_pck.strip(pck, strip_pck.GODOT_PROJECT_FILES)
    assert pck.read_bytes() == before
