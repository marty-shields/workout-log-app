#!/usr/bin/env bash
# e2e-shard-plan.sh against a canned "dotnet test --list-tests" listing and durations table.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
plan="$here/../e2e-shard-plan.sh"
prefix=FullyQualifiedName~WorkoutLog.E2E.Tests.Features.AFeature

# Long and Long2WithMore share a prefix, so they stay together; the outline rows are summed; Missing counts 15 s.
filter=$(bash "$plan" 1 2 "$here/fixtures/durations.tsv" < "$here/fixtures/list-tests.txt" 2> "$here/.stderr")
check "shard 1 filter: the longest group with its prefix sibling" "$prefix.Long" "$filter"
check "shard 1 summary" "Shard 1/2: 2 scenarios, about 110 s, 0 not in e2e-durations.tsv" "$(cat "$here/.stderr")"

filter=$(bash "$plan" 2 2 "$here/fixtures/durations.tsv" < "$here/fixtures/list-tests.txt" 2> "$here/.stderr")
check "shard 2 filter: outline rows together, a missing scenario, a short one" "$prefix.Outline|$prefix.Missing|$prefix.Short" "$filter"
check "shard 2 summary: counts the scenario missing from the table" "Shard 2/2: 4 scenarios, about 60 s, 1 not in e2e-durations.tsv" "$(cat "$here/.stderr")"

filter=$(bash "$plan" 1 1 "$here/fixtures/durations.tsv" < "$here/fixtures/list-tests.txt" 2> /dev/null)
check "one shard gets every group" "$prefix.Long|$prefix.Outline|$prefix.Missing|$prefix.Short" "$filter"

filter=$(bash "$plan" 5 5 "$here/fixtures/durations.tsv" < "$here/fixtures/list-tests.txt" 2> "$here/.stderr")
check "a shard with nothing to run has an empty filter" "" "$filter"
check "its summary" "Shard 5/5: 0 scenarios, about 0 s, 0 not in e2e-durations.tsv" "$(cat "$here/.stderr")"

rm -f "$here/.stderr"
finish
