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

.PHONY: build test up down restart clean

build:
	$(DOTNET) dotnet build src -c Release -p:Sts2DataDir=/sts2 -o /repo/build/dll --nologo -v quiet
	tools/build-pck.sh "$(STAGE)/$(MOD_ID).pck"
	cp mod/$(MOD_ID).json "$(STAGE)/"
	cp build/dll/$(MOD_ID).dll "$(STAGE)/"

test:
	$(DOTNET) dotnet test tests --nologo

# Build and install into the game's mods folder.
up: build
	mkdir -p "$(STS2_DIR)/mods/$(MOD_ID)"
	cp "$(STAGE)"/* "$(STS2_DIR)/mods/$(MOD_ID)/"

# Remove the mod from the game.
down:
	rm -rf "$(STS2_DIR)/mods/$(MOD_ID)"

restart: down up

clean:
	rm -rf build src/bin src/obj tests/bin tests/obj
