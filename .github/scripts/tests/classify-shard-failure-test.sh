#!/usr/bin/env bash
# classify-shard-failure.sh against trimmed logs of real failed shards (fixtures/shard-*.log).
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
classify() { "$here/../classify-shard-failure.sh" "$here/fixtures/shard-$1.log"; }

check "infra: Appium found no device" infra "$(classify no-device)"
check "infra: adb: device offline" infra "$(classify device-offline)"
check "infra: the UiAutomator2 server crashed" infra "$(classify uiautomator-crash)"
check "infra: the WebView never appeared" infra "$(classify webview)"
check "infra: the emulator did not recover" infra "$(classify no-recovery)"
check "test: a UI timeout" test "$(classify ui-timeout)"
check "test: an assertion failed next to a WebView timeout" test "$(classify assertion-and-webview)"
check "test: an emulator.log line with device offline doesn't count" test "$(classify boot-offline-then-timeout)"

finish
