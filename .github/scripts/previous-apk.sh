#!/usr/bin/env bash
# Usage: previous-apk.sh <out apk>
# Builds the Debug APK of the last release, the latest v* tag before this commit, to <out apk> for the @upgrade scenario,
# and prints the tag. The tag's files are built with this commit's global.json, so the SDK CI installs builds them. Build
# this commit's APK on the same machine: one APK installs over another only when the same debug key signs both.
set -euo pipefail
out=$(realpath -m -- "$1")
cd "$(git rev-parse --show-toplevel)"

tag=$(git describe --tags --abbrev=0 --match 'v[0-9]*' HEAD 2> /dev/null) || {
  echo "No v* tag before $(git rev-parse --short HEAD): fetch the tags (git fetch --tags, or fetch-depth: 0)." >&2
  exit 1
}
src=$(mktemp -d)
trap 'rm -rf "$src"' EXIT
git archive "$tag" | tar -x -C "$src"
cp global.json "$src/global.json"
dotnet build "$src/src/WorkoutLog.App" -f net10.0-android -p:TreatWarningsAsErrors=false -p:MSBuildTreatWarningsAsErrors=false >&2
cp "$src/src/WorkoutLog.App/bin/Debug/net10.0-android/uk.martinshields.workoutlog.debug-Signed.apk" "$out"
echo "$tag"
