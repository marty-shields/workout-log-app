# shellcheck shell=bash
# Refuses local E2E runs too broad for one emulator. Sourced by e2e-local.sh; E2E_ALLOW_BROAD=1 skips the checks.

# Why a dotnet test filter is too broad, or nothing.
broad_filter_reason() {
  local filter="$1" positive
  local smoke='(^|[|&(])[[:space:]]*Category[[:space:]]*=[[:space:]]*smoke[[:space:]]*($|[|&)])'
  positive="${filter//!=/}"
  positive="${positive//!~/}"
  if [[ -z "${filter//[[:space:]]/}" ]]; then
    echo "no filter selects every scenario"
  elif [[ "$filter" =~ $smoke ]]; then
    echo "Category=smoke is most of the suite"
  elif [[ "$positive" != *[=~]* ]]; then
    echo "a filter of only negations selects nearly every scenario"
  fi
}

# The number of scenarios in a "dotnet test --list-tests -- NUnit.DisplayName=FullName" listing on stdin.
count_scenarios() {
  awk '{ sub(/\r$/, "") } /^ *WorkoutLog\.E2E\.Tests\.Features\./ { n++ } END { print n + 0 }'
}

# Prints the refusal for <reason> and exits 64.
refuse_broad() {
  cat >&2 << EOF
Refused: $1. CI's 8 shards run the full suite after a push; one local emulator would take an hour.
Run what you changed, e.g. "Category=upgrade" or "FullyQualifiedName~.EditAWorkoutFeature.".
To run it anyway, set E2E_ALLOW_BROAD=1.
EOF
  exit 64
}
