#!/usr/bin/env bash
# Usage: emulator-wait.sh <results dir>
# Waits for the emulator from emulator-start.sh to boot, reporting every 15 s; fails at once if its process ends, and after
# 15 minutes. Then sets the device up for tests and writes logcat and a health log (every 30 s) to the results dir.
# A guest still busy after boot can leave the launcher without a focused window, and a key sent then makes it "not
# respond": the ANR dialog that follows covers the app for the whole job. So it hides error dialogs first, sends no key,
# waits up to 2 minutes for a focused window and closes any error dialog left (emulator-dialogs.sh).
set -euo pipefail
source "$(dirname "$0")/emulator-dialogs.sh"
serial="${ANDROID_SERIAL:-emulator-5554}"
results="$1"
pid=$(cat "$RUNNER_TEMP/emulator.pid")
log="$RUNNER_TEMP/emulator.log"
start=$SECONDS
shown=0

while true; do
  elapsed=$((SECONDS - start))
  if ! kill -0 "$pid" 2>/dev/null; then
    echo "::error title=Emulator::The emulator process ended after ${elapsed}s."
    echo "::group::emulator.log"; cat "$log"; echo "::endgroup::"
    exit 1
  fi
  state=$(timeout 10 adb get-state 2>/dev/null || echo none)
  boot=$(timeout 10 adb shell getprop sys.boot_completed 2>/dev/null | tr -d '\r' || true)
  anim=$(timeout 10 adb shell getprop init.svc.bootanim 2>/dev/null | tr -d '\r' || true)
  lines=$(wc -l < "$log")
  echo "[${elapsed}s] adb: $state, sys.boot_completed: ${boot:-?}, bootanim: ${anim:-?}, emulator.log: $lines lines"
  if (( lines > shown )); then
    tail -n +"$((shown + 1))" "$log" | tail -n 5 | sed 's/^/    | /'
    shown=$lines
  fi
  [[ "$boot" == 1 ]] && break
  if (( elapsed > 900 )); then
    echo "::error title=Emulator::Not booted after 15 minutes."
    echo "::group::emulator.log"; cat "$log"; echo "::endgroup::"
    exit 1
  fi
  sleep 15
done
echo "Booted in $((SECONDS - start))s."

adb shell settings put global hide_error_dialogs 1
# No system animations, so the keyboard and its insets settle before the next key.
adb shell settings put global window_animation_scale 0
adb shell settings put global transition_animation_scale 0
adb shell settings put global animator_duration_scale 0
# The app is offline; no network means no background syncs or scans. The screen stays on for the whole run.
adb shell svc wifi disable || true
adb shell svc data disable || true
adb shell svc power stayon true

wait_for_focused_window() {
  local start=$SECONDS focus
  while true; do
    focus=$({ timeout 10 adb shell dumpsys window 2>/dev/null || true; } | tr -d '\r' | sed -n 's/^ *mCurrentFocus=//p')
    [[ "$focus" == Window* ]] && { echo "Focused: $focus"; return; }
    if (( SECONDS - start >= 120 )); then
      echo "::warning title=Emulator::No window has focus 2 minutes after boot (mCurrentFocus=${focus:-?}); going on."
      return
    fi
    echo "[$((SECONDS - start))s] no window has focus yet (mCurrentFocus=${focus:-?})"
    sleep 3
  done
}
wait_for_focused_window
adb shell wm dismiss-keyguard || true
close_error_dialogs "$serial"

echo "::group::Device"
adb devices -l
adb shell getprop ro.build.fingerprint
adb shell wm size
adb shell wm density
echo "CPUs: $(adb shell nproc | tr -d '\r'), $(adb shell head -n 1 /proc/meminfo | tr -d '\r')"
adb shell dumpsys webviewupdate | grep -i -E "current webview package|webview packages"
echo "::endgroup::"

mkdir -p "$results"
nohup adb logcat -v threadtime > "$results/logcat.txt" 2>&1 &
echo $! >> "$RUNNER_TEMP/background.pids"

# Every 30 s: the host's load and free memory, and whether the emulator still answers.
# shellcheck disable=SC2016 # the loop runs in its own bash, so $ must reach it unexpanded
nohup bash -c '
  while true; do
    guest=$(timeout 10 adb shell cat /proc/loadavg 2>/dev/null | tr -d "\r" | cut -d" " -f1-3)
    echo "$(date -u +%T) host load $(cut -d" " -f1-3 /proc/loadavg), $(free -m | awk "/^Mem/ {print \$7}") MB free | emulator: ${guest:-NO RESPONSE}"
    sleep 30
  done' > "$results/health.log" 2>&1 &
echo $! >> "$RUNNER_TEMP/background.pids"
