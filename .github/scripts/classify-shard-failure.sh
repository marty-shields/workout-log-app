#!/usr/bin/env bash
# Usage: classify-shard-failure.sh <log of a failed emulator shard>
# Prints `infra` when the log shows the emulator or its UiAutomator2 server failing and no assertion failing, else `test`.
# `infra` licenses one rerun of the job, not more; `test` is the safe default, so a log with both is `test`.
# Infra signatures:
#   Could not find a connected Android device     Appium found no device
#   adb: device offline                           an adb call hit a dead device (emulator.log lines, which start with `|`, don't count)
#   instrumentation process is not running        the UiAutomator2 server crashed
#   adb shell date +%s failed (255)               adb lost the device mid-test
#   WEBVIEW_<package> didn't appear               the app's WebView never showed up
#   The emulator did not recover                  emulator-recover.sh gave up
# An assertion failure is an NUnit `Expected:` line.
set -euo pipefail
log="${1:?usage: classify-shard-failure.sh <log>}"

infra='Could not find a connected Android device|adb: device offline|instrumentation process is not running|adb shell date \+%s failed \(255\)|WEBVIEW_[^ ]+ didn.t appear|The emulator did not recover'
assertion='(^|Z) +Expected:'

if grep -q -E "$assertion" "$log"; then
  echo test
elif grep -v -E 'Z +\|' "$log" | grep -q -E "$infra"; then
  echo infra
else
  echo test
fi
