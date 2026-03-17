#!/usr/bin/env bash
set -e

PLUGIN_NAME="ArtisanJelly"
DLL_NAME="Jellyfin.Plugin.ArtisanJelly.dll"
BUILD_OUTPUT="bin/publish"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GUID="800aa8b6-9226-4069-a99a-4cdfafcdf394"
VERSION="1.0.0.0"
TARGET_ABI="10.11.6.0"

# ── Typical Jellyfin plugin locations ───────────────────────────────
CANDIDATE_DIRS=(
  "/var/lib/jellyfin/plugins"
  "/usr/lib/jellyfin/plugins"
  "/opt/jellyfin/plugins"
  "$HOME/.local/share/jellyfin/plugins"
  "$HOME/.jellyfin/plugins"
  "/config/plugins"
  "/data/plugins"
  "/jellyfin/plugins"
  "/mnt/user/appdata/jellyfin/plugins"
  "/volume1/@appstore/jellyfin/var/plugins"
)

# ── Generate meta.json ───────────────────────────────────────────────
generate_meta() {
  local timestamp
  timestamp="$(date -u +"%Y-%m-%dT%H:%M:%S.0000000Z")"

  cat >"$SCRIPT_DIR/meta.json" <<EOF
{
  "category": "Metadata",
  "changelog": "Initial release.",
  "description": "Scans your library for missing artwork",
  "guid": "$GUID",
  "name": "Artisan Jelly",
  "overview": "Finds movies and shows with incomplete images — missing posters, logos, backdrops, discs, and more.",
  "owner": "hummird",
  "targetAbi": "$TARGET_ABI",
  "timestamp": "$timestamp",
  "version": "$VERSION",
  "status": "Active",
  "autoUpdate": false,
  "assemblies": [
    "$DLL_NAME"
  ]
}
EOF
  echo "==> Generated meta.json (timestamp: $timestamp)"
}

# ── Build ────────────────────────────────────────────────────────────
build() {
  echo "==> Cleaning build artifacts..."
  rm -rf bin/ obj/

  echo "==> Building project..."
  dotnet publish -c Release -o "$BUILD_OUTPUT"

  generate_meta

  echo "==> Build complete: $BUILD_OUTPUT/$DLL_NAME"
}

# ── Find installed Jellyfin plugin roots ─────────────────────────────
find_jellyfin_dirs() {
  local found=()
  for dir in "${CANDIDATE_DIRS[@]}"; do
    if [ -d "$dir" ]; then
      found+=("$dir")
    fi
  done
  echo "${found[@]}"
}

# ── Install ──────────────────────────────────────────────────────────
install_plugin() {
  local dll="$BUILD_OUTPUT/$DLL_NAME"

  if [ ! -f "$dll" ]; then
    echo "ERROR: $dll not found. Run ./build.sh first." >&2
    exit 1
  fi

  local found_dirs
  read -ra found_dirs <<<"$(find_jellyfin_dirs)"

  if [ ${#found_dirs[@]} -eq 0 ]; then
    echo ""
    echo "!!! No Jellyfin plugin directories found automatically."
    echo "    Locate your Jellyfin plugins folder and run:"
    echo ""
    echo "    mkdir -p /YOUR/JELLYFIN/plugins/$PLUGIN_NAME"
    echo "    cp $dll /YOUR/JELLYFIN/plugins/$PLUGIN_NAME/"
    echo "    cp $SCRIPT_DIR/meta.json /YOUR/JELLYFIN/plugins/$PLUGIN_NAME/"
    echo "    chown -R jellyfin:jellyfin /YOUR/JELLYFIN/plugins/$PLUGIN_NAME"
    echo ""
    exit 0
  fi

  local target_dir
  if [ ${#found_dirs[@]} -eq 1 ]; then
    target_dir="${found_dirs[0]}"
  else
    echo "Multiple Jellyfin plugin directories found:"
    for i in "${!found_dirs[@]}"; do
      echo "  [$((i + 1))] ${found_dirs[$i]}"
    done
    printf "Pick one [1]: "
    read -r choice
    choice="${choice:-1}"
    target_dir="${found_dirs[$((choice - 1))]}"
  fi

  local plugin_dir="$target_dir/$PLUGIN_NAME"

  if [ ! -d "$plugin_dir" ]; then
    echo "==> First run: creating $plugin_dir"
    sudo mkdir -p "$plugin_dir"
  fi

  echo "==> Copying DLL and meta.json to $plugin_dir..."
  sudo cp "$dll" "$plugin_dir/"
  sudo cp "$SCRIPT_DIR/meta.json" "$plugin_dir/"

  echo "==> Setting permissions (chown jellyfin:jellyfin)..."
  sudo chown -R jellyfin:jellyfin "$plugin_dir" 2>/dev/null || true

  if systemctl list-units --type=service 2>/dev/null | grep -q "jellyfin"; then
    echo "==> Restarting Jellyfin service..."
    sudo systemctl restart jellyfin
    echo "==> Done!"
  else
    echo "==> Jellyfin systemd service not found — restart it manually."
  fi
}

# ── Entrypoint ───────────────────────────────────────────────────────
case "${1:-}" in
--install)
  build
  install_plugin
  ;;
"")
  build
  ;;
*)
  echo "Usage: ./build.sh [--install]"
  echo "  (no args)   Build only"
  echo "  --install   Build and install to detected Jellyfin location"
  exit 1
  ;;
esac
