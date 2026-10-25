#!/usr/bin/env bash
# Usage: release-smoke.sh <apk>
# Installs a Release APK on the emulator, launches it and fails unless the log shows a clean start.
set -euo pipefail
apk="$1"
package=uk.martinshields.workoutlog

fail() { echo "::error title=Release smoke::$1"; exit 1; }

adb install -r "$apk"
adb logcat -c
adb logcat -b crash -c
adb shell monkey -p "$package" -c android.intent.category.LAUNCHER 1 > /dev/null

for _ in $(seq 60); do
  adb logcat -d -s WorkoutLog | grep -q -E "Startup (finished|failed)" && break
  sleep 2
done
sleep 5

log=$(adb logcat -d -v brief -s WorkoutLog)
echo "$log"
grep -q "Release started on Android API" <<<"$log" || fail "No Release start line."
grep -q -E "Exercise list seeded: [1-9][0-9]* added" <<<"$log" || fail "The exercise list wasn't seeded."
grep -q "Startup finished" <<<"$log" || fail "Startup didn't finish."
! grep -q "Startup failed" <<<"$log" || fail "Startup failed."
! grep -q "^[EF]/WorkoutLog" <<<"$log" || fail "The app logged an error."
crash=$(adb logcat -d -b crash)
! grep -q "$package" <<<"$crash" || { echo "$crash"; fail "The app crashed."; }
adb shell pidof "$package" > /dev/null || fail "The app isn't running."
echo "Release smoke passed."
