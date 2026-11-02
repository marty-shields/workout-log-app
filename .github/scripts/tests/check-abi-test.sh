#!/usr/bin/env bash
# check-abi.sh on APKs (zips) built with the given native library folders.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../check-abi.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# apk <name> <abi...>: a zip with lib/<abi>/libx.so for each abi, plus a dex.
apk() {
  python3 -I - "$work/$1.apk" "${@:2}" <<'PY'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1], "w") as z:
    z.writestr("classes.dex", "")
    for abi in sys.argv[2:]:
        z.writestr(f"lib/{abi}/libx.so", "")
PY
}

# run <name>: the exit code, then the output.
run() { local out; out=$(bash "$script" "$work/$1.apk" 2>&1); echo "exit=$?"; echo "$out"; }

apk arm64 arm64-v8a
apk both arm64-v8a x86_64
apk x64 x86_64
apk none

check "arm64-v8a only passes" $'exit=0\nABIs arm64-v8a' "$(run arm64)"
check "arm64-v8a with x86_64 fails" $'exit=1\n::error title=APK check::The APK must hold arm64-v8a only; it has: arm64-v8a x86_64' "$(run both)"
check "x86_64 only fails" $'exit=1\n::error title=APK check::The APK must hold arm64-v8a only; it has: x86_64' "$(run x64)"
check "no native libraries fails" $'exit=1\n::error title=APK check::The APK must hold arm64-v8a only; it has: none' "$(run none)"

finish
