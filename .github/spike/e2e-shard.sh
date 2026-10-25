#!/usr/bin/env bash
# SPIKE ONLY. Usage: e2e-shard.sh <shard> <shards>
# Prints the dotnet test filter for one shard: whole features, the largest first, each to the shard with the fewest scenarios.
# Needs the E2E project built.
set -euo pipefail
shard="$1"
shards="$2"

dotnet test tests/WorkoutLog.E2E.Tests --no-build --list-tests -- NUnit.DisplayName=FullName \
  | sed -n 's/^ *\(WorkoutLog\.E2E\.Tests\.Features\.[A-Za-z0-9_]*\)\..*/\1/p' \
  | sort | uniq -c | sort -k1,1nr -k2,2 \
  | awk -v shard="$shard" -v shards="$shards" '
      {
        best = 1
        for (s = 2; s <= shards; s++) if (load[s] < load[best]) best = s
        load[best] += $1
        if (best == shard) { filter = filter (filter ? "|" : "") "FullyQualifiedName~" $2 "."; names = names " " $2 "(" $1 ")" }
      }
      END {
        print "Shard " shard "/" shards ": " load[shard] + 0 " scenarios:" names > "/dev/stderr"
        print filter
      }'
