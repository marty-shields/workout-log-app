#!/usr/bin/env bash
# trx-summary.py against small trx reports in a temp folder.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../trx-summary.py"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir "$work/core" "$work/ui" "$work/empty" "$work/bad"
cat > "$work/core/results.trx" <<'TRX'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="1" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Passes" outcome="Passed" />
    <UnitTestResult testName="Skips" outcome="NotExecuted" />
    <UnitTestResult testName="Breaks(1,&quot;a&quot;)" outcome="Failed">
      <Output><ErrorInfo>
        <Message>Expected: 1
But was: 2, 100%</Message>
      </ErrorInfo></Output>
    </UnitTestResult>
    <UnitTestResult testName="NoFrame" outcome="Failed" />
  </Results>
</TestRun>
TRX
printf '<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results><UnitTestResult testName="Ok" outcome="Passed" /></Results></TestRun>\n' > "$work/ui/results.trx"
printf 'not xml' > "$work/bad/results.trx"

check "the table counts each outcome, then lists the failures" '| Tests | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Core | 1 | 2 | 1 |
| UI | 1 | 0 | 0 |

**Core: Breaks(1,"a")**

```
Expected: 1
But was: 2, 100%
```

**Core: NoFrame**

```
Failed
```
exit=0' "$(python3 -I "$script" Core="$work/core" UI="$work/ui"; echo "exit=$?")"

check "annotations escape newlines, colons, commas and percents" '::error title=Core%3A Breaks(1%2C"a")::Expected%3A 1%0ABut was%3A 2%2C 100%25
::error title=Core%3A NoFrame::Failed
exit=0' "$(python3 -I "$script" --annotations Core="$work/core"; echo "exit=$?")"

check "a run with no failures prints no annotations" "exit=0" "$(python3 -I "$script" --annotations UI="$work/ui"; echo "exit=$?")"
check "a single trx file works as well as a folder" "::error title=Core%3A NoFrame::Failed" "$(python3 -I "$script" --annotations Core="$work/core/results.trx" | tail -1)"
check "a folder with no trx fails and says so" "Core has no trx report in $work/empty
exit=1" "$(python3 -I "$script" Core="$work/empty" 2>&1; echo "exit=$?")"
check "a missing folder fails" "exit=1" "$(python3 -I "$script" Core="$work/none" > /dev/null 2>&1; echo "exit=$?")"
check "an unreadable report fails" "exit=1" "$(python3 -I "$script" Core="$work/bad" > /dev/null 2>&1; echo "exit=$?")"

finish
