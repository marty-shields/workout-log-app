#!/usr/bin/env bash
# Usage: check-backup.sh <apk> <true|false>
# Fails unless the APK's manifest sets android:allowBackup to the expected value.
set -euo pipefail
apk="$1"; expected="$2"
tools=$(ls -d "$ANDROID_HOME"/build-tools/* | sort -V | tail -n 1)

fail() { echo "::error title=Backup check::$1"; exit 1; }

# aapt2 prints the attribute as "A: http://schemas.android.com/apk/res/android:allowBackup(0x01010280)=false".
actual=$("$tools/aapt2" dump xmltree --file AndroidManifest.xml "$apk" | sed -n 's/^.*:allowBackup([^)]*)=\([a-z]*\).*$/\1/p')
[[ "$actual" == "$expected" ]] || fail "$(basename "$apk") has allowBackup=${actual:-unset}; expected $expected."
echo "Checked: $(basename "$apk") allowBackup=$actual"
