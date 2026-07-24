#!/usr/bin/env bash
# Publish PodcastSync CLI as single-file platform-specific executables.
# Run from the repo root.
#   ./scripts/publish.sh              # publish all three platforms
#   ./scripts/publish.sh linux-x64    # publish one platform

set -euo pipefail
cd "$(dirname "$0")/.."

VERSION=$(cat VERSION)
RIDS="${*:-linux-x64 win-x64 osx-x64}"
CLI_PROJ="src/PodcastSync.Cli/PodcastSync.Cli.csproj"
OUT="publish"

echo "=== PodcastSync v${VERSION} release publish ==="

# Restore with all RIDs so cross-publish works from any host
RID_CSV=$(echo "$RIDS" | tr ' ' ';')
dotnet restore "$CLI_PROJ" -r "$(echo "$RIDS" | awk '{print $1}')" >/dev/null 2>&1

for rid in $RIDS; do
    echo ""
    echo "--- Publishing ${rid} ---"
    dotnet publish "$CLI_PROJ" \
        -c Release \
        -r "$rid" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:DebugType=embedded \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:Version="${VERSION}" \
        -o "${OUT}/${rid}"

    if [[ "$rid" == win-* ]]; then
        mv "${OUT}/${rid}/PodcastSync.Cli" "${OUT}/${rid}/podcastsync.exe" 2>/dev/null || true
    else
        mv "${OUT}/${rid}/PodcastSync.Cli" "${OUT}/${rid}/podcastsync" 2>/dev/null || true
    fi

    # Pack as tar.gz (or zip on windows target)
    ARCHIVE="podcastsync-v${VERSION}-${rid}"
    if [[ "$rid" == win-* ]]; then
        (cd publish && zip -qr "${ARCHIVE}.zip" "${rid}")
        echo "  -> publish/${ARCHIVE}.zip"
    else
        tar -czf "publish/${ARCHIVE}.tar.gz" -C publish "${rid}"
        echo "  -> publish/${ARCHIVE}.tar.gz"
    fi
done

echo ""
echo "=== Done. Archives in publish/ ==="
ls -lh publish/*.{tar.gz,zip} 2>/dev/null || true
