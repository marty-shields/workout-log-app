#!/usr/bin/env bash
# check-maui-pin.sh on sample global.json and action files: the workload set must share the SDK's feature band.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../check-maui-pin.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# run <name> <sdk> <action line> <expected exit>
run() {
  echo "{\"sdk\": {\"version\": \"$2\"}}" > "$work/global.json"
  echo "$3" > "$work/action.yml"
  bash "$script" "$work/global.json" "$work/action.yml" > /dev/null 2>&1
  check "$1" "$4" "exit=$?"
}

install='run: dotnet workload install maui-android --version'
run "the same band passes" 10.0.403 "$install 10.0.402.1" exit=0
run "a patch bump inside the band passes" 10.0.499 "$install 10.0.402.1" exit=0
run "a new feature band fails" 10.0.500 "$install 10.0.402.1" exit=1
run "a new major fails" 11.0.403 "$install 10.0.402.1" exit=1
run "a preview SDK is read by its band" 10.0.100-rc.2.25502.107 "$install 10.0.100-rc.2.25502.107" exit=0
run "a missing pin fails" 10.0.403 "run: dotnet workload install maui-android" exit=1
finish
