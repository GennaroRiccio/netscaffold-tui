#!/usr/bin/env bash
#
# NetScaffold TUI Build Script for Linux/macOS
#
# Usage:
#   ./build.sh              # build all targets
#   ./build.sh linux-x64    # build only Linux x64
#   ./build.sh --config Debug
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/src/NetScaffoldTui/NetScaffoldTui.csproj"
OUTPUT_ROOT="$SCRIPT_DIR/publish"

CONFIG="Release"
RUNTIME=""
TARGETS=("linux-x64" "osx-x64" "osx-arm64")

usage() {
    echo "Usage: $0 [runtime] [--config Debug|Release]"
    echo "Runtimes: win-x64, linux-x64, osx-x64, osx-arm64"
    exit 1
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --config)
            CONFIG="$2"
            shift 2
            ;;
        -c)
            CONFIG="$2"
            shift 2
            ;;
        win-x64|linux-x64|osx-x64|osx-arm64)
            RUNTIME="$1"
            shift
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo "Unknown option: $1"
            usage
            ;;
    esac
done

if [[ -n "$RUNTIME" ]]; then
    TARGETS=("$RUNTIME")
fi

echo "=== NetScaffold TUI Build ==="
echo "Configuration: $CONFIG"
echo "Targets: ${TARGETS[*]}"
echo ""

failed=()

for rid in "${TARGETS[@]}"; do
    out_dir="$OUTPUT_ROOT/$rid"
    echo "[$rid] Publishing to $out_dir ..."
    
    dotnet publish "$PROJECT" \
        --configuration "$CONFIG" \
        --runtime "$rid" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=false \
        --output "$out_dir"
    
    if [[ $? -ne 0 ]]; then
        echo "[$rid] ERROR" >&2
        failed+=("$rid")
    else
        echo "[$rid] OK"
    fi
    echo ""
done

echo "=== Summary ==="
for rid in "${TARGETS[@]}"; do
    if [[ " ${failed[*]} " =~ " $rid " ]]; then
        echo "  $rid: FAILED"
    else
        echo "  $rid: $OUTPUT_ROOT/$rid"
    fi
done

if [[ ${#failed[@]} -gt 0 ]]; then
    echo ""
    echo "Build failed for: ${failed[*]}"
    exit 1
fi

echo ""
echo "Build complete!"