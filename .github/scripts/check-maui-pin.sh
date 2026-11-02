#!/usr/bin/env bash
# Usage: check-maui-pin.sh [global.json] [setup-maui action.yml]
# Fails when the maui-android workload set pinned in the setup-maui action is in a different .NET feature band than the
# SDK in global.json (10.0.402.1 and 10.0.403 are both 10.0.400). Dependabot bumps global.json alone.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
global="${1:-$here/../../global.json}"
action="${2:-$here/../actions/setup-maui/action.yml}"

pin=$(sed -n 's/.*maui-android --version \([0-9][0-9.]*\).*/\1/p' "$action")
sdk=$(jq -r '.sdk.version' "$global")
[[ -n "$pin" ]] || { echo "::error::No 'maui-android --version' pin found in $action."; exit 1; }

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
