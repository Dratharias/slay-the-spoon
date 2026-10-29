#!/usr/bin/env python3
"""Remove files from a Godot 4.5 PCK in place.

`godot --export-pack` always adds the packing project's own `project.binary` and
`.godot/global_script_class_cache.cfg`. The mod's PCK is mounted over the game's files, so those two
shadow the game's copies; they are dropped. `.godot/uid_cache.bin` stays: Godot merges it when it
loads the pack.

Usage: strip_pck.py <file.pck>   (stdlib only, runs on the host after the export)
"""
import struct
import sys
from pathlib import Path

GODOT_PROJECT_FILES = ("project.binary", ".godot/global_script_class_cache.cfg")

MAGIC = b"GDPC"
FORMAT_VERSION = 3  # Godot 4.5: directory after the file data, its offset in the header
DIR_ENCRYPTED = 1
REL_FILEBASE = 2
HEADER = struct.Struct("<4sIIIIIQQ")  # magic, format, major, minor, patch, flags, file base, dir offset
ALIGNMENT = 16


def strip(path, names):
    """Rewrite the PCK at `path` without the entries in `names`; return the removed names in pack order."""
    path = Path(path)
    blob = path.read_bytes()
    magic, version, _, _, _, flags, file_base, dir_offset = HEADER.unpack_from(blob, 0)
    if magic != MAGIC or version != FORMAT_VERSION:
        raise ValueError(f"{path}: not a format {FORMAT_VERSION} PCK")
    if flags & DIR_ENCRYPTED or not flags & REL_FILEBASE:
        raise ValueError(f"{path}: encrypted directory or absolute offsets are not supported")

    (count,) = struct.unpack_from("<I", blob, dir_offset)
    pos = dir_offset + 4
    entries = []
    for _ in range(count):
        (length,) = struct.unpack_from("<I", blob, pos)
        raw_name = blob[pos + 4:pos + 4 + length]
        pos += 4 + length
        offset, size = struct.unpack_from("<QQ", blob, pos)
        tail = blob[pos + 16:pos + 16 + 20]  # md5 + flags, copied as is
        pos += 36
        entries.append((raw_name.rstrip(b"\0").decode(), raw_name, offset, size, tail))

    removed = [e[0] for e in entries if e[0] in names]
    if not removed:
        return []

    kept = sorted((e for e in entries if e[0] not in names), key=lambda e: e[2])
    data = bytearray()
    directory = bytearray(struct.pack("<I", len(kept)))
    for _, raw_name, offset, size, tail in kept:
        data += bytes(-len(data) % ALIGNMENT)
        directory += struct.pack("<I", len(raw_name)) + raw_name + struct.pack("<QQ", len(data), size) + tail
        data += blob[file_base + offset:file_base + offset + size]
    data += bytes(-len(data) % ALIGNMENT)

    header = bytearray(blob[:file_base])
    struct.pack_into("<Q", header, HEADER.size - 8, file_base + len(data))
    path.write_bytes(bytes(header) + bytes(data) + bytes(directory))
    return removed


if __name__ == "__main__":
    for name in strip(sys.argv[1], GODOT_PROJECT_FILES):
        print(f"strip_pck: removed {name}")
