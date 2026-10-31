#!/usr/bin/env bash
# Usage: e2e-durations.sh <trx file>...
# Prints the shard duration table (.github/e2e-durations.tsv) from E2E trx files: the median duration in seconds of each
# scenario's passing runs, then its full name, tab-separated, sorted by name. e2e-shard.sh balances the shards with it.
set -euo pipefail

echo '# Median seconds of a passing run per scenario, then its full name. Made by e2e-durations.sh (docs/dev-setup-build.md).'

# Each passing scenario in a trx file as "<full name><TAB><seconds>".
passed() {
  awk 'BEGIN { RS = "<"; FS = "\t" }
    function attr(name,   s, i) {
      if (!match($0, " " name "=\"[^\"]*\"")) return ""
      s = substr($0, RSTART + length(name) + 3, RLENGTH - length(name) - 4)
      gsub(/&quot;/, "\"", s); gsub(/&apos;/, "\047", s); gsub(/&lt;/, "<", s); gsub(/&gt;/, ">", s)
      while ((i = index(s, "&amp;")) > 0) s = substr(s, 1, i) substr(s, i + 5)
      return s
    }
    /^UnitTestResult / && attr("outcome") == "Passed" {
      split(attr("duration"), t, ":")
      seconds[attr("testId")] = t[1] * 3600 + t[2] * 60 + t[3]
    }
    /^UnitTest / { id = attr("id") }
    /^TestMethod / { full[id] = attr("className") "." attr("name") }
    END { for (id in seconds) printf "%s\t%.3f\n", full[id], seconds[id] }' "$1"
}

for trx in "$@"; do passed "$trx"; done | LC_ALL=C sort -t $'\t' -k1,1 -k2,2n \
  | awk -F '\t' '
      function flush(   mid) {
        if (n == 0) return
        mid = int((n + 1) / 2)
        median = (n % 2) ? v[mid] : (v[mid] + v[mid + 1]) / 2
        printf "%d\t%s\n", (median < 1 ? 1 : median + 0.5), name
      }
      $1 != name { flush(); name = $1; n = 0 }
      { v[++n] = $2 }
      END { flush() }'
