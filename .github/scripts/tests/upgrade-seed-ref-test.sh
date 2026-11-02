#!/usr/bin/env bash
# upgrade-seed-ref.sh in a throwaway repo: which commit's E2E project saves the upgrade check's workouts.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../upgrade-seed-ref.sh"
feature=tests/WorkoutLog.E2E.Tests/Features/UpgradeSeed.feature

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
repo="$work/repo"

git_() { git -C "$repo" "$@"; }
fresh() {
  rm -rf "$repo"
  git init -q -b main "$repo"
  git_ config user.email t@t; git_ config user.name t
  git_ commit -q --allow-empty -m base
}
commit() { git_ commit -q --allow-empty -m "$1"; git_ rev-parse HEAD; }
write_feature() {
  mkdir -p "$repo/$(dirname "$feature")"
  echo "$1" > "$repo/$feature"
  git_ add -A; git_ commit -q -m "$2"; git_ rev-parse HEAD
}
delete_feature() { git_ rm -q "$feature"; git_ commit -q -m "delete"; }

# run <name> <expected stdout and exit>
run() {
  local out
  out=$(cd "$repo" && bash "$script" 2> /dev/null; echo "exit=$?")
  check "$1" "$2" "$out"
}

fresh; git_ tag v1.0.0
added=$(write_feature seed "add the seed")
run "a tag older than the feature takes the commit that added it" "$added
exit=0"

write_feature "seed, changed" "change the seed" > /dev/null
commit later > /dev/null
run "a later change to the feature keeps the commit that added it" "$added
exit=0"

git_ tag v1.1.0
commit next > /dev/null
run "a tag with the feature is its own seed" "v1.1.0
exit=0"

delete_feature; git_ tag v1.2.0
commit next > /dev/null
run "a tag that holds the adding commit but no feature fails" "exit=1"

fresh; git_ tag v1.0.0
commit next > /dev/null
run "no feature anywhere fails" "exit=1"

finish
