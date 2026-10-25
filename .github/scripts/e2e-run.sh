#!/usr/bin/env bash
# Usage: e2e-run.sh <filter> <tries>
# Runs the E2E tests in the filter on emulator-5554 with the Chromedriver in $CHROMEDRIVER, then re-runs only the
# failed scenarios until they pass or <tries> runs are done. Fails if a scenario fails on its last try; each scenario
# that passed on a retry gets a warning. Writes e2e-<try>.trx to $RESULTS/trx.
set -euo pipefail
filter="$1"
tries="$2"

# The full name of each failed test in a trx file, one per line.
failed_tests() {
  awk 'BEGIN { RS = "<" }
    function attr(name,   s, i) {
      if (!match($0, " " name "=\"[^\"]*\"")) return ""
      s = substr($0, RSTART + length(name) + 3, RLENGTH - length(name) - 4)
      gsub(/&quot;/, "\"", s); gsub(/&apos;/, "\047", s); gsub(/&lt;/, "<", s); gsub(/&gt;/, ">", s)
      while ((i = index(s, "&amp;")) > 0) s = substr(s, 1, i) substr(s, i + 5)
      return s
    }
    /^UnitTestResult / && attr("outcome") == "Failed" { failed[attr("testId")] = 1 }
    /^UnitTest / { id = attr("id") }
    /^TestMethod / { full[id] = attr("className") "." attr("name") }
    END { for (id in failed) print full[id] }' "$1"
}

# A filter matching exactly these full names: filter operators escaped with \, and the characters MSBuild splits on
# percent-encoded.
filter_for() {
  sed 's/[\\()&|=!~]/\\&/g; s/%/%25/g; s/,/%2C/g; s/;/%3B/g; s/"/%22/g; s/^/FullyQualifiedName=/' | paste -sd '|'
}

retried=""
for try in $(seq "$tries"); do
  # Failed scenarios become annotations and each run writes a results table to the job summary (T95).
  if dotnet test tests/WorkoutLog.E2E.Tests --no-build --filter "$filter" --logger "console;verbosity=detailed" \
    --logger GitHubActions --logger "trx;LogFileName=e2e-$try.trx" --results-directory "$RESULTS/trx" \
    -- 'TestRunParameters.Parameter(name="DeviceUdid", value="emulator-5554")' \
    "TestRunParameters.Parameter(name=\"ChromedriverExecutable\", value=\"$CHROMEDRIVER\")" \
    RunConfiguration.CollectSourceInformation=true; then
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
  if [[ -z "$retried" ]]; then
    retried="$failed"
  fi
  if (( try < tries )); then
    echo "::notice title=E2E::Try $try: $(grep -c . <<<"$failed") scenario(s) failed; running them again."
  fi
  filter=$(filter_for <<<"$failed")
done
exit 1
