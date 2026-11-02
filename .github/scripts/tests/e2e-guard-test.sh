#!/usr/bin/env bash
# e2e-guard.sh's checks, and e2e-local.sh refusing broad filters before it looks for an emulator.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
source "$here/../e2e-guard.sh"
local_run="$here/../e2e-local.sh"

check "broad: an empty filter" "no filter selects every scenario" "$(broad_filter_reason "")"
check "broad: a blank filter" "no filter selects every scenario" "$(broad_filter_reason "  ")"
check "broad: Category=smoke" "Category=smoke is most of the suite" "$(broad_filter_reason "Category=smoke")"
check "broad: Category=smoke in an or" "Category=smoke is most of the suite" "$(broad_filter_reason "Category=US-17|Category=smoke")"
check "broad: one negation" "a filter of only negations selects nearly every scenario" "$(broad_filter_reason "Category!=upgrade")"
check "broad: only negations" "a filter of only negations selects nearly every scenario" \
  "$(broad_filter_reason "Category!=upgrade&FullyQualifiedName!~Backup")"
check "narrow: Category=upgrade" "" "$(broad_filter_reason "Category=upgrade")"
check "narrow: a feature" "" "$(broad_filter_reason "FullyQualifiedName~.EditAWorkoutFeature.")"
check "narrow: a negation beside a match" "" "$(broad_filter_reason "Category!=upgrade&FullyQualifiedName~EditWorkout")"
check "narrow: a category that starts with smoke" "" "$(broad_filter_reason "Category=smoketest")"

check "count_scenarios: the feature tests in a listing" "6" "$(count_scenarios < "$here/fixtures/list-tests.txt")"
check "count_scenarios: an empty listing" "0" "$(count_scenarios < /dev/null)"

# e2e-local.sh with no SDK, so a run the guard lets through skips (2) instead of booting an emulator.
# local_exit <filter> [env...]: the exit code, then "refused" when the refusal was printed.
local_exit() {
  local filter="$1" err status
  shift
  err=$(env ANDROID_HOME=/nonexistent "$@" bash "$local_run" "$filter" 2>&1 > /dev/null)
  status=$?
  echo "$status"
  grep -q '^Refused: .*E2E_ALLOW_BROAD=1' <<<"$(tr '\n' ' ' <<<"$err")" && echo refused
}
check "e2e-local: an empty filter is refused" "64
refused" "$(local_exit "")"
check "e2e-local: Category=smoke is refused" "64
refused" "$(local_exit "Category=smoke")"
check "e2e-local: only negations are refused" "64
refused" "$(local_exit "Category!=upgrade&Category!=backup")"
check "e2e-local: Category=smoke with E2E_ALLOW_BROAD=1 gets past the guard" "2" \
  "$(local_exit "Category=smoke" E2E_ALLOW_BROAD=1)"
check "e2e-local: a narrow filter gets past the guard" "2" "$(local_exit "Category=upgrade")"
check "e2e-local: no filter at all is bad usage" "64" "$(bash "$local_run" > /dev/null 2>&1; echo $?)"

finish
