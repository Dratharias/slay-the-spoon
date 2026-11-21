#!/usr/bin/env bash
# Stage the built mod for the Steam Workshop and upload it with SteamCMD (Podman).
#   tools/workshop-upload.sh --stage   build/workshop/ only (content, preview, item.vdf), no upload
#   tools/workshop-upload.sh           stage, then upload; asks for the Steam password and Steam Guard
# Run `make build` first. The first upload creates the item and stores its id in
# workshop/published-file-id (commit it); later uploads update that item.
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
APP_ID=2868840
VISIBILITY=0 # 0 public, 1 friends only, 2 private, 3 unlisted
IMAGE=docker.io/steamcmd/steamcmd:latest
STAGE="$REPO/build/mods/SlaySpoon"
OUT="$REPO/build/workshop"
ID_FILE="$REPO/workshop/published-file-id"
# Workshop items subscribers need too (Steam "Required items"), one id per line: BaseLib (3737335127).
REQUIRED_FILE="$REPO/workshop/required-items"
# Workshop tags (categories, English and French), one per line; set after the upload with the required items.
TAGS_FILE="$REPO/workshop/tags"
# French description (Steam shows it to players whose Steam is in French); description.txt is the English one.
FRENCH_FILE="$REPO/workshop/description-fr.txt"

for f in SlaySpoon.json SlaySpoon.dll SlaySpoon.pck; do
	[[ -f "$STAGE/$f" ]] || { echo "Missing $STAGE/$f: run make build first." >&2; exit 1; }
done
VERSION="$(sed -n 's/.*"version": *"\([^"]*\)".*/\1/p' "$STAGE/SlaySpoon.json")"
PUBLISHED_ID="$(cat "$ID_FILE" 2>/dev/null || echo 0)"

rm -rf "$OUT"
mkdir -p "$OUT/content/SlaySpoon"
cp "$STAGE"/SlaySpoon.{json,dll,pck} "$OUT/content/SlaySpoon/"

# 512x512 preview (owner, 2026-11-19): the Spoonful card in front of two others, drawn by tools/workshop_banners.py.
cp "$REPO/workshop/images/preview.png" "$OUT/preview.png"

# VDF strings: SteamCMD's KeyValues parser reads no escape sequences, so a straight double quote ends the string
# (an upload failed on '"Broken Card"': "key name too long"). Straight quotes become curly ones instead.
vdf_escape() { sed -E -e 's/(^|[[:space:](])"/\1\xe2\x80\x9c/g' -e 's/"/\xe2\x80\x9d/g'; }
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
	"title" "Slay the Spoon"
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
	# SteamCMD cannot set required items, tags or a second language: set them through the Steam client (must be running, same account).
	mapfile -t REQUIRED < <(grep -o '^[0-9]\+' "$REQUIRED_FILE")
	python3 -I "$REPO/tools/workshop_dependency.py" --tags "$TAGS_FILE" --french-description "$FRENCH_FILE" "$NEW_ID" "${REQUIRED[@]}" \
		|| echo "Required items, tags or French text not set: rerun tools/workshop_dependency.py, or use the item page." >&2
fi
