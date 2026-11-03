#!/usr/bin/env bash
# emulator-recover.sh and its use in e2e-run.sh, against a fake adb, emulator and dotnet on the PATH.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
scripts="$here/.."

work=$(mktemp -d)
trap 'kill "$(cat "$work/tmp/emulator.pid" 2>/dev/null)" 2>/dev/null; rm -rf "$work"' EXIT
mkdir -p "$work/bin" "$work/sdk/emulator" "$work/tmp"

# The device state is the content of $work/state; "reconnect-works" in $work/reconnect makes `adb reconnect` bring it back.
cat > "$work/bin/adb" <<'FAKE'
#!/usr/bin/env bash
[[ "$1" == -s ]] && shift 2
echo "adb $*" >> "$WORK/adb.log"
state=$(cat "$WORK/state")
case "$1" in
  get-state) [[ "$state" == none ]] && { echo "error: no devices/emulators found" >&2; exit 1; }; echo "$state" ;;
  reconnect) [[ -f "$WORK/reconnect" ]] && echo device > "$WORK/state" ;;
  wait-for-device) ;;
  emu) echo none > "$WORK/state" ;;
  shell)
    shift
    case "$1" in
      echo) [[ "$state" == device ]] && echo ok || { echo "adb: device offline" >&2; exit 1; } ;;
      getprop) [[ "$2" == sys.boot_completed ]] && echo 1 ;;
      dumpsys) echo "Current WebView package (name, version): (com.android.webview, 133)" ;;
      nproc) echo 3 ;;
      head) echo "MemTotal: 4096 kB" ;;
    esac ;;
esac
exit 0
FAKE
# The fake emulator comes up as soon as it starts, unless $work/emulator-broken exists.
cat > "$work/sdk/emulator/emulator" <<'FAKE'
#!/usr/bin/env bash
echo "$*" >> "$WORK/emulator.args"
[[ -f "$WORK/emulator-broken" ]] && exit 1
echo device > "$WORK/state"
exec sleep 30
FAKE
# The fake dotnet fails its first run with the sample trx and passes the next ones.
cat > "$work/bin/dotnet" <<'FAKE'
#!/usr/bin/env bash
echo "dotnet" >> "$WORK/dotnet.log"
trx=$(grep -o 'LogFileName=[^ ]*' <<<"$*" | cut -d= -f2)
if (( $(wc -l < "$WORK/dotnet.log") == 1 )); then
  mkdir -p "$RESULTS/trx"; cp "$FIXTURES/sample-trx.xml" "$RESULTS/trx/$trx"; exit 1
fi
exit 0
FAKE
chmod +x "$work/bin/"* "$work/sdk/emulator/emulator"

export WORK="$work" FIXTURES="$here/fixtures" PATH="$work/bin:$PATH" ANDROID_HOME="$work/sdk" RUNNER_TEMP="$work/tmp"
export GITHUB_ACTIONS=true RESULTS="$work/results" CHROMEDRIVER=/none GITHUB_STEP_SUMMARY="$work/summary.md"
sleep 30 &
echo $! > "$work/tmp/emulator.pid"
reset() { echo "$1" > "$work/state"; : > "$work/adb.log"; : > "$work/emulator.args"; : > "$work/dotnet.log"; rm -f "$work/reconnect" "$work/emulator-broken" "$work/summary.md"; rm -rf "$work/results"; }
recover() { bash "$scripts/emulator-recover.sh" "$RESULTS" "$@" 2>&1; }

reset device
out=$(recover); status=$?
check "an answering device: nothing to do" "0 0" "$status $(grep -c -E 'reconnect|emu|uninstall' "$work/adb.log")"

reset offline
touch "$work/reconnect"
out=$(recover); status=$?
check "an offline device that comes back on a reconnect: no emulator restart" "0 0 device" "$status $(wc -l < "$work/emulator.args") $(cat "$work/state")"

reset offline
out=$(recover); status=$?
check "an offline device that stays offline: the emulator starts again with the job's flags" \
  "0 -avd WorkoutLog_API36 -no-window -no-audio -no-boot-anim -no-snapshot -cores 3 -memory 4096 -partition-size 2048 -gpu swangle_indirect -verbose" \
  "$status $(cat "$work/emulator.args")"
check "an offline device that stays offline: the device answers again" "device" "$(cat "$work/state")"

reset device
out=$(recover reset-server); status=$?
check "reset-server: the UiAutomator2 server is stopped and uninstalled" "adb shell am force-stop io.appium.uiautomator2.server
adb uninstall io.appium.uiautomator2.server
adb shell am force-stop io.appium.uiautomator2.server.test
adb uninstall io.appium.uiautomator2.server.test" "$(grep -E 'force-stop|uninstall' "$work/adb.log")"

reset offline
touch "$work/emulator-broken"
out=$(recover); status=$?
check "an emulator that won't boot: an Emulator error, exit 1" "1 yes" "$status $(grep -q '::error title=Emulator::The emulator did not recover' <<<"$out" && echo yes)"

reset offline
out=$(GITHUB_ACTIONS='' recover); status=$?
check "outside CI: no restart, an Emulator error, exit 1" "1 0 yes" "$status $(wc -l < "$work/emulator.args") $(grep -q '::error title=Emulator::' <<<"$out" && echo yes)"

# e2e-run.sh: try 1 fails on the sample trx's failed scenarios; try 2 passes.
retry() { bash "$scripts/e2e-run.sh" "Category=x" 2 2>&1; }

reset device
out=$(retry); status=$?
check "e2e-run: a retry on a healthy device passes, and the server is reset before it only" "0 2 2" \
  "$status $(wc -l < "$work/dotnet.log") $(grep -c 'adb uninstall' "$work/adb.log")"
check "e2e-run: the first run's failed scenarios are annotated and tabled in the job summary" "yes yes" \
  "$(grep -q '^::error title=E2E%3A Plain::' <<<"$out" && echo yes) $(grep -q '^| E2E | 2 | 4 | 0 |$' "$work/summary.md" && echo yes)"

reset device
# Kills the device between the tries by wrapping the first dotnet run.
mv "$work/bin/dotnet" "$work/bin/dotnet-real"
cat > "$work/bin/dotnet" <<'FAKE'
#!/usr/bin/env bash
"$WORK/bin/dotnet-real" "$@"; status=$?
(( $(wc -l < "$WORK/dotnet.log") == 1 )) && echo offline > "$WORK/state"
exit "$status"
FAKE
chmod +x "$work/bin/dotnet"
out=$(retry); status=$?
check "e2e-run: the device dies after try 1, the emulator restarts and try 2 passes" "0 2 1 device" \
  "$status $(wc -l < "$work/dotnet.log") $(wc -l < "$work/emulator.args") $(cat "$work/state")"

reset device
touch "$work/emulator-broken"
out=$(retry); status=$?
check "e2e-run: a device that can't be recovered stops the job before try 2" "1 1 yes" \
  "$status $(wc -l < "$work/dotnet.log") $(grep -q '::error title=Emulator::' <<<"$out" && echo yes)"

finish
