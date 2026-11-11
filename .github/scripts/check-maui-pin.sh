#!/usr/bin/env bash
# Usage: check-maui-pin.sh [global.json] [setup-maui action.yml]
# Fails when the maui-android workload set pinned in the setup-maui action is in a different .NET feature band than the
# SDK in global.json (11.0.100.1 and 11.0.100 are both 11.0.100). Dependabot bumps global.json alone.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
global="${1:-$here/../../global.json}"
action="${2:-$here/../actions/setup-maui/action.yml}"

pins=$(sed -n 's/.*workload install maui-android --version \([^ ]*\).*/\1/p' "$action")
sdk=$(jq -r '.sdk.version' "$global")
shape='^[0-9]+\.[0-9]+\.[0-9]{3}'
[[ -n "$pins" ]] || { echo "::error::No 'workload install maui-android --version' line found in $action."; exit 1; }
(( $(wc -l <<<"$pins") == 1 )) || { echo "::error::$action has more than one 'workload install maui-android --version' line, so the pin is ambiguous."; exit 1; }
pin=$pins
[[ "$pin" =~ $shape ]] || { echo "::error::The maui-android pin '$pin' in $action is not a version like 11.0.100.1."; exit 1; }
[[ "$sdk" =~ $shape ]] || { echo "::error::The SDK version '$sdk' in $global is not a version like 11.0.100."; exit 1; }

band() {
  local major minor patch
  IFS=. read -r major minor patch _ <<<"$1"
  patch=${patch%%[!0-9]*}
  echo "$major.$minor.$((patch / 100 * 100))"
}

if [[ "$(band "$pin")" != "$(band "$sdk")" ]]; then
  echo "::error::The maui-android workload set $pin is in the $(band "$pin") feature band but global.json has SDK $sdk ($(band "$sdk")). Bump the pin in .github/actions/setup-maui/action.yml in the same PR (dotnet workload search version)."
  exit 1
fi
echo "maui-android $pin matches SDK $sdk ($(band "$sdk"))."
