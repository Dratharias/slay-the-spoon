#!/usr/bin/env bash
# Stage the built mod for the Steam Workshop and upload it with SteamCMD (Podman).
#   tools/workshop-upload.sh --stage   build/workshop/ only (content, preview, item.vdf), no upload
#   tools/workshop-upload.sh           stage, then upload; asks for the Steam password and Steam Guard
# Run `make build` first. The first upload creates the item and stores its id in
# workshop/published-file-id (commit it); later uploads update that item.
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
APP_ID=2868840
VISIBILITY=1 # 0 public, 1 friends only, 2 private, 3 unlisted
IMAGE=docker.io/steamcmd/steamcmd:latest
STAGE="$REPO/build/mods/SlaySpoon"
OUT="$REPO/build/workshop"
ID_FILE="$REPO/workshop/published-file-id"
# Workshop items subscribers need too (Steam "Required items"), one id per line: BaseLib (3737335127).
REQUIRED_FILE="$REPO/workshop/required-items"
CARDS="$REPO/godot/SlaySpoon/images/cards"

for f in SlaySpoon.json SlaySpoon.dll SlaySpoon.pck; do
	[[ -f "$STAGE/$f" ]] || { echo "Missing $STAGE/$f: run make build first." >&2; exit 1; }
done
VERSION="$(sed -n 's/.*"version": *"\([^"]*\)".*/\1/p' "$STAGE/SlaySpoon.json")"
PUBLISHED_ID="$(cat "$ID_FILE" 2>/dev/null || echo 0)"

rm -rf "$OUT"
mkdir -p "$OUT/content/SlaySpoon"
cp "$STAGE"/SlaySpoon.{json,dll,pck} "$OUT/content/SlaySpoon/"

# 512x512 preview: title band over four StS1 portraits (one per card pool).
BG='#1b1e2b'
magick "$CARDS/ironclad/sts1_limit_break.png" "$CARDS/silent/sts1_wraith_form.png" +append "$OUT/row1.png"
magick "$CARDS/defect/sts1_electrodynamics.png" "$CARDS/colorless/sts1_apotheosis.png" +append "$OUT/row2.png"
magick -size 500x120 xc:"$BG" -gravity north \
	-font Liberation-Sans-Bold -pointsize 52 -fill '#f2d27a' -annotate +0+12 'Slay the Spoon' \
	-font Liberation-Sans -pointsize 24 -fill '#d8d8d8' -annotate +0+76 "StS1 cards - alpha $VERSION" "$OUT/title.png"
magick "$OUT/title.png" "$OUT/row1.png" "$OUT/row2.png" -append \
	-background "$BG" -gravity center -extent 512x512 +repage "$OUT/preview.png"
rm "$OUT/row1.png" "$OUT/row2.png" "$OUT/title.png"

# VDF strings: escape backslashes and double quotes.
vdf_escape() { sed -e 's/\\/\\\\/g' -e 's/"/\\"/g'; }
DESCRIPTION="$(vdf_escape < "$REPO/workshop/description.txt")"
CHANGENOTE="Version $VERSION"
if [[ -f "$REPO/workshop/changelog-$VERSION.txt" ]]; then
	CHANGENOTE="$(vdf_escape < "$REPO/workshop/changelog-$VERSION.txt")"
fi
cat > "$OUT/item.vdf" <<EOF
"workshopitem"
{
	"appid" "$APP_ID"
	"publishedfileid" "$PUBLISHED_ID"
	"contentfolder" "/workshop/content"
	"previewfile" "/workshop/preview.png"
	"visibility" "$VISIBILITY"
	"title" "Slay the Spoon (alpha)"
	"description" "$DESCRIPTION"
	"changenote" "$CHANGENOTE"
}
EOF
if [[ "$PUBLISHED_ID" == 0 ]]; then ITEM="new item"; else ITEM="item $PUBLISHED_ID"; fi
echo "Staged $OUT (version $VERSION, $ITEM)."
[[ "${1:-}" == "--stage" ]] && exit 0

read -r -p "Steam account name: " STEAM_USER
# SteamCMD prompts for the password and Steam Guard code; its login cache stays in the volume.
podman run --rm -it --security-opt label=disable \
	-v slayspoon-steamcmd:/root \
	-v "$OUT":/workshop \
	"$IMAGE" +login "$STEAM_USER" +workshop_build_item /workshop/item.vdf +quit

# SteamCMD writes the new item's id back into item.vdf.
NEW_ID="$(sed -n 's/.*"publishedfileid"[[:space:]]*"\([0-9]*\)".*/\1/p' "$OUT/item.vdf")"
if [[ -n "$NEW_ID" && "$NEW_ID" != 0 ]]; then
	echo "$NEW_ID" > "$ID_FILE"
	echo "Workshop item: https://steamcommunity.com/sharedfiles/filedetails/?id=$NEW_ID"
	# SteamCMD cannot set required items: map them through the Steam client (must be running, same account).
	mapfile -t REQUIRED < <(grep -o '^[0-9]\+' "$REQUIRED_FILE")
	if (( ${#REQUIRED[@]} )); then
		python3 -I "$REPO/tools/workshop_dependency.py" "$NEW_ID" "${REQUIRED[@]}" \
			|| echo "Required items not set: add them on the item page (Add/Remove Required Items)." >&2
	fi
fi
