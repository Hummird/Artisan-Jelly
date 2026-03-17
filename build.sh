#!/usr/bin/env bash
set -e

PLUGIN_NAME="Artisan Jelly"
DLL_NAME="Jellyfin.Plugin.ArtisanJelly.dll"
BUILD_OUTPUT="bin/publish"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GUID="800aa8b6-9226-4069-a99a-4cdfafcdf394"
VERSION="1.1.0.3"
TARGET_ABI="10.11.6.0"

# ── Generate meta.json ───────────────────────────────────────────────
generate_meta() {
  local timestamp
  timestamp="$(date -u +"%Y-%m-%dT%H:%M:%S.0000000Z")"

  cat >"$SCRIPT_DIR/meta.json" <<EOF
{
  "category": "General",
  "changelog": "Fixed transparent background on modal",
  "description": "Scans your library for missing artwork and lets you fix it from one place.",
  "guid": "$GUID",
  "name": "$PLUGIN_NAME",
  "overview": "Finds movies and shows with incomplete images — missing posters, logos, backdrops, discs, and more.",
  "owner": "hummird",
  "targetAbi": "$TARGET_ABI",
  "timestamp": "$timestamp",
  "version": "$VERSION",
  "imagePath": "/var/lib/jellyfin/plugins/${PLUGIN_NAME}_$VERSION/${PLUGIN_NAME}.png",
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

# ── Install ──────────────────────────────────────────────────────────
install_plugin() {
  local dll="$BUILD_OUTPUT/$DLL_NAME"

  local plugin_dir="/var/lib/jellyfin/plugins/${PLUGIN_NAME}_$VERSION"

  if [ ! -d "$plugin_dir" ]; then
    echo "==> First run: creating $plugin_dir"
    sudo mkdir -p "$plugin_dir"
  fi

  echo "==> Copying DLL and meta.json to $plugin_dir..."
  sudo cp "$dll" "$plugin_dir/"
  sudo cp "$SCRIPT_DIR/meta.json" "$plugin_dir/"

  if [ -f "$SCRIPT_DIR/$PLUGIN_NAME.png" ]; then
    echo "==> Copying $PLUGIN_NAME.png..."
    sudo cp "$SCRIPT_DIR/$PLUGIN_NAME.png" "$plugin_dir/"
    sudo chmod 644 "$plugin_dir/$PLUGIN_NAME.png"
  fi

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
