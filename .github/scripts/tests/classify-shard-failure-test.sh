#!/usr/bin/env bash
# classify-shard-failure.sh against trimmed logs of real failed shards (fixtures/shard-*.log).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
classify() { "$here/../classify-shard-failure.sh" "$1"; }
fixture() { classify "$here/fixtures/shard-$1.log"; }
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

check "infra: Appium found no device" infra "$(fixture no-device)"
check "infra: adb: device offline" infra "$(fixture device-offline)"
check "infra: the UiAutomator2 server crashed" infra "$(fixture uiautomator-crash)"
check "infra: the WebView never appeared" infra "$(fixture webview)"
check "infra: the emulator did not recover" infra "$(fixture no-recovery)"
check "infra: a system app's ANR dialog covered the screen" infra "$(fixture anr)"
check "test: a UI timeout" test "$(fixture ui-timeout)"
check "test: an assertion failed next to a WebView timeout" test "$(fixture assertion-and-webview)"
check "test: an emulator.log line with device offline doesn't count" test "$(fixture boot-offline-then-timeout)"
check "test: an assertion failed after an ANR dialog was closed" test "$(fixture anr-and-assertion)"
check "test: the app's own ANR dialog doesn't count" test "$(fixture app-anr)"

{ cat "$here/fixtures/shard-uiautomator-crash.log"; seq 200000 | sed 's/^/2026-11-03T08:43:09.0000000Z log line /'; } > "$work/big.log"
check "infra: a signature early in a big log" infra "$(classify "$work/big.log")"

finish
