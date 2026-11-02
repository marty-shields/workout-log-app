#!/usr/bin/env bash
# Usage: .github/scripts/tests/run.sh
# Runs the tests of the CI scripts (needs bash, git, jq, awk).
set -uo pipefail
cd "$(dirname "$0")" || exit
status=0
for test in ./*-test.sh; do
  echo "== $test"
  bash "$test" || status=1
done
exit $status
