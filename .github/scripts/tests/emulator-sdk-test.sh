#!/usr/bin/env bash
# emulator-sdk.sh against a fake sdkmanager whose tries follow $WORK/plan, one line per try:
#   ok         installs both packages at the pinned versions
#   truncated  leaves a half-unpacked emulator folder and exits 1, like a corrupt download
#   license    installs the emulator only, says the image's license is not accepted and exits 0, as sdkmanager does
#   newer      installs an emulator newer than the pin
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../emulator-sdk.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
export WORK="$work" ANDROID_HOME="$work/sdk" SDK_RETRY_SECONDS=0
mkdir -p "$ANDROID_HOME/cmdline-tools/latest/bin"
cat > "$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager" <<'FAKE'
#!/usr/bin/env bash
image="$ANDROID_HOME/system-images/android-36/default/x86_64"
package() { mkdir -p "$1"; echo "<localPackage/>" > "$1/package.xml"; echo "Pkg.Revision=$2" > "$1/source.properties"; }
if [[ "$1" == --list_installed ]]; then
  echo "Installed packages:"
  echo "  Path                                    | Version | Description      | Location"
  [[ -f "$ANDROID_HOME/emulator/package.xml" ]] && echo "  emulator                                | 37.2.13 | Android Emulator | emulator"
  [[ -f "$image/package.xml" ]] && echo "  system-images;android-36;default;x86_64 | 2       | System Image     | system-images/android-36/default/x86_64"
  exit 0
fi
[[ -d "$ANDROID_HOME/emulator" ]] && echo leftover >> "$WORK/tries" || echo clean >> "$WORK/tries"
case "$(sed -n "$(wc -l < "$WORK/tries")p" "$WORK/plan")" in
  ok) package "$ANDROID_HOME/emulator" 37.2.13; package "$image" 2 ;;
  truncated) mkdir -p "$ANDROID_HOME/emulator/lib"; echo "Error on ZipFile unknown archive"; exit 1 ;;
  license) package "$ANDROID_HOME/emulator" 37.2.13; echo "Skipping following packages as the license is not accepted:" ;;
  newer) package "$ANDROID_HOME/emulator" 37.3.1; package "$image" 2 ;;
esac
exit 0
FAKE
chmod +x "$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager"

reset() { rm -rf "$ANDROID_HOME/emulator" "$ANDROID_HOME/system-images"; : > "$work/tries"; printf '%s\n' "$@" > "$work/plan"; }
install() { out=$(bash "$script" install 2>&1); status=$?; }
tries() { paste -s -d ' ' "$work/tries"; }

check "cache: the key names the pinned versions" "key=android-emulator-37.2.13-android-36-default-x86_64-r2-v1" \
  "$(bash "$script" cache | head -n 1)"
check "cache: the paths are the two package folders" "$ANDROID_HOME/emulator
$ANDROID_HOME/system-images/android-36/default/x86_64" "$(bash "$script" cache | sed -n '/^path<<PATHS$/,/^PATHS$/p' | sed '1d;$d')"

reset ok
install
check "install: both packages on the first try" "0 clean" "$status $(tries)"
check "check: the pinned versions pass" "0" "$(bash "$script" check > /dev/null; echo $?)"

reset truncated ok
install
check "install: a truncated download is deleted before the retry, which passes" "0 clean clean" "$status $(tries)"
check "install: the failed try is a warning" "yes" "$(grep -q '^::warning title=sdkmanager::The install failed on try 1 of 3' <<<"$out" && echo yes)"

reset truncated truncated truncated
install
check "install: 3 failed tries are an sdkmanager error, exit 1" "1 3 yes" \
  "$status $(wc -l < "$work/tries") $(grep -q '^::error title=sdkmanager::The install failed 3 times' <<<"$out" && echo yes)"

reset license ok
install
check "install: a license not accepted fails at once though sdkmanager exits 0" "1 1 yes" \
  "$status $(wc -l < "$work/tries") $(grep -q '^::error title=sdkmanager::.*license is not accepted' <<<"$out" && echo yes)"

reset newer
install
check "install: a newer stable emulator installs" "0" "$status"
check "check: an emulator newer than the pin fails, naming both versions" "1 yes" \
  "$(bash "$script" check > "$work/check.out"; echo $?) $(grep -q 'revision 37.3.1, not the pinned 37.2.13' "$work/check.out" && echo yes)"

reset ok
check "check: nothing installed fails" "1 2" \
  "$(bash "$script" check > "$work/check.out"; echo $?) $(grep -c 'has no package.xml' "$work/check.out")"

finish
