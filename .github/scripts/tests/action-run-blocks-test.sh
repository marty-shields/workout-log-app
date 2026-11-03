#!/usr/bin/env bash
# action-run-blocks.py on sample composite actions, and the shellcheck lint it feeds: an unquoted variable in a run block fails.
# shellcheck disable=SC2016 # the samples are literal action text, not expansions
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../action-run-blocks.py"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir "$work/out" "$work/sample" "$work/other"

lint() {
  rm -f "$work"/out/*
  python3 -I "$script" "$work/out" "$1" > /dev/null 2>&1 || { echo "exit=extract"; return; }
  shellcheck -s bash "$work"/out/*.sh > /dev/null 2>&1
  echo "exit=$?"
}

cat > "$work/sample/action.yml" <<'Y'
inputs:
  run:
    description: Not a step.
runs:
  using: composite
  steps:
    - name: Clean
      shell: bash
      env:
        RESULTS: ${{ inputs.results }}
      run: |
        mkdir -p "$RESULTS"
        echo "${{ inputs.name }}"
    - run: echo "$HOME"
Y
check "a clean action passes" "exit=0" "$(lint "$work/sample/action.yml")"

check "a block is named for its line in the action" "sample-12.sh sample-14.sh" "$(cd "$work/out" && echo *)"
check "a block keeps its line number and \${{ }} becomes a placeholder" "$(printf 'mkdir -p "$RESULTS"\necho "__EXPR__"')" "$(sed -n 12,13p "$work/out/sample-12.sh")"

sed 's|mkdir -p "$RESULTS"|mkdir -p $RESULTS|' "$work/sample/action.yml" > "$work/other/action.yml"
check "an unquoted \$RESULTS fails" "exit=1" "$(lint "$work/other/action.yml")"

printf 'run: |\n  echo hi\nother: x\n' > "$work/other/action.yml"
check "a block ends at the next key" "echo hi" "$(lint "$work/other/action.yml" > /dev/null; sed -n 2p "$work/out/other-2.sh")"

printf 'name: no steps\n' > "$work/other/action.yml"
check "an action with no run block fails" "exit=extract" "$(lint "$work/other/action.yml")"

finish
