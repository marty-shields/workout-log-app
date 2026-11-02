#!/usr/bin/env bash
# Usage: e2e-local.sh <dotnet test filter> [tries]
# Boots the CI emulator (emulator-start.sh's image and flags), builds the App and the tests, runs the E2E tests in the
# filter through e2e-run.sh, then stops the emulator, whatever the result. For an agent or a developer in a worktree.
#   e2e-local.sh "Category=smoke"
#   e2e-local.sh "FullyQualifiedName~.EditAWorkoutFeature." 2
# Exit: 0 passed, 1 failed, 2 skipped (this machine can't host the emulator, or stayed too busy), 64 bad usage.
# Skips at once when a prerequisite is missing. Otherwise waits for the emulator slot and for free CPU, memory and disk,
# and skips after MAX_WAIT_SECONDS (default 3600) in all. One emulator runs per machine: runs from other worktrees queue
# on a lock. A run takes a boot, a build and the tests (10+ minutes) plus any wait: run it in the background.
# Output: tests/WorkoutLog.E2E.Tests/TestResults/E2E-emulator/ (results, logcat, emulator log) and, for a failed
# scenario's screen, page source and Appium log, TestResults/E2E/<yyyyMMdd-HHmmss>/. E2E_TRIES is the total number of
# tries (1 = no retry) when none is passed.
set -euo pipefail

usage() {
  sed -n '2,6p' "$script" >&2
  exit 64
}
script=$(readlink -f "$0")
filter="${1:-}"
tries="${2:-${E2E_TRIES:-1}}"
[[ -n "$filter" && "$tries" =~ ^[1-9][0-9]*$ ]] || usage
cd "$(git rev-parse --show-toplevel)"

AVD="${AVD:-WorkoutLog_Local_API36}"
SYSTEM_IMAGE="${SYSTEM_IMAGE:-system-images;android-36;default;x86_64}"
SERIAL=emulator-5554
EMULATOR_CORES=3
EMULATOR_MB=4096
# The emulator's cores and memory, plus what dotnet, Appium and adb need beside it.
MIN_CPUS=4
MIN_TOTAL_MB=8192
MIN_FREE_MB=$((EMULATOR_MB + 2048))
MIN_FREE_GB=10
max_wait="${MAX_WAIT_SECONDS:-3600}"

lockfile="/tmp/workoutlog-emulator-$(id -u).lock"
pidfile="/tmp/workoutlog-emulator-$(id -u).pid"
results="$PWD/tests/WorkoutLog.E2E.Tests/TestResults/E2E-emulator"
deadline=$((SECONDS + max_wait))
state=""
skipped=""
child=""

skip() {
  skipped=1
  echo "SKIPPED: $*" >&2
  exit 2
}

# Every long step runs through here: the lock isn't inherited by what outlives the run (build servers, adb), and a
# signal stops the step at once instead of after it.
run() {
  "$@" 9>&- &
  child=$!
  local status=0
  wait "$child" || status=$?
  child=""
  return "$status"
}

teardown() {
  local status=$?
  trap '' INT TERM PIPE
  set +e
  if [[ -n "$state" ]]; then
    [[ -f "$state/background.pids" ]] && xargs -r kill < "$state/background.pids" 2>/dev/null
    if [[ -f "$state/emulator.pid" ]]; then
      pid=$(cat "$state/emulator.pid")
      echo "Stopping the emulator (pid $pid)."
      kill -0 "$pid" 2>/dev/null && timeout 20 adb -s "$SERIAL" emu kill 9>&- > /dev/null 2>&1
      for _ in $(seq 30); do kill -0 "$pid" 2>/dev/null || break; sleep 1; done
      kill -0 "$pid" 2>/dev/null && kill -9 "$pid"
      rm -f "$pidfile"
    fi
    [[ -d "$results" && -f "$state/emulator.log" ]] && cp "$state/emulator.log" "$results/"
    rm -rf "$state"
  fi
  case $status in
    0 | 64 | 130 | 141) ;;
    2) [[ -n "$skipped" ]] || status=1 ;;
    *) status=1 ;;
  esac
  exit "$status"
}
trap teardown EXIT
trap '[[ -n "$child" ]] && kill "$child" 2>/dev/null; exit 130' INT TERM
# A reader that stops (| head) would otherwise kill the script before teardown runs.
trap '[[ -n "$child" ]] && kill "$child" 2>/dev/null; exit 141' PIPE

echo "::group::Prerequisites"
# An agent's shell is non-interactive and never reads the profile that sets these: fall back to the usual places.
if [[ -z "${ANDROID_HOME:-}" ]]; then
  for dir in "$HOME/Android/Sdk" "$HOME/Android/sdk"; do
    if [[ -d "$dir" ]]; then ANDROID_HOME="$dir"; break; fi
  done
fi
if [[ -z "${JAVA_HOME:-}" ]] && command -v java > /dev/null; then
  JAVA_HOME=$(dirname "$(dirname "$(readlink -f "$(command -v java)")")")
fi
if [[ -x "$HOME/.dotnet/dotnet" ]]; then
  export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
  PATH="$DOTNET_ROOT:$PATH"
fi
if [[ -x "$HOME/.local/node/bin/node" ]]; then
  PATH="$HOME/.local/node/bin:$PATH"
fi
export ANDROID_HOME JAVA_HOME

for tool in git curl unzip sha256sum flock node npm dotnet; do
  command -v "$tool" > /dev/null || skip "$tool is not installed."
done
[[ -n "${ANDROID_HOME:-}" && -d "$ANDROID_HOME" ]] || skip "ANDROID_HOME is not set and there is no SDK in ~/Android/Sdk."
sdk="$ANDROID_HOME/cmdline-tools/latest/bin"
for tool in "$sdk/avdmanager" "$ANDROID_HOME/emulator/emulator" "$ANDROID_HOME/platform-tools/adb"; do
  [[ -x "$tool" ]] || skip "$tool is missing: install the Android SDK command-line tools, emulator and platform-tools."
done
image_dir="$ANDROID_HOME/$(tr ';' '/' <<<"$SYSTEM_IMAGE")"
[[ -d "$image_dir" ]] || skip "the system image is not installed: sdkmanager \"$SYSTEM_IMAGE\"."
export PATH="$ANDROID_HOME/platform-tools:$PATH"
export ANDROID_SERIAL="$SERIAL"
export ANDROID_AVD_HOME="${ANDROID_AVD_HOME:-$HOME/.android/avd}"
mkdir -p "$ANDROID_AVD_HOME"

[[ -n "${JAVA_HOME:-}" && -x "$JAVA_HOME/bin/java" ]] || skip "JAVA_HOME is not set and no JDK is on the PATH."
node -e 'const [a, b] = process.versions.node.split(".").map(Number); process.exit(a >= 24 || (a === 22 && b >= 12) || (a === 20 && b >= 19) ? 0 : 1)' \
  || skip "Node.js $(node -v) is too old: Appium needs 20.19+, 22.12+ or 24+."
dotnet --version > /dev/null 2>&1 || skip "the .NET SDK in global.json is not installed (using $(command -v dotnet))."
workload_pin=$(sed -n 's/.*maui-android --version \([0-9.]*\).*/\1/p' .github/workflows/dev-pipeline.yml | head -n 1)
workload=$(dotnet workload --version 2>/dev/null || true)
[[ "$workload" == "$workload_pin" ]] \
  || skip "the maui-android workload set is '${workload:-none}', CI uses $workload_pin: dotnet workload install maui-android --version $workload_pin."
workloads=$(dotnet workload list 2>&1 || true)
grep -q 'maui-android' <<<"$workloads" || skip "the maui-android workload is not installed: dotnet workload install maui-android --version $workload_pin."

(( $(nproc) >= MIN_CPUS )) || skip "$(nproc) CPUs; the emulator needs at least $MIN_CPUS."
total_mb=$(awk '/MemTotal/ {print int($2 / 1024)}' /proc/meminfo)
(( total_mb >= MIN_TOTAL_MB )) || skip "${total_mb} MB of memory; the emulator needs at least $MIN_TOTAL_MB."
{ [[ -r /dev/kvm && -w /dev/kvm ]]; } || skip "no read-write access to /dev/kvm: enable virtualisation and add yourself to the kvm group."
libs=$(PATH="$PATH:/sbin:/usr/sbin" ldconfig -p 2> /dev/null || true)
grep -q 'libpulse\.so\.0' <<<"$libs" || skip "libpulse0 is not installed (the emulator links it)."
"$ANDROID_HOME/emulator/emulator" -accel-check > /dev/null 2>&1 || skip "the emulator reports no hardware acceleration."
echo "All present."
echo "::endgroup::"

busy_reason() {
  local avail_mb free_gb load devices
  avail_mb=$(awk '/MemAvailable/ {print int($2 / 1024)}' /proc/meminfo)
  free_gb=$(df -BG --output=avail "$HOME" | tail -n 1 | tr -dc 0-9)
  load=$(cut -d' ' -f1 /proc/loadavg)
  devices=$(timeout 10 adb devices 9>&- 2>/dev/null || true)
  if grep -q "^$SERIAL" <<<"$devices"; then
    echo "$SERIAL is already in use"
  elif command -v ss > /dev/null && [[ -n "$(ss -Hltn '( sport = :5554 or sport = :5555 )' 2>/dev/null)" ]]; then
    echo "ports 5554 and 5555 are in use"
  elif (( avail_mb < MIN_FREE_MB )); then
    echo "${avail_mb} MB of memory free, need $MIN_FREE_MB"
  elif (( free_gb < MIN_FREE_GB )); then
    echo "${free_gb} GB of disk free, need $MIN_FREE_GB"
  elif awk -v load="$load" -v cpus="$(nproc)" -v need="$EMULATOR_CORES" 'BEGIN { exit !(load + need > cpus) }'; then
    echo "load average $load on $(nproc) CPUs, need $EMULATOR_CORES free"
  fi
}

# One emulator per machine: other worktrees' runs queue on this lock. The kernel drops it when the run dies. Anything
# that can start a daemon (adb) runs with 9>&-, or the daemon would hold the lock after the run.
exec 9> "$lockfile"
announced=""
until flock -n 9; do
  (( SECONDS < deadline )) || skip "the emulator slot stayed taken for ${max_wait}s."
  [[ -n "$announced" ]] || echo "Another emulator run holds the slot; waiting."
  announced=1
  run sleep 5
done

# Holding the lock means no live run owns an emulator: one left by a killed run would block the port for good.
if [[ -f "$pidfile" ]]; then
  old=$(cat "$pidfile")
  if [[ -n "$old" ]] && kill -0 "$old" 2> /dev/null && grep -qa -- "$AVD" "/proc/$old/cmdline" 2> /dev/null; then
    echo "Stopping an emulator left by a killed run (pid $old)."
    kill "$old" 2> /dev/null || true
    for _ in $(seq 30); do kill -0 "$old" 2> /dev/null || break; sleep 1; done
    kill -9 "$old" 2> /dev/null || true
  fi
  rm -f "$pidfile"
fi

while reason=$(busy_reason) && [[ -n "$reason" ]]; do
  (( SECONDS < deadline )) || skip "the machine stayed too busy for ${max_wait}s ($reason)."
  echo "[$(date +%T)] Waiting for resources: $reason."
  run sleep 30
done

state=$(mktemp -d)
export RUNNER_TEMP="$state"
rm -rf "$results"
mkdir -p "$results"

# The AVD outlives the run; recreate it when the image it was made from isn't the one asked for.
avds=$("$sdk/avdmanager" list avd -c)
image_path="$(tr ';' '/' <<<"$SYSTEM_IMAGE")/"
if ! grep -qx "$AVD" <<<"$avds" || ! grep -qxF "image.sysdir.1 = $image_path" "$ANDROID_AVD_HOME/$AVD.avd/config.ini" 2> /dev/null; then
  echo "Creating the AVD $AVD."
  echo no | "$sdk/avdmanager" create avd --force -n "$AVD" -k "$SYSTEM_IMAGE" -d pixel_7 > /dev/null
fi

# CI's flags (emulator-start.sh), with a clean data partition since the AVD outlives the run.
echo "Starting the emulator."
nohup "$ANDROID_HOME/emulator/emulator" -avd "$AVD" -port 5554 -no-window -no-audio -no-boot-anim -no-snapshot -wipe-data \
  -cores "$EMULATOR_CORES" -memory "$EMULATOR_MB" -partition-size 2048 -gpu swangle_indirect -verbose \
  > "$state/emulator.log" 2>&1 9>&- &
echo $! > "$state/emulator.pid"
cp "$state/emulator.pid" "$pidfile"

# CI installs Appium on every run; do the same when the lockfile is newer than what is installed.
e2e=tests/WorkoutLog.E2E.Tests
if [[ ! -d $e2e/node_modules || $e2e/package-lock.json -nt $e2e/node_modules/.package-lock.json ]]; then
  run npm ci --ignore-scripts --prefix "$e2e"
fi
run dotnet build src/WorkoutLog.App -f net10.0-android
run dotnet build "$e2e"

run .github/scripts/emulator-wait.sh "$results" || exit 1

# The Chromedriver for the image's WebView, pinned in qa-pipeline.yml and cached.
CHROMEDRIVER=$(.github/scripts/chromedriver.sh "${XDG_CACHE_HOME:-$HOME/.cache}/workoutlog-e2e" 9>&-) || exit 1

export RESULTS="$results"
export CHROMEDRIVER
run .github/scripts/e2e-run.sh "$filter" "$tries" || exit 1
