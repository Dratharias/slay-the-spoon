"""Mark Workshop items as required by the Slay the Spoon item (Steam's "Required items").

Usage: python3 -I tools/workshop_dependency.py [--check] <parent item id> <required item id>...

SteamCMD's workshop_build_item cannot set required items, so this calls ISteamUGC::AddDependency through the game's
own libsteam_api.so (ctypes, no extra package). Subscribing to the parent then offers to subscribe to the required
items as well. It needs the Steam client running and logged in as the item's owner; Steam shows Slay the Spire 2 as
running for a moment. --check only initializes the Steam API and exits, without changing anything.
"""
import argparse
import ctypes
import os
import sys
import time
from pathlib import Path

APP_ID = 2868840
LIB = Path.home() / ".local/share/Steam/steamapps/common/Slay the Spire 2/data_sts2_linuxbsd_x86_64/libsteam_api.so"
ADD_UGC_DEPENDENCY_RESULT = 3412  # k_iSteamUGCCallbacks + 12
RESULT_OK = 1
RESULT_DUPLICATE_REQUEST = 29


class AddDependencyResult(ctypes.Structure):
    # Steam callback structs use 4-byte packing on Linux.
    _pack_ = 4
    _layout_ = "ms"
    _fields_ = [("result", ctypes.c_int32), ("parent", ctypes.c_uint64), ("child", ctypes.c_uint64)]


def _api(lib_path: Path):
    lib = ctypes.CDLL(str(lib_path))
    lib.SteamAPI_InitFlat.argtypes = [ctypes.c_char_p]
    lib.SteamAPI_InitFlat.restype = ctypes.c_int
    lib.SteamAPI_SteamUGC_v020.restype = ctypes.c_void_p
    lib.SteamAPI_SteamUtils_v010.restype = ctypes.c_void_p
    lib.SteamAPI_ISteamUGC_AddDependency.argtypes = [ctypes.c_void_p, ctypes.c_uint64, ctypes.c_uint64]
    lib.SteamAPI_ISteamUGC_AddDependency.restype = ctypes.c_uint64
    lib.SteamAPI_ISteamUtils_IsAPICallCompleted.argtypes = [ctypes.c_void_p, ctypes.c_uint64, ctypes.POINTER(ctypes.c_bool)]
    lib.SteamAPI_ISteamUtils_IsAPICallCompleted.restype = ctypes.c_bool
    lib.SteamAPI_ISteamUtils_GetAPICallResult.argtypes = [
        ctypes.c_void_p, ctypes.c_uint64, ctypes.c_void_p, ctypes.c_int, ctypes.c_int, ctypes.POINTER(ctypes.c_bool)]
    lib.SteamAPI_ISteamUtils_GetAPICallResult.restype = ctypes.c_bool
    return lib


def _wait(lib, utils, call: int, timeout: float = 30.0) -> AddDependencyResult:
    failed = ctypes.c_bool(False)
    deadline = time.monotonic() + timeout
    while not lib.SteamAPI_ISteamUtils_IsAPICallCompleted(utils, call, ctypes.byref(failed)):
        if time.monotonic() > deadline:
            raise TimeoutError("Steam did not answer in time")
        lib.SteamAPI_RunCallbacks()
        time.sleep(0.1)
    out = AddDependencyResult()
    if not lib.SteamAPI_ISteamUtils_GetAPICallResult(utils, call, ctypes.byref(out), ctypes.sizeof(out),
                                                     ADD_UGC_DEPENDENCY_RESULT, ctypes.byref(failed)) or failed.value:
        raise RuntimeError("Steam reported an I/O failure for the call")
    return out


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--lib", type=Path, default=LIB)
    ap.add_argument("parent", type=int)
    ap.add_argument("required", type=int, nargs="+")
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
        return status
    finally:
        lib.SteamAPI_Shutdown()


if __name__ == "__main__":
    sys.exit(main())
