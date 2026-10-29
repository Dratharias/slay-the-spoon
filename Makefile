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
TOOLS     := nice -n 19 podman run --rm \
	--security-opt label=disable \
	-v "$(CURDIR)":/repo \
	-v "$(STS1_DECOMP)":/sts1:ro \
	-v "$(STS1_JAR)":/sts1.jar:ro \
	-v "$(STS2_DECOMP)":/sts2src:ro \
	-w /repo localhost/slayspoon-tools

.PHONY: build test up down restart clean mp tools-image sts1-data

build:
	$(DOTNET) dotnet build src -c Release -p:Sts2DataDir=/sts2 -o /repo/build/dll --nologo -v quiet
	tools/build-pck.sh "$(STAGE)/$(MOD_ID).pck"
	cp mod/$(MOD_ID).json "$(STAGE)/"
	cp build/dll/$(MOD_ID).dll "$(STAGE)/"

test:
	$(DOTNET) dotnet test tests --nologo
	$(TOOLS) python -m pytest -q -p no:cacheprovider tools/sts1/tests

tools-image:
	podman build -q -t localhost/slayspoon-tools -f tools/sts1/Containerfile tools/sts1

# Regenerate card data, the duplicate table, portraits and card skeletons from StS1.
sts1-data:
	$(TOOLS) python tools/sts1/extract.py --decomp /sts1 --color red --out data/sts1/ironclad.json

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

clean:
	rm -rf build src/bin src/obj tests/bin tests/obj
