#!/usr/bin/env bash
# Packs godot/SlaySpoon/** into a Godot 4.5.1 PCK.
# Usage: tools/build-pck.sh <output.pck>
set -euo pipefail

GODOT_VERSION=4.5.1
CACHE="${XDG_CACHE_HOME:-$HOME/.cache}/slay-spoon/godot-$GODOT_VERSION"
GODOT="$CACHE/Godot_v${GODOT_VERSION}-stable_linux.x86_64"
REPO="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$(realpath -m "$1")"

if [[ ! -x "$GODOT" ]]; then
	mkdir -p "$CACHE"
	curl -fsSL -o "$CACHE/godot.zip" \
		"https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}-stable/Godot_v${GODOT_VERSION}-stable_linux.x86_64.zip"
	unzip -q -o "$CACHE/godot.zip" -d "$CACHE"
	rm "$CACHE/godot.zip"
	chmod +x "$GODOT"
fi

mkdir -p "$(dirname "$OUT")"
"$GODOT" --headless --path "$REPO/godot" --export-pack "SlaySpoon" "$OUT"
test -s "$OUT"
