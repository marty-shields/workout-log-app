#!/usr/bin/env bash
# Usage: check-abi.sh <apk>
# Fails unless the APK's native libraries are for arm64-v8a only.
set -euo pipefail
apk="$1"
abis=$(unzip -Z1 "$apk" | sed -n 's|^lib/\([^/]*\)/.*|\1|p' | sort -u | paste -sd ' ')
[[ "$abis" == "arm64-v8a" ]] || { echo "::error title=APK check::The APK must hold arm64-v8a only; it has: ${abis:-none}"; exit 1; }
echo "ABIs $abis"
