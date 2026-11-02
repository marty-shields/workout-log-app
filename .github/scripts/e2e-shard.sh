#!/usr/bin/env bash
# Usage: e2e-shard.sh <shard> <shards>
# Prints the dotnet test filter for one shard: scenarios balanced by time, the longest first, each to the shard with the
# least time so far. The rows of a Scenario Outline stay together, as do scenarios whose names start with another
# scenario's name (the filter matches by name prefix). A scenario's time is in .github/e2e-durations.tsv
# (e2e-durations.sh, per method); a row of a method missing from it counts as 15 s. Needs the E2E project built.
set -euo pipefail
shard="$1"
shards="$2"

dotnet test tests/WorkoutLog.E2E.Tests --no-build --list-tests -- NUnit.DisplayName=FullName \
  | "$(dirname "$0")/e2e-shard-plan.sh" "$shard" "$shards" "$(dirname "$0")/../e2e-durations.tsv"
