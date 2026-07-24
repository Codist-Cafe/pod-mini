#!/usr/bin/env bash
# Publish PodcastSync CLI as single-file platform-specific executables.
#
# Run from the repo root:
#   ./scripts/publish.sh                  # all 6 platforms
#   ./scripts/publish.sh linux-x64        # single platform
#   ./scripts/publish.sh linux-x64 win-x64 # subset
#
# Cross-compiles from Linux x64 for all .NET 10 runtime identifiers.
# Archives produced:
#   Linux x64:   podcastsync-v1.0.0-linux-x64.tar.gz
#   Linux ARM64: podcastsync-v1.0.0-linux-arm64.tar.gz
#   Windows x64: podcastsync-v1.0.0-win-x64.zip
#   Windows ARM64: podcastsync-v1.0.0-win-arm64.zip
#   macOS x64:   podcastsync-v1.0.0-osx-x64.tar.gz
#   macOS ARM64: podcastsync-v1.0.0-osx-arm64.tar.gz

set -euo pipefail
cd "$(dirname "$0")/.."

VERSION=$(cat VERSION)
ALL_RIDS="linux-x64 linux-arm64 win-x64 win-arm64 osx-x64 osx-arm64"
RIDS="${*:-$ALL_RIDS}"
CLI_PROJ="src/PodcastSync.Cli/PodcastSync.Cli.csproj"
OUT="publish"

echo "=== PodcastSync v${VERSION} release publish ==="
echo "Targets: ${RIDS}"

# Pre-restore for the first RID so native pack resolution is cached
FIRST=$(echo "$RIDS" | awk '{print $1}')
dotnet restore "$CLI_PROJ" -r "$FIRST" >/dev/null 2>&1

for rid in $RIDS; do
    echo ""
    echo "--- ${rid} ---"

    if [[ "${rid}" == osx-* && "$(uname -s)" != "Darwin" ]]; then
        echo "  (cross-compiling from Linux — .tar.gz only; .dmg needs a macOS runner)"
    fi

    dotnet publish "$CLI_PROJ" \
        -c Release \
        -r "$rid" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:DebugType=embedded \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:Version="${VERSION}" \
        -o "${OUT}/${rid}" 2>&1 | tail -2

    if [[ "$rid" == win-* ]]; then
        mv "${OUT}/${rid}/PodcastSync.Cli" "${OUT}/${rid}/podcastsync.exe" 2>/dev/null || true
        ARCHIVE="podcastsync-v${VERSION}-${rid}.zip"
        (cd publish && zip -qr "${ARCHIVE}.zip" "${rid}" && mv "${ARCHIVE}.zip" "${ARCHIVE}")
        echo "  -> publish/${ARCHIVE}"
    else
        mv "${OUT}/${rid}/PodcastSync.Cli" "${OUT}/${rid}/podcastsync" 2>/dev/null || true
        ARCHIVE="podcastsync-v${VERSION}-${rid}.tar.gz"
        tar -czf "publish/${ARCHIVE}" -C publish "${rid}"
        echo "  -> publish/${ARCHIVE}"
    fi
done

echo ""
echo "=== Done ==="
ls -lh publish/podcastsync-* 2>/dev/null || true
