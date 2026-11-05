#!/usr/bin/env bash
# Usage: emulator-sdk.sh cache | check | install
# The emulator and system image pinned in .github/android-emulator.env, in $ANDROID_HOME.
#   cache    prints the cache key and the two package folders as step outputs `key` and `path`, for actions/cache.
#   check    exits 0 when both packages are installed at the pinned versions, else prints what differs and exits 1.
#   install  installs the latest stable emulator and the image with sdkmanager: up to 3 tries $SDK_RETRY_SECONDS (default
#            45) apart, deleting half-unpacked folders before a retry, since a truncated download stays truncated on a
#            quick retry. Fails with an `sdkmanager` error when either package is still missing, or at once when
#            sdkmanager skipped one for a license that isn't accepted (it exits 0 then).
# There is no `sdkmanager --licenses`: the runner ships the android-sdk-license, the only license both packages use.
set -euo pipefail
# shellcheck source=.github/android-emulator.env
source "$(dirname "$0")/../android-emulator.env"
emulator_dir="$ANDROID_HOME/emulator"
image_dir="$ANDROID_HOME/$(tr ';' '/' <<<"$SYSTEM_IMAGE")"
sdkmanager="$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager"

revision() { sed -n 's/^Pkg\.Revision=//p' "$1/source.properties" 2> /dev/null | tr -d '\r'; }

# The first column of `sdkmanager --list_installed` is the package path.
listed() {
  "$sdkmanager" --list_installed 2> /dev/null | awk -F'|' '{ gsub(/ /, "", $1); print $1 }' |
    grep -c -x -F -e emulator -e "$SYSTEM_IMAGE" || true
}

case "${1:-}" in
  cache)
    echo "key=android-emulator-$EMULATOR_VERSION-$(tr ';' '-' <<<"${SYSTEM_IMAGE#system-images;}")-r$SYSTEM_IMAGE_REVISION-v$CACHE_VERSION"
    printf 'path<<PATHS\n%s\n%s\nPATHS\n' "$emulator_dir" "$image_dir"
    ;;
  check)
    status=0
    for package in "$emulator_dir:$EMULATOR_VERSION" "$image_dir:$SYSTEM_IMAGE_REVISION"; do
      dir="${package%:*}" want="${package##*:}"
      if [[ ! -f "$dir/package.xml" ]]; then
        echo "$dir has no package.xml."; status=1
      elif [[ "$(revision "$dir")" != "$want" ]]; then
        echo "$dir is revision $(revision "$dir"), not the pinned $want."; status=1
      fi
    done
    exit $status
    ;;
  install)
    out=$(mktemp)
    for try in 1 2 3; do
      status=0
      "$sdkmanager" --install emulator "$SYSTEM_IMAGE" 2>&1 | tee "$out" || status=$?
      if grep -q 'not accepted' "$out"; then
        echo "::error title=sdkmanager::sdkmanager skipped a package whose license is not accepted. Add the license to \$ANDROID_HOME/licenses."
        exit 1
      fi
      (( status == 0 && $(listed) == 2 )) && break
      if (( try == 3 )); then
        echo "::error title=sdkmanager::The install failed 3 times (sdkmanager exit $status; --list_installed shows $(listed) of emulator and $SYSTEM_IMAGE)."
        exit 1
      fi
      echo "::warning title=sdkmanager::The install failed on try $try of 3 (exit $status); deleting what it unpacked and trying again in ${SDK_RETRY_SECONDS:-45}s."
      rm -rf "$emulator_dir" "$image_dir" "$ANDROID_HOME/.temp"
      sleep "${SDK_RETRY_SECONDS:-45}"
    done
    "$sdkmanager" --list_installed 2> /dev/null | grep -E "emulator|system-images|platform-tools|build-tools"
    ;;
  *)
    echo "usage: emulator-sdk.sh cache | check | install" >&2
    exit 64
    ;;
esac
