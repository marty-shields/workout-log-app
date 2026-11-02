#!/usr/bin/env bash
# Usage: check-backup.sh <apk> <true|false> <backup_rules|no_backup>
# Fails unless the APK's manifest sets android:allowBackup to the expected value and android:dataExtractionRules to the
# expected rules file, and that file says what it should: backup_rules includes only the database, no_backup includes nothing.
set -euo pipefail
apk="$1"; expected="$2"; rules="$3"
tools=$(ls -d "$ANDROID_HOME"/build-tools/* | sort -V | tail -n 1)
name=$(basename "$apk")

fail() { echo "::error title=Backup check::$1"; exit 1; }

manifest=$("$tools/aapt2" dump xmltree --file AndroidManifest.xml "$apk")

# aapt2 prints the attribute as "A: http://schemas.android.com/apk/res/android:allowBackup(0x01010280)=false".
actual=$(sed -n 's/^.*:allowBackup([^)]*)=\([a-z]*\).*$/\1/p' <<<"$manifest")
[[ "$actual" == "$expected" ]] || fail "$name has allowBackup=${actual:-unset}; expected $expected."

# The rules attribute is a resource id ("=@0x7f120003"); the resource table maps it to a name and a file.
id=$(sed -n 's/^.*:dataExtractionRules([^)]*)=@\(0x[0-9a-f]*\).*$/\1/p' <<<"$manifest")
[[ -n "$id" ]] || fail "$name has no dataExtractionRules; expected xml/$rules."
resources=$("$tools/aapt2" dump resources "$apk")
actual_rules=$(sed -n "s|^ *resource $id xml/\(.*\)\$|\1|p" <<<"$resources")
[[ "$actual_rules" == "$rules" ]] || fail "$name has dataExtractionRules=xml/${actual_rules:-unknown}; expected xml/$rules."
file=$(grep -A1 "resource $id xml/" <<<"$resources" | sed -n 's|^.*(file) \(res/[^ ]*\) type=XML.*$|\1|p')
[[ -n "$file" ]] || fail "$name has no file for xml/$rules."

# Each section of the rules lists its includes as "domain" and "path" attribute pairs.
tree=$("$tools/aapt2" dump xmltree --file "$file" "$apk")
includes=$(awk '
  /E: (cloud-backup|device-transfer)/ { section = $2 }
  /E: include/ { in_include = 1; next }
  /E: / { in_include = 0 }
  in_include && /A: domain=/ { match($0, /Raw: "[^"]*"/); domain = substr($0, RSTART + 6, RLENGTH - 7) }
  in_include && /A: path=/ { match($0, /Raw: "[^"]*"/); print section ": " domain "/" substr($0, RSTART + 6, RLENGTH - 7) }
' <<<"$tree")
case "$rules" in
  backup_rules) want=$'cloud-backup: file/workoutlog.db3\ndevice-transfer: file/workoutlog.db3' ;;
  no_backup) want="" ;;
  *) fail "Unknown rules file $rules." ;;
esac
[[ "$includes" == "$want" ]] || fail "$name's xml/$rules includes:"$'\n'"${includes:-nothing}"$'\n'"expected:"$'\n'"${want:-nothing}"
if [[ "$rules" == no_backup ]]; then
  [[ $(grep -c 'E: exclude' <<<"$tree") -ge 2 ]] || fail "$name's xml/no_backup excludes nothing."
fi
echo "Checked: $name allowBackup=$actual dataExtractionRules=xml/$rules"
