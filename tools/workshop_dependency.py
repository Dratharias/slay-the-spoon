"""Set what SteamCMD cannot on the Slay the Spoon Workshop item: required items, tags and the French description.

Usage: python3 -I tools/workshop_dependency.py [--check] [--tags FILE] [--french-description FILE]
           <parent item id> [<required item id>...]

SteamCMD's workshop_build_item sets none of these, so this calls ISteamUGC through the game's own libsteam_api.so
(ctypes, no extra package): AddDependency (Steam's "Required items": subscribing to the parent offers the required
items too), SetItemTags (the game's category and language tags, one per line in FILE), and SetItemUpdateLanguage
"french" with SetItemTitle/SetItemDescription (Steam shows that description to players whose Steam is in French). It needs the Steam client running and logged in as the item's owner; Steam shows Slay the Spire 2 as
running for a moment. --check only initializes the Steam API and exits, without changing anything.
"""
import argparse
import ctypes
import os
import sys
import time
from pathlib import Path

APP_ID = 2868840
TITLE = "Slay the Spoon"
LIB = Path.home() / ".local/share/Steam/steamapps/common/Slay the Spire 2/data_sts2_linuxbsd_x86_64/libsteam_api.so"
ADD_UGC_DEPENDENCY_RESULT = 3412  # k_iSteamUGCCallbacks + 12
SUBMIT_ITEM_UPDATE_RESULT = 3404  # k_iSteamUGCCallbacks + 4
RESULT_OK = 1
RESULT_DUPLICATE_REQUEST = 29


class AddDependencyResult(ctypes.Structure):
    # Steam callback structs use 4-byte packing on Linux.
    _pack_ = 4
    _layout_ = "ms"
    _fields_ = [("result", ctypes.c_int32), ("parent", ctypes.c_uint64), ("child", ctypes.c_uint64)]


class SubmitItemUpdateResult(ctypes.Structure):
    _pack_ = 4
    _layout_ = "ms"
    _fields_ = [("result", ctypes.c_int32), ("needs_legal_agreement", ctypes.c_bool), ("item", ctypes.c_uint64)]


class StringArray(ctypes.Structure):
    # SteamParamStringArray_t
    _fields_ = [("strings", ctypes.POINTER(ctypes.c_char_p)), ("count", ctypes.c_int32)]


def read_tags(path: Path) -> list[str]:
    """One tag per line; blank lines and # comments are skipped."""
    lines = path.read_text(encoding="utf-8").splitlines()
    return [t for line in lines if (t := line.split("#", 1)[0].strip())]


def _api(lib_path: Path):
    lib = ctypes.CDLL(str(lib_path))
    lib.SteamAPI_InitFlat.argtypes = [ctypes.c_char_p]
    lib.SteamAPI_InitFlat.restype = ctypes.c_int
    lib.SteamAPI_SteamUGC_v020.restype = ctypes.c_void_p
    lib.SteamAPI_SteamUtils_v010.restype = ctypes.c_void_p
    lib.SteamAPI_ISteamUGC_AddDependency.argtypes = [ctypes.c_void_p, ctypes.c_uint64, ctypes.c_uint64]
    lib.SteamAPI_ISteamUGC_AddDependency.restype = ctypes.c_uint64
    lib.SteamAPI_ISteamUGC_StartItemUpdate.argtypes = [ctypes.c_void_p, ctypes.c_uint32, ctypes.c_uint64]
    lib.SteamAPI_ISteamUGC_StartItemUpdate.restype = ctypes.c_uint64
    # The trailing bool (bAllowAdminTags) is ignored by SDKs older than 1.59.
    lib.SteamAPI_ISteamUGC_SetItemTags.argtypes = [
        ctypes.c_void_p, ctypes.c_uint64, ctypes.POINTER(StringArray), ctypes.c_bool]
    lib.SteamAPI_ISteamUGC_SetItemTags.restype = ctypes.c_bool
    for name in ("SetItemUpdateLanguage", "SetItemTitle", "SetItemDescription"):
        fn = getattr(lib, f"SteamAPI_ISteamUGC_{name}")
        fn.argtypes = [ctypes.c_void_p, ctypes.c_uint64, ctypes.c_char_p]
        fn.restype = ctypes.c_bool
    lib.SteamAPI_ISteamUGC_SubmitItemUpdate.argtypes = [ctypes.c_void_p, ctypes.c_uint64, ctypes.c_char_p]
    lib.SteamAPI_ISteamUGC_SubmitItemUpdate.restype = ctypes.c_uint64
    lib.SteamAPI_ISteamUtils_IsAPICallCompleted.argtypes = [ctypes.c_void_p, ctypes.c_uint64, ctypes.POINTER(ctypes.c_bool)]
    lib.SteamAPI_ISteamUtils_IsAPICallCompleted.restype = ctypes.c_bool
    lib.SteamAPI_ISteamUtils_GetAPICallResult.argtypes = [
        ctypes.c_void_p, ctypes.c_uint64, ctypes.c_void_p, ctypes.c_int, ctypes.c_int, ctypes.POINTER(ctypes.c_bool)]
    lib.SteamAPI_ISteamUtils_GetAPICallResult.restype = ctypes.c_bool
    return lib


def _wait(lib, utils, call: int, kind=AddDependencyResult, callback: int = ADD_UGC_DEPENDENCY_RESULT,
          timeout: float = 30.0):
    failed = ctypes.c_bool(False)
    deadline = time.monotonic() + timeout
    while not lib.SteamAPI_ISteamUtils_IsAPICallCompleted(utils, call, ctypes.byref(failed)):
        if time.monotonic() > deadline:
            raise TimeoutError("Steam did not answer in time")
        lib.SteamAPI_RunCallbacks()
        time.sleep(0.1)
    out = kind()
    if not lib.SteamAPI_ISteamUtils_GetAPICallResult(utils, call, ctypes.byref(out), ctypes.sizeof(out),
                                                     callback, ctypes.byref(failed)) or failed.value:
        raise RuntimeError("Steam reported an I/O failure for the call")
    return out


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--lib", type=Path, default=LIB)
    ap.add_argument("--tags", type=Path, help="file with one Workshop tag per line")
    ap.add_argument("--french-description", type=Path, help="BBCode description shown to French Steam users")
    ap.add_argument("parent", type=int)
    ap.add_argument("required", type=int, nargs="*")
    args = ap.parse_args(argv)

    os.environ["SteamAppId"] = os.environ["SteamGameId"] = str(APP_ID)
    lib = _api(args.lib)
    error = ctypes.create_string_buffer(1024)
    if lib.SteamAPI_InitFlat(error) != 0:
        print(f"Steam API init failed (is the Steam client running?): {error.value.decode(errors='replace')}",
              file=sys.stderr)
        return 1
    try:
        ugc, utils = lib.SteamAPI_SteamUGC_v020(), lib.SteamAPI_SteamUtils_v010()
        if not ugc or not utils:
            print("Steam UGC interface unavailable", file=sys.stderr)
            return 1
        if args.check:
            print("Steam API ready; nothing changed (--check).")
            return 0
        status = 0
        for child in args.required:
            res = _wait(lib, utils, lib.SteamAPI_ISteamUGC_AddDependency(ugc, args.parent, child))
            if res.result in (RESULT_OK, RESULT_DUPLICATE_REQUEST):
                print(f"Item {args.parent} now requires item {child}.")
            else:
                print(f"Adding item {child} as required by {args.parent} failed: EResult {res.result}", file=sys.stderr)
                status = 1
        if args.tags or args.french_description:
            tags = read_tags(args.tags) if args.tags else None
            french = args.french_description.read_text(encoding="utf-8") if args.french_description else None
            status |= _update_item(lib, ugc, utils, args.parent, tags, french)
        return status
    finally:
        lib.SteamAPI_Shutdown()


def _update_item(lib, ugc, utils, item: int, tags: list[str] | None, french: str | None) -> int:
    """One item update: the tags and/or the French title and description. Content and English text are untouched."""
    handle = lib.SteamAPI_ISteamUGC_StartItemUpdate(ugc, APP_ID, item)
    if tags is not None:
        encoded = (ctypes.c_char_p * len(tags))(*(t.encode() for t in tags))
        array = StringArray(encoded, len(tags))
        if not lib.SteamAPI_ISteamUGC_SetItemTags(ugc, handle, ctypes.byref(array), False):
            print("SetItemTags refused the tags", file=sys.stderr)
            return 1
    if french is not None:
        if not (lib.SteamAPI_ISteamUGC_SetItemUpdateLanguage(ugc, handle, b"french")
                and lib.SteamAPI_ISteamUGC_SetItemTitle(ugc, handle, TITLE.encode())
                and lib.SteamAPI_ISteamUGC_SetItemDescription(ugc, handle, french.encode())):
            print("Steam refused the French title or description (description limit: 8000 bytes of UTF-8)",
                  file=sys.stderr)
            return 1
    res = _wait(lib, utils, lib.SteamAPI_ISteamUGC_SubmitItemUpdate(ugc, handle, None),
                SubmitItemUpdateResult, SUBMIT_ITEM_UPDATE_RESULT)
    if res.result != RESULT_OK:
        print(f"Updating item {item} failed: EResult {res.result}", file=sys.stderr)
        return 1
    if tags is not None:
        print(f"Item {item} tags: {', '.join(tags)}.")
    if french is not None:
        print(f"Item {item}: French description set.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
