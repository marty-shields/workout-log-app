#!/usr/bin/env bash
# coverage-summary.py against small Cobertura reports and baselines in a temp folder.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../coverage-summary.py"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# report <file> <line-rate>
report() {
  printf '<coverage><packages><package name="WorkoutLog.Core" line-rate="%s" branch-rate="0.5"/><package name="Other" line-rate="0.1" branch-rate="0.1"/></packages></coverage>\n' "$2" > "$work/$1"
}

# baseline <line %>
baseline() { printf '# comment\nWorkoutLog.Core\t%s\n' "$1" > "$work/baseline.tsv"; }

# run <name> <report rate> <baseline %> <expected exit and output>
run() {
  report core.xml "$2"
  baseline "$3"
  check "$1" "$4" "$(python3 -I "$script" --baseline "$work/baseline.tsv" WorkoutLog.Core="$work/core.xml" 2>/dev/null; echo "exit=$?")"
}

run "at the baseline" 0.9 90.0 '| Assembly | Lines | Branches | Baseline lines |
| --- | ---: | ---: | ---: |
| WorkoutLog.Core | 90.0% | 50.0% | 90.0% |
exit=0'

run "a drop of exactly 1 point passes" 0.89 90.0 '| Assembly | Lines | Branches | Baseline lines |
| --- | ---: | ---: | ---: |
| WorkoutLog.Core | 89.0% | 50.0% | 90.0% |
exit=0'

run "a drop of more than 1 point fails" 0.888 90.0 '| Assembly | Lines | Branches | Baseline lines |
| --- | ---: | ---: | ---: |
| WorkoutLog.Core | 88.8% | 50.0% | 90.0% |

**WorkoutLog.Core line coverage 88.8% is more than 1 point below its baseline 90.0%.**
exit=1'

run "a rise passes" 0.95 90.0 '| Assembly | Lines | Branches | Baseline lines |
| --- | ---: | ---: | ---: |
| WorkoutLog.Core | 95.0% | 50.0% | 90.0% |
exit=0'

report core.xml 0.9
printf '# comment\n' > "$work/baseline.tsv"
check "an assembly missing from the baseline fails" '| Assembly | Lines | Branches | Baseline lines |
| --- | ---: | ---: | ---: |
| WorkoutLog.Core | 90.0% | 50.0% | none |

**WorkoutLog.Core has no row in the coverage baseline.**
exit=1' "$(python3 -I "$script" --baseline "$work/baseline.tsv" WorkoutLog.Core="$work/core.xml" 2>/dev/null; echo "exit=$?")"

for bad in NaN "" abc; do
  report core.xml "$bad"
  baseline 90.0
  check "a line-rate of '$bad' fails" "exit=1" "$(python3 -I "$script" --baseline "$work/baseline.tsv" WorkoutLog.Core="$work/core.xml" 2>/dev/null; echo "exit=$?")"
done
report core.xml NaN
check "a NaN line-rate says why" "WorkoutLog.Core has a line-rate of NaN in $work/core.xml, not a number" "$(python3 -I "$script" --baseline "$work/baseline.tsv" WorkoutLog.Core="$work/core.xml" 2>&1)"
report core.xml 0.9

check "no option prints the table without a gate" '| Assembly | Lines | Branches |
| --- | ---: | ---: |
| WorkoutLog.Core | 90.0% | 50.0% |
exit=0' "$(python3 -I "$script" WorkoutLog.Core="$work/core.xml"; echo "exit=$?")"

check "--tsv prints the baseline file" '# Line coverage % per assembly, a gate for dev / build-and-test. Made by coverage-summary.py --tsv (docs/dev-setup-build.md).
WorkoutLog.Core	90.0
exit=0' "$(python3 -I "$script" --tsv WorkoutLog.Core="$work/core.xml"; echo "exit=$?")"

finish
