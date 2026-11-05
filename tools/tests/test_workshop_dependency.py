import ctypes
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import workshop_dependency

REPO = Path(__file__).resolve().parents[2]


def test_callback_struct_matches_steam_packing():
    # AddUGCDependencyResult_t: int32 + two uint64 with 4-byte packing = 20 bytes on Linux.
    assert ctypes.sizeof(workshop_dependency.AddDependencyResult) == 20


def test_manifest_dependency_has_a_required_item():
    manifest = json.loads((REPO / "mod/SlaySpoon.json").read_text(encoding="utf-8"))
    deps = {d["id"]: d for d in manifest["dependencies"]}
    assert "BaseLib" in deps and deps["BaseLib"]["min_version"]
    lines = (REPO / "workshop/required-items").read_text(encoding="utf-8").splitlines()
    items = {m.group(2): m.group(1) for line in lines if (m := re.match(r"^(\d+)\s+(\S+)", line))}
    assert set(items) == set(deps)
    assert items["BaseLib"] == "3737335127"
