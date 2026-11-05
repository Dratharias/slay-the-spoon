"""Convert StS1's Spine 3.4 JSON skeletons to the Spine 4.2 JSON that StS2's spine-godot runtime reads.

Usage: python spine34to42.py in.json out.spine-json [--anim NEW=OLD[@SPEED]]...

--anim exposes the 3.4 animation OLD under the name NEW (the names StS2's creature animator plays: idle_loop, attack,
hurt...), its timeline times divided by SPEED, which bakes StS1's per-track time scale into the data. Without any --anim
every animation keeps its name and speed.

Only the features the StS1 skeletons use are handled (the converter refuses anything else rather than guessing):
bones (with 3.4's inheritRotation/inheritScale flags), slots, IK and path constraints, region/mesh/path attachments,
events, and animations with bone, slot (attachment, color), deform, draw order and event timelines.

Format changes from 3.4 to 4.2:
- skins: an object keyed by skin name becomes a list of {name, attachments};
- bones: inheritRotation/inheritScale become one "inherit" mode;
- path constraints: rotateMix/translateMix become mixRotate/mixX;
- rotate keys: "angle" becomes "value"; 3.4 interpolated between two keys along the shortest arc, 4.2 interpolates the
  stored values, so the keys are unwrapped (each delta brought into [-180, 180]);
- curves: 3.4 stores one normalized bezier [cx1, cy1, cx2, cy2] per key, 4.2 stores absolute (time, value) control
  points, four numbers per channel;
- slot "color" timelines become "rgba"; animation "deform" moves under "attachments" as a "deform" timeline.
"""
import argparse
import json
import sys
from pathlib import Path

SPINE_VERSION = "4.2.43"


class Unsupported(ValueError):
    pass


def _check_keys(where: str, obj: dict, allowed: set) -> None:
    extra = set(obj) - allowed
    if extra:
        raise Unsupported(f"{where}: unsupported keys {sorted(extra)}")


def _inherit(bone: dict) -> str:
    rotation = bone.get("inheritRotation", True)
    scale = bone.get("inheritScale", True)
    if rotation and scale:
        return "normal"
    if not rotation and not scale:
        return "onlyTranslation"
    return "noRotationOrReflection" if not rotation else "noScale"


def convert_bone(bone: dict) -> dict:
    _check_keys(f"bone {bone.get('name')}", bone, {
        "name", "parent", "length", "x", "y", "rotation", "scaleX", "scaleY", "shearX", "shearY", "color",
        "inheritRotation", "inheritScale"})
    out = {k: v for k, v in bone.items() if k not in ("inheritRotation", "inheritScale")}
    inherit = _inherit(bone)
    if inherit != "normal":
        out["inherit"] = inherit
    return out


def convert_slot(slot: dict) -> dict:
    _check_keys(f"slot {slot.get('name')}", slot, {"name", "bone", "color", "attachment", "blend"})
    return dict(slot)


def convert_ik(ik: dict) -> dict:
    _check_keys(f"ik {ik.get('name')}", ik, {"name", "bones", "target", "mix", "bendPositive"})
    return dict(ik)


def convert_path(path: dict) -> dict:
    _check_keys(f"path {path.get('name')}", path, {
        "name", "bones", "target", "positionMode", "spacingMode", "rotateMode", "rotation", "position", "spacing",
        "rotateMix", "translateMix"})
    out = {k: v for k, v in path.items() if k not in ("rotateMix", "translateMix")}
    if "rotateMix" in path:
        out["mixRotate"] = path["rotateMix"]
    if "translateMix" in path:
        out["mixX"] = path["translateMix"]
    return out


ATTACHMENT_KEYS = {
    "region": {"type", "name", "path", "x", "y", "scaleX", "scaleY", "rotation", "width", "height", "color"},
    "mesh": {"type", "name", "path", "uvs", "triangles", "vertices", "hull", "edges", "width", "height", "color"},
    "path": {"type", "name", "closed", "constantSpeed", "lengths", "vertexCount", "vertices", "color"},
}


def convert_attachment(where: str, att: dict) -> dict:
    kind = att.get("type", "region")
    if kind not in ATTACHMENT_KEYS:
        raise Unsupported(f"{where}: attachment type {kind}")
    _check_keys(where, att, ATTACHMENT_KEYS[kind])
    return dict(att)


def convert_skins(skins: dict) -> list:
    if not isinstance(skins, dict):
        raise Unsupported("skins: expected the 3.4 object form")
    out = []
    for name, slots in skins.items():
        attachments = {
            slot: {att: convert_attachment(f"skin {name}/{slot}/{att}", value) for att, value in atts.items()}
            for slot, atts in slots.items()
        }
        out.append({"name": name, "attachments": attachments})
    return out


def _wrap(delta: float) -> float:
    """3.4's RotateTimeline brings a key-to-key delta into [-180, 180]."""
    return delta - (16384 - int(16384.499999999996 - delta / 360)) * 360


def _curve(curve, t1: float, t2: float, channels: list) -> object:
    """A 3.4 key curve to 4.2's: absolute control points for each (value1, value2) channel."""
    if curve == "stepped":
        return "stepped"
    if not (isinstance(curve, list) and len(curve) == 4):
        raise Unsupported(f"curve {curve!r}")
    cx1, cy1, cx2, cy2 = curve
    out = []
    for v1, v2 in channels:
        out += [t1 + cx1 * (t2 - t1), v1 + cy1 * (v2 - v1), t1 + cx2 * (t2 - t1), v1 + cy2 * (v2 - v1)]
    return [round(x, 6) for x in out]


def _keys(where: str, keys: list, allowed: set, values) -> list:
    """Copies keys with 4.2 curves; values(key) gives the key's channel values (after any rewrite)."""
    out = []
    for i, key in enumerate(keys):
        _check_keys(where, key, allowed | {"time", "curve"})
        out.append({k: v for k, v in key.items() if k != "curve"})
    for i, key in enumerate(keys):
        if "curve" not in key or i + 1 == len(keys):
            continue
        a, b = values(out[i]), values(out[i + 1])
        out[i]["curve"] = _curve(key["curve"], key.get("time", 0), keys[i + 1].get("time", 0), list(zip(a, b)))
    return out


def convert_rotate(where: str, keys: list) -> list:
    unwrapped = []
    prev = None
    for key in keys:
        angle = key.get("angle", 0)
        if prev is not None:
            angle = prev + _wrap(angle - prev)
        prev = angle
        k = {kk: vv for kk, vv in key.items() if kk != "angle"}
        k["value"] = round(angle, 6)
        unwrapped.append(k)
    return _keys(where, unwrapped, {"value"}, lambda k: [k["value"]])


def _xy(default: float):
    return lambda k: [k.get("x", default), k.get("y", default)]


def _rgba(hexstr: str) -> list:
    return [int(hexstr[i:i + 2], 16) / 255 for i in range(0, 8, 2)]


def convert_bone_timelines(name: str, timelines: dict) -> dict:
    out = {}
    for kind, keys in timelines.items():
        where = f"bone {name} {kind}"
        if kind == "rotate":
            out[kind] = convert_rotate(where, keys)
        elif kind == "translate" or kind == "shear":
            out[kind] = _keys(where, keys, {"x", "y"}, _xy(0))
        elif kind == "scale":
            out[kind] = _keys(where, keys, {"x", "y"}, _xy(1))
        else:
            raise Unsupported(where)
    return out


def convert_slot_timelines(name: str, timelines: dict) -> dict:
    out = {}
    for kind, keys in timelines.items():
        where = f"slot {name} {kind}"
        if kind == "attachment":
            for key in keys:
                _check_keys(where, key, {"time", "name"})
            out["attachment"] = [dict(k) for k in keys]
        elif kind == "color":
            out["rgba"] = _keys(where, keys, {"color"}, lambda k: _rgba(k["color"]))
        else:
            raise Unsupported(where)
    return out


def convert_deform(deform: dict) -> dict:
    out = {}
    for skin, slots in deform.items():
        for slot, atts in slots.items():
            for att, keys in atts.items():
                where = f"deform {skin}/{slot}/{att}"
                converted = _keys(where, keys, {"offset", "vertices"}, lambda k: [0.0])
                # A deform curve's channel runs from 0 to 1 (the blend between the two keys' vertices).
                for i, key in enumerate(keys):
                    if isinstance(key.get("curve"), list) and i + 1 < len(keys):
                        converted[i]["curve"] = _curve(key["curve"], key.get("time", 0), keys[i + 1].get("time", 0),
                                                       [(0.0, 1.0)])
                out.setdefault(skin, {}).setdefault(slot, {})[att] = {"deform": converted}
    return out


def convert_animation(name: str, anim: dict) -> dict:
    _check_keys(f"animation {name}", anim, {"bones", "slots", "deform", "drawOrder", "draworder", "events"})
    out = {}
    if "slots" in anim:
        out["slots"] = {s: convert_slot_timelines(s, t) for s, t in anim["slots"].items()}
    if "bones" in anim:
        out["bones"] = {b: convert_bone_timelines(b, t) for b, t in anim["bones"].items()}
    if "deform" in anim:
        out["attachments"] = convert_deform(anim["deform"])
    order = anim.get("drawOrder", anim.get("draworder"))
    if order is not None:
        out["drawOrder"] = order
    if "events" in anim:
        out["events"] = anim["events"]
    return out


def _scale_times(node, factor: float):
    """A copy of a 3.4 animation with every key time multiplied by factor (3.4 curves are normalized, so unaffected)."""
    if isinstance(node, dict):
        return {k: (round(v * factor, 6) if k == "time" else _scale_times(v, factor)) for k, v in node.items()}
    if isinstance(node, list):
        return [_scale_times(v, factor) for v in node]
    return node


def rename_animations(animations: dict, renames) -> dict:
    """renames: (new name, old name, speed) triples; the result holds only the renamed animations."""
    out = {}
    for new, old, speed in renames:
        if old not in animations:
            raise Unsupported(f"no animation {old!r} (have {sorted(animations)})")
        if new in out:
            raise Unsupported(f"animation {new!r} given twice")
        if speed <= 0:
            raise Unsupported(f"speed {speed} for {new!r}")
        out[new] = animations[old] if speed == 1 else _scale_times(animations[old], 1 / speed)
    return out


def parse_anim(spec: str):
    """'NEW=OLD' or 'NEW=OLD@SPEED' as (new, old, speed)."""
    new, sep, rest = spec.partition("=")
    if not sep or not new or not rest:
        raise argparse.ArgumentTypeError(f"expected NEW=OLD[@SPEED], got {spec!r}")
    old, _, speed = rest.partition("@")
    return new, old, float(speed) if speed else 1.0


def convert(data: dict, renames=None) -> dict:
    _check_keys("root", data, {"skeleton", "bones", "slots", "ik", "path", "skins", "events", "animations"})
    version = data.get("skeleton", {}).get("spine", "")
    if not version.startswith("3.4"):
        raise Unsupported(f"skeleton version {version!r}, expected 3.4")
    skeleton = {k: v for k, v in data["skeleton"].items() if k in ("hash", "width", "height", "images")}
    skeleton["spine"] = SPINE_VERSION
    out = {"skeleton": skeleton, "bones": [convert_bone(b) for b in data["bones"]]}
    out["slots"] = [convert_slot(s) for s in data.get("slots", [])]
    if "ik" in data:
        out["ik"] = [convert_ik(c) for c in data["ik"]]
    if "path" in data:
        out["path"] = [convert_path(c) for c in data["path"]]
    out["skins"] = convert_skins(data["skins"])
    if "events" in data:
        out["events"] = data["events"]
    animations = data.get("animations", {})
    if renames:
        animations = rename_animations(animations, renames)
    out["animations"] = {n: convert_animation(n, a) for n, a in animations.items()}
    return out


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("src", type=Path)
    ap.add_argument("dst", type=Path)
    ap.add_argument("--anim", type=parse_anim, action="append", default=[])
    args = ap.parse_args(argv)
    out = convert(json.loads(args.src.read_text(encoding="utf-8")), args.anim)
    args.dst.parent.mkdir(parents=True, exist_ok=True)
    args.dst.write_text(json.dumps(out, separators=(",", ":")), encoding="utf-8")
    return 0


if __name__ == "__main__":
    sys.exit(main())
