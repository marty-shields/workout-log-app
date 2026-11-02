#!/usr/bin/env bash
# failed_tests and filter_for from e2e-trx.sh, against a sample trx file.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
source "$here/../e2e-trx.sh"
class=WorkoutLog.E2E.Tests.Features.AFeature

check "failed_tests: the failed tests only, unescaped" "$class.Lt<T>'s
$class.Outline(1,\"a & b\")
$class.Outline(2,\"c\")
$class.Plain" "$(failed_tests "$here/fixtures/sample.trx" | sort)"

check "filter_for: Class.Method for an outline, the exact name otherwise, no parenthesis" "FullyQualifiedName=$class.Lt<T>'s|FullyQualifiedName=$class.Plain|FullyQualifiedName~$class.Outline" "$(failed_tests "$here/fixtures/sample.trx" | filter_for)"

check "filter_for: an outline's rows make one entry" "FullyQualifiedName~$class.Outline" "$(printf '%s\n' "$class.Outline(1)" "$class.Outline(2)" | filter_for)"

check "failed_tests: a trx with no failure prints nothing" "" "$(grep -v 'outcome="Failed"' "$here/fixtures/sample.trx" | failed_tests /dev/stdin)"

finish
