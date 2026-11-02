# Test helpers, sourced by the *-test.sh scripts.
failures=0

# check <name> <expected text> <actual text>
check() {
  if diff <(printf '%s\n' "$2") <(printf '%s\n' "$3") > /dev/null; then
    echo "ok   $1"
  else
    echo "FAIL $1"
    diff <(printf '%s\n' "$2") <(printf '%s\n' "$3") | sed 's/^/     /' || true
    failures=$((failures + 1))
  fi
}

finish() {
  (( failures == 0 )) || { echo "$failures failed"; exit 1; }
}
