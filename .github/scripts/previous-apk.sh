#!/usr/bin/env bash
# Usage: previous-apk.sh <out apk> [Debug|Release]
# Builds the APK of the last release, the latest v* tag before this commit, to <out apk> for the upgrade check, and prints
# the tag. Debug (the default) is the Debug app; Release is the x64 Release app, signed with the debug key like this
# commit's in release-e2e.yml. The tag's files are built with this commit's global.json, so the SDK CI installs builds
# them, for the tag's own NetVersion. For an older NetVersion that SDK can bring implicit packages (such as
# Microsoft.NET.ILLink.Tasks) at versions the tag's lock files don't hold, so the restore is locked only when the tag's
# NetVersion is this commit's. Build this commit's APK on the same machine, or with the same debug keystore: one APK
# installs over another only when the same key signs both.
set -euo pipefail
out=$(realpath -m -- "$1")
configuration="${2:-Debug}"
cd "$(git rev-parse --show-toplevel)"

tag=$(git describe --tags --abbrev=0 --match 'v[0-9]*' HEAD 2> /dev/null) || {
  echo "No v* tag before $(git rev-parse --short HEAD): fetch the tags (git fetch --tags, or fetch-depth: 0)." >&2
  exit 1
}
src=$(mktemp -d)
trap 'rm -rf "$src"' EXIT
git archive "$tag" | tar -x -C "$src"
cp global.json "$src/global.json"
app="$src/src/WorkoutLog.App"
net_version() { sed -n 's:.*<NetVersion>\(.*\)</NetVersion>.*:\1:p' "$1/Directory.Build.props"; }
net=$(net_version "$src")
[[ -n "$net" ]] || { echo "No NetVersion in $tag's Directory.Build.props." >&2; exit 1; }
lenient=(-p:TreatWarningsAsErrors=false -p:MSBuildTreatWarningsAsErrors=false)
[[ "$net" == "$(net_version .)" ]] || lenient+=(-p:RestoreLockedMode=false)
case "$configuration" in
  Debug)
    dotnet build "$app" -f "$net-android" "${lenient[@]}" >&2
    cp "$app/bin/Debug/$net-android/uk.martinshields.workoutlog.debug-Signed.apk" "$out"
    ;;
  Release)
    dotnet restore "$app" "${lenient[@]}" >&2
    dotnet publish "$app" -c Release -f "$net-android" -p:AndroidPackageFormat=apk -r android-x64 --no-restore \
      "${lenient[@]}" -o "$src/publish" >&2
    cp "$src/publish/uk.martinshields.workoutlog-Signed.apk" "$out"
    ;;
  *)
    echo "Usage: previous-apk.sh <out apk> [Debug|Release]" >&2
    exit 64
    ;;
esac
echo "$tag"
