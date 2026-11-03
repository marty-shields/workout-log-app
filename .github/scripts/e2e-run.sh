#!/usr/bin/env bash
# Usage: e2e-run.sh <filter> <tries>
# Runs the E2E tests in the filter on emulator-5554 with the Chromedriver in $CHROMEDRIVER, then re-runs the failed
# scenarios (all rows of a failed Scenario Outline) until they pass or <tries> runs are done. Before each try the
# device must answer (emulator-recover.sh), and before a retry the UiAutomator2 server starts clean, so a dead emulator
# fails the job at once instead of failing every retry. Fails if a scenario fails on its last try; each scenario that
# passed on a retry gets a warning. Writes e2e-<try>.trx to $RESULTS/trx.
# E2E_APP_PACKAGE, E2E_APP_APK and E2E_PREVIOUS_APK, when set, pass the run settings AppPackage, AppApk and PreviousApk
# (docs/e2e-tests.md).
set -euo pipefail
filter="$1"
tries="$2"

source "$(dirname "$0")/e2e-trx.sh"

if [[ -n "${E2E_APP_APK:-}" ]]; then E2E_APP_APK=$(realpath -m -- "$E2E_APP_APK"); fi
if [[ -n "${E2E_PREVIOUS_APK:-}" ]]; then E2E_PREVIOUS_APK=$(realpath -m -- "$E2E_PREVIOUS_APK"); fi

settings=(DeviceUdid=emulator-5554 "ChromedriverExecutable=$CHROMEDRIVER")
if [[ -n "${E2E_APP_PACKAGE:-}" ]]; then settings+=("AppPackage=$E2E_APP_PACKAGE"); fi
if [[ -n "${E2E_APP_APK:-}" ]]; then settings+=("AppApk=$E2E_APP_APK"); fi
if [[ -n "${E2E_PREVIOUS_APK:-}" ]]; then settings+=("PreviousApk=$E2E_PREVIOUS_APK"); fi
parameters=()
for setting in "${settings[@]}"; do
  parameters+=("TestRunParameters.Parameter(name=\"${setting%%=*}\", value=\"${setting#*=}\")")
done

retried=""
for try in $(seq "$tries"); do
  recover=()
  (( try > 1 )) && recover=(reset-server)
  "$(dirname "$0")/emulator-recover.sh" "$RESULTS" "${recover[@]}" || exit 1
  # Failed scenarios become annotations and each run writes a results table to the job summary.
  if dotnet test tests/WorkoutLog.E2E.Tests --no-build --filter "$filter" --logger "console;verbosity=detailed" \
    --logger GitHubActions --logger "trx;LogFileName=e2e-$try.trx" --results-directory "$RESULTS/trx" \
    -- "${parameters[@]}" RunConfiguration.CollectSourceInformation=true; then
    while IFS= read -r name; do
      [[ -n "$name" ]] && echo "::warning title=E2E retry::$name failed, then passed on a retry."
    done <<<"$retried"
    exit 0
  fi

  failed=$(failed_tests "$RESULTS/trx/e2e-$try.trx")
  if [[ -z "$failed" ]]; then
    echo "::error title=E2E::The test run failed with no failed scenario in e2e-$try.trx; not retrying."
    exit 1
  fi
  retried=$(sort -u <<<"$retried"$'\n'"$failed" | grep . || true)
  if (( try < tries )); then
    echo "::notice title=E2E::Try $try: $(grep -c . <<<"$failed") scenario(s) failed; running them again."
  fi
  filter=$(filter_for <<<"$failed")
done
exit 1
