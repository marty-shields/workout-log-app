#!/usr/bin/env bash
# Usage: emulator-recover.sh <results dir> [reset-server]
# Makes sure emulator-5554 answers before a test try. A device that is not "device" or does not run a shell command gets
# `adb reconnect`; if it is still down, the emulator is stopped and booted again the way emulator-start.sh did
# (emulator-launch.sh, emulator-wait.sh; CI only). With reset-server, the UiAutomator2 server is stopped and uninstalled
# too, since a crashed one keeps failing every call; Appium installs it again. Then an ANR or app-error dialog on the
# screen is closed, with an `Emulator` warning (emulator-dialogs.sh).
# Exit 1 with an `Emulator` error if the device stays down.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/emulator-dialogs.sh"
results="$1"
reset_server="${2:-}"
serial=emulator-5554

# adb prints nothing, or only an error, for a missing device.
healthy() {
  [[ "$(timeout 10 adb -s "$serial" get-state 2>/dev/null || true)" == device ]] &&
    [[ "$(timeout 10 adb -s "$serial" shell echo ok 2>/dev/null | tr -d '\r' || true)" == ok ]]
}

fail() {
  echo "::error title=Emulator::The emulator did not recover: $*"
  exit 1
}

if ! healthy; then
  echo "::warning title=Emulator::The device is not answering (adb get-state: $(timeout 10 adb -s "$serial" get-state 2>&1 | head -n 1)); reconnecting."
  timeout 10 adb reconnect > /dev/null 2>&1 || true
  timeout 30 adb -s "$serial" wait-for-device > /dev/null 2>&1 || true
  if ! healthy; then
    [[ "${GITHUB_ACTIONS:-}" == true && -f "${RUNNER_TEMP:-}/emulator.pid" ]] || fail "it is still down and this run cannot start it again."
    echo "::warning title=Emulator::Still down after a reconnect; starting the emulator again."
    timeout 10 adb -s "$serial" emu kill > /dev/null 2>&1 || true
    pid=$(cat "$RUNNER_TEMP/emulator.pid")
    kill "$pid" 2> /dev/null || true
    for _ in $(seq 20); do kill -0 "$pid" 2> /dev/null || break; sleep 1; done
    kill -9 "$pid" 2> /dev/null || true
    mv -f "$RUNNER_TEMP/emulator.log" "$RUNNER_TEMP/emulator-before-restart.log" 2> /dev/null || true
    source "$here/emulator-launch.sh"
    launch_emulator
    "$here/emulator-wait.sh" "$results/restart" || fail "it did not boot again."
    healthy || fail "it booted but does not answer."
  fi
  echo "::notice title=Emulator::The device answers again."
fi

if [[ "$reset_server" == reset-server ]]; then
  for package in io.appium.uiautomator2.server io.appium.uiautomator2.server.test; do
    timeout 20 adb -s "$serial" shell am force-stop "$package" > /dev/null 2>&1 || true
    timeout 30 adb -s "$serial" uninstall "$package" > /dev/null 2>&1 || true
  done
fi

close_error_dialogs "$serial"
