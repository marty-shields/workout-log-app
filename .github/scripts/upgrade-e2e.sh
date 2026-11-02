#!/usr/bin/env bash
# Usage: upgrade-e2e.sh <previous apk> <tries>
# The upgrade check (docs/e2e-tests.md). The last release's own E2E project, from its v* tag, saves the workouts in
# UpgradeSeed.feature through its own screens on <previous apk> and leaves the app installed. Then this commit's @upgrade
# scenario installs this build over it and checks them, so this commit's steps never drive the last release's screens.
# A failed try starts again from an uninstalled app, up to <tries> tries in all. For a tag older than UpgradeSeed.feature
# another commit's project saves the workouts (upgrade-seed-ref.sh).
# Needs the tags (fetch-depth: 0) and what e2e-run.sh needs: RESULTS, CHROMEDRIVER, and E2E_APP_PACKAGE and E2E_APP_APK
# for a build other than the Debug app. Both APKs must be signed with the same key.
set -euo pipefail
previous=$(realpath -- "$1")
tries="$2"
results=$(realpath -m -- "$RESULTS")
scripts=$(realpath -- "$(dirname "$0")")
cd "$(git rev-parse --show-toplevel)"
package="${E2E_APP_PACKAGE:-uk.martinshields.workoutlog.debug}"
seed_ref=$("$scripts/upgrade-seed-ref.sh")

seed=$(mktemp -d)
trap 'mkdir -p "$results/seed"; cp -r "$seed/tests/WorkoutLog.E2E.Tests/TestResults/E2E/." "$results/seed/" 2> /dev/null || true; rm -rf "$seed"' EXIT
git archive "$seed_ref" | tar -x -C "$seed"
cp global.json "$seed/global.json"
dotnet build "$seed/tests/WorkoutLog.E2E.Tests" -p:TreatWarningsAsErrors=false -p:MSBuildTreatWarningsAsErrors=false
npm ci --ignore-scripts --prefix "$seed/tests/WorkoutLog.E2E.Tests"

for try in $(seq "$tries"); do
  adb uninstall "$package" > /dev/null 2>&1 || true
  if (cd "$seed" && RESULTS="$results/upgrade-$try/seed" E2E_APP_APK="$previous" E2E_PREVIOUS_APK="" "$scripts/e2e-run.sh" "Category=upgrade-seed" 1) \
    && RESULTS="$results/upgrade-$try/verify" E2E_PREVIOUS_APK="$previous" "$scripts/e2e-run.sh" "Category=upgrade" 1; then
    exit 0
  fi
  if (( try < tries )); then
    echo "::notice title=Upgrade::Try $try failed; uninstalling and trying again."
  fi
done
exit 1
