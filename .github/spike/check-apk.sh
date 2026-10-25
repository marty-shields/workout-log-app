#!/usr/bin/env bash
# SPIKE ONLY. Usage: check-apk.sh <apk> <version> <code> <cert SHA-256, keytool format> <out dir>
# Fails unless the APK is signed by the pinned cert, has the version and code, isn't debuggable and holds arm64 only.
# Writes WorkoutLog-<version>.apk, its .sha256 and notes.md to the out dir.
set -euo pipefail
apk="$1"; version="$2"; code="$3"; cert="$4"; out="$5"
tools=$(ls -d "$ANDROID_HOME"/build-tools/* | sort -V | tail -n 1)

fail() { echo "::error title=APK check::$1"; exit 1; }

signers=$("$tools/apksigner" verify --print-certs "$apk")
digests=$(sed -n 's/^Signer #[0-9]* certificate SHA-256 digest: //p' <<<"$signers")
[[ $(grep -c . <<<"$digests") -eq 1 ]] || fail "The APK must have exactly one signer."
pinned=$(tr -d ':' <<<"$cert" | tr 'A-F' 'a-f')
[[ -n "$pinned" && "$digests" == "$pinned" ]] || fail "The APK's signing cert ($digests) isn't the pinned release cert. Check the release environment's keystore secret."

badging=$("$tools/aapt2" dump badging "$apk")
grep -q "^package: name='uk.martinshields.workoutlog' versionCode='$code' versionName='$version'" <<<"$badging" \
  || fail "Package, version code or version doesn't match: $(head -n 1 <<<"$badging")"
! grep -q "application-debuggable" <<<"$badging" || fail "The APK is debuggable."

abis=$(unzip -Z1 "$apk" | sed -n 's|^lib/\([^/]*\)/.*|\1|p' | sort -u | paste -sd ' ')
[[ "$abis" == "arm64-v8a" ]] || fail "The APK must hold arm64-v8a only; it has: $abis"

mkdir -p "$out"
cp "$apk" "$out/WorkoutLog-$version.apk"
(cd "$out" && sha256sum "WorkoutLog-$version.apk" > "WorkoutLog-$version.apk.sha256")
fingerprint=$(sed 's/../&:/g; s/:$//' <<<"$digests" | tr 'a-f' 'A-F')
printf 'Signing certificate SHA-256: `%s`\n\nCheck an APK before installing: `apksigner verify --print-certs <apk>` must show this digest ([08](https://github.com/%s/blob/main/docs/08-dev-setup-build.md)).\n' \
  "$fingerprint" "$GITHUB_REPOSITORY" > "$out/notes.md"
echo "Checked: $(head -n 1 <<<"$badging"); ABIs $abis; cert $fingerprint"
