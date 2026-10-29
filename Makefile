# Slay the Spoon — build and install the mod.
# .NET runs inside Podman (the dev machine is an immutable Bazzite system).

STS2_DIR ?= $(HOME)/.local/share/Steam/steamapps/common/Slay the Spire 2
STS2_DATA := $(STS2_DIR)/data_sts2_linuxbsd_x86_64
MOD_ID    := SlaySpoon
STAGE     := build/mods/$(MOD_ID)
DOTNET    := podman run --rm \
	--security-opt label=disable \
	-v "$(CURDIR)":/repo \
	-v "$(STS2_DATA)":/sts2:ro \
	-v slayspoon-nuget:/root/.nuget \
	-w /repo mcr.microsoft.com/dotnet/sdk:9.0

# StS1 card pipeline (tools/sts1, not shipped). Reads the local decompile, the StS1 jar and StS2's install.
STS1_DECOMP ?= $(HOME)/Projects/sts-decomp/sts1
STS1_JAR    ?= $(HOME)/.local/share/Steam/steamapps/common/SlayTheSpire/desktop-1.0.jar
STS2_DECOMP ?= $(HOME)/Projects/sts-decomp/sts2
STS2_LOC    ?= $(HOME)/Projects/sts-decomp/sts2-loc
TOOLS     := nice -n 19 podman run --rm \
	--security-opt label=disable \
	-v "$(CURDIR)":/repo \
	-v "$(STS1_DECOMP)":/sts1:ro \
	-v "$(STS1_JAR)":/sts1.jar:ro \
	-v "$(STS2_DECOMP)":/sts2src:ro \
	-v "$(STS2_LOC)":/sts2loc:ro \
	-w /repo localhost/slayspoon-tools

.PHONY: build test up down restart clean mp tools-image sts1-data sts1-colorless-data sts2-loc workshop workshop-upload

build:
	$(DOTNET) dotnet build src -c Release -p:Sts2DataDir=/sts2 -o /repo/build/dll --nologo -v quiet
	tools/build-pck.sh "$(STAGE)/$(MOD_ID).pck"
	cp mod/$(MOD_ID).json "$(STAGE)/"
	cp build/dll/$(MOD_ID).dll "$(STAGE)/"

test:
	$(DOTNET) dotnet test tests --nologo
	$(TOOLS) python -m pytest -q -p no:cacheprovider tools/sts1/tests tools/tests

tools-image:
	podman build -q -t localhost/slayspoon-tools -f tools/sts1/Containerfile tools/sts1

# Copy StS2's eng/fra card text out of the game's PCK (needed by sts1-data).
sts2-loc:
	tools/build-pck.sh --godot-only
	cd tools/sts1 && PCK="$(STS2_DIR)/SlayTheSpire2.pck" OUT="$(STS2_LOC)" nice -n 19 \
		"$${XDG_CACHE_HOME:-$$HOME/.cache}/slay-spoon/godot-4.5.1/Godot_v4.5.1-stable_linux.x86_64" --headless -s dump_sts2_loc.gd

# Regenerate card data, the duplicate table, portraits, card skeletons and event art from StS1.
sts1-data:
	for pair in red:Ironclad green:Silent blue:Defect colorless:Colorless; do \
		color=$${pair%%:*}; character=$${pair##*:}; lower=$$(echo $$character | tr A-Z a-z); \
		$(TOOLS) python tools/sts1/extract.py --decomp /sts1 --color $$color --out data/sts1/$$lower.json && \
		$(TOOLS) python tools/sts1/match.py --sts1 data/sts1/$$lower.json \
			--sts2-cards /sts2src/MegaCrit.Sts2.Core.Models.Cards --sts2-loc /sts2loc \
			--table data/duplicates/$$lower.json --review docs/review/$$lower-duplicates.md \
			--matches src/CardSets/Sts1Matches.$$character.g.cs --character $$character --color $$color && \
		$(TOOLS) python tools/sts1/art.py --jar /sts1.jar --sts1 data/sts1/$$lower.json \
			--table data/duplicates/$$lower.json --out godot/SlaySpoon/images/cards/$$lower && \
		$(TOOLS) python tools/sts1/codegen.py --sts1 data/sts1/$$lower.json --table data/duplicates/$$lower.json \
			--character $$character --src src/Cards/Sts1/$$character --loc godot/SlaySpoon/localization \
			--relations unique,same-sts1,duplicate || exit 1; \
	done
	$(TOOLS) python tools/sts1/art.py --jar /sts1.jar --events godot/SlaySpoon/images/events

# Colorless catalog first: no playable skeletons until namesakes and special cards are reviewed.
sts1-colorless-data:
	$(TOOLS) python tools/sts1/extract.py --decomp /sts1 --color colorless --out data/sts1/colorless.json
	$(TOOLS) python tools/sts1/match.py --sts1 data/sts1/colorless.json \
		--sts2-cards /sts2src/MegaCrit.Sts2.Core.Models.Cards --sts2-loc /sts2loc \
		--table data/duplicates/colorless.json --review docs/review/colorless-duplicates.md \
		--matches src/CardSets/Sts1Matches.Colorless.g.cs --character Colorless --color colorless

# Build and install into the game's mods folder.
up: build
	mkdir -p "$(STS2_DIR)/mods/$(MOD_ID)"
	cp "$(STAGE)"/* "$(STS2_DIR)/mods/$(MOD_ID)/"

# Remove the mod from the game.
down:
	rm -rf "$(STS2_DIR)/mods/$(MOD_ID)"

restart: down up

# Local co-op test on this machine (Mega Crit's dev flag --fastmp, ENet on 127.0.0.1:33771):
# a host window in the Custom Run lobby and a client window that joins it.
GAME_ARGS := --windowed --resolution 1280x720 -wpos
mp: up
	cd "$(STS2_DIR)" && SteamAppId=2868840 ./SlayTheSpire2 --fastmp host_custom $(GAME_ARGS) --position 0,40 >/dev/null 2>&1 </dev/null &
	sleep 12
	cd "$(STS2_DIR)" && SteamAppId=2868840 ./SlayTheSpire2 --fastmp join --clientId 1001 $(GAME_ARGS) --position 1290,40 >/dev/null 2>&1 </dev/null &

# Steam Workshop (friends-only alpha): stage build/workshop/ only, or stage and upload with SteamCMD.
# The upload asks for the Steam account, password and Steam Guard code: run it in a terminal.
workshop: build
	tools/workshop-upload.sh --stage

workshop-upload: build
	tools/workshop-upload.sh

clean:
	rm -rf build src/bin src/obj tests/bin tests/obj
