#!/usr/bin/env bash
# Usage: e2e-shard.sh <shard> <shards>
# Prints the dotnet test filter for one shard: scenarios balanced by time, the longest first, each to the shard with the
# least time so far. The rows of a Scenario Outline stay together, as do scenarios whose names start with another
# scenario's name (the filter matches by name prefix). A scenario's time is in .github/e2e-durations.tsv
# (e2e-durations.sh); one missing from it counts as 15 s. Needs the E2E project built.
set -euo pipefail
shard="$1"
shards="$2"
durations="$(dirname "$0")/../e2e-durations.tsv"

dotnet test tests/WorkoutLog.E2E.Tests --no-build --list-tests -- NUnit.DisplayName=FullName \
  | sed -n 's/\r$//; s/^ *\(WorkoutLog\.E2E\.Tests\.Features\..*\)$/\1/p' \
  | awk -F '\t' -v default_seconds=15 '
      NR == FNR { if ($1 ~ /^[0-9]+$/) seconds[$2] = $1; next }
      {
        name = $0; sub(/[(].*/, "", name)
        group[name] = 1
        if ($0 in seconds) time[name] += seconds[$0]; else { time[name] += default_seconds; missing[name]++ }
        count[name]++
      }
      END {
        for (g in group) {
          root = g
          for (h in group) if (index(g, h) == 1 && length(h) < length(root)) root = h
          unit_time[root] += time[g]; unit_count[root] += count[g]; unit_missing[root] += missing[g]
        }
        for (u in unit_time) print unit_time[u] "\t" u "\t" unit_count[u] "\t" unit_missing[u]
      }' "$durations" - \
  | LC_ALL=C sort -t $'\t' -k1,1nr -k2,2 \
  | awk -F '\t' -v shard="$shard" -v shards="$shards" '
      {
        best = 1
        for (s = 2; s <= shards; s++) if (load[s] < load[best]) best = s
        load[best] += $1
        if (best == shard) { filter = filter (filter ? "|" : "") "FullyQualifiedName~" $2; scenarios += $3; missing += $4 }
      }
      END {
        print "Shard " shard "/" shards ": " scenarios + 0 " scenarios, about " load[shard] + 0 " s, " missing + 0 " not in e2e-durations.tsv" > "/dev/stderr"
        print filter
      }'
