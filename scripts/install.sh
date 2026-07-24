#!/usr/bin/env bash
# Install PodcastSync from GitHub.
# Usage (human or AI agent):
#   curl -fsSL https://raw.githubusercontent.com/Codist-Cafe/pod-mini/master/scripts/install.sh | bash
#
# Or specify a version:
#   curl -fsSL ... | bash -s -- --version 1.0.0
#
# Or install from local checkout:
#   ./scripts/install.sh --from-source

set -euo pipefail

REPO="Codist-Cafe/pod-mini"
BIN="podcastsync"
INSTALL_DIR="${INSTALL_DIR:-$HOME/.local/bin}"
VERSION="${VERSION:-latest}"
FROM_SOURCE=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --version) VERSION="$2"; shift 2 ;;
        --from-source) FROM_SOURCE=true; shift ;;
        --install-dir) INSTALL_DIR="$2"; shift 2 ;;
        *) echo "Unknown flag: $1"; exit 1 ;;
    esac
done

# ── Detect platform ──────────────────────────────────────────────────────────

OS=$(uname -s)
ARCH=$(uname -m)

case "$OS" in
    Linux)
        case "$ARCH" in
            x86_64)  RID="linux-x64" ;;
            aarch64|arm64) RID="linux-arm64" ;;
            *) echo "Unknown Linux arch: $ARCH"; exit 1 ;;
        esac ;;
    Darwin)
        case "$ARCH" in
            x86_64)  RID="osx-x64" ;;
            arm64)   RID="osx-arm64" ;;
            *) echo "Unknown macOS arch: $ARCH"; exit 1 ;;
        esac ;;
    MINGW*|MSYS*|CYGWIN*)
        case "$ARCH" in
            x86_64)  RID="win-x64" ;;
            aarch64|arm64) RID="win-arm64" ;;
            *) echo "Unknown Windows arch: $ARCH"; exit 1 ;;
        esac ;;
    *)
        echo "Unsupported OS: $OS. Installing from source..."
        FROM_SOURCE=true
        ;;
esac
    *)
        echo "Unsupported OS: $OS. Installing from source..."
        FROM_SOURCE=true
        ;;
echo "Platform: ${RID:-source build}"

# ── Source build path ────────────────────────────────────────────────────────

if $FROM_SOURCE; then
    echo "Building from source..."
    TMP=$(mktemp -d)
    trap 'rm -rf "$TMP"' EXIT
    git clone --depth 1 "https://github.com/${REPO}.git" "$TMP"
    cd "$TMP"
    dotnet publish src/PodcastSync.Cli -c Release \
        -r "${RID:-linux-x64}" --self-contained true \
        -p:PublishSingleFile=true -p:DebugType=embedded \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -o publish
    SRC_BIN="publish/PodcastSync.Cli"
    if [[ "$RID" == win-* ]]; then
        SRC_BIN="${SRC_BIN}.exe"
    fi
    cd - >/dev/null
else
    # ── Download release binary ───────────────────────────────────────────────

    if [[ "$VERSION" == "latest" ]]; then
        echo "Resolving latest release..."
        VERSION=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" \
            | grep -oP '"tag_name":\s*"\K[^"]+' || echo "")
        if [[ -z "$VERSION" ]]; then
            echo "Could not resolve latest version. Falling back to source build..."
            exec bash "$0" --from-source --install-dir "$INSTALL_DIR"
        fi
    fi

    # Normalize: tag is always vX.Y.Z; we store it with the v prefix
    if [[ "$VERSION" != v* ]]; then
        VERSION="v${VERSION}"
    fi

    # Archive filenames omit the v prefix (podcastsync-v1.0.0-linux-x64.tar.gz)
    FILE_VER="${VERSION#v}"

    if [[ "$RID" == win-* ]]; then
        ARCHIVE="podcastsync-v${FILE_VER}-${RID}.zip"
    else
        ARCHIVE="podcastsync-v${FILE_VER}-${RID}.tar.gz"
    fi
    URL="https://github.com/${REPO}/releases/download/${VERSION}/${ARCHIVE}"

    echo "Downloading ${URL}..."
    TMP=$(mktemp -d)
    trap 'rm -rf "$TMP"' EXIT
    if ! curl -fsSL "$URL" -o "$TMP/$ARCHIVE"; then
        echo "Release binary not found for ${RID}. Falling back to source build..."
        FROM_SOURCE=true
        exec bash "$0" --from-source --install-dir "$INSTALL_DIR"
    fi

    cd "$TMP"
    if [[ "$RID" == win-* ]]; then
        unzip -q "$ARCHIVE"
    else
        tar xzf "$ARCHIVE"
    fi
    SRC_BIN="${RID}/podcastsync"
    if [[ "$RID" == win-* ]]; then
        SRC_BIN="${RID}/podcastsync.exe"
    fi
    cd - >/dev/null
fi

# ── Install ──────────────────────────────────────────────────────────────────

mkdir -p "$INSTALL_DIR"
cp "$TMP/$SRC_BIN" "$INSTALL_DIR/$BIN"
chmod +x "$INSTALL_DIR/$BIN"

echo ""
echo "PodcastSync ${VERSION:-dev} installed to $INSTALL_DIR/$BIN"

if ! echo "$PATH" | grep -q "$INSTALL_DIR"; then
    echo "Add $INSTALL_DIR to your PATH:"
    echo "  export PATH=\"$INSTALL_DIR:\$PATH\""
fi

echo ""
echo "Try it: $BIN info"
rm -rf "$TMP"
