#!/usr/bin/env bash
# Claude Code PreToolUse hook on Bash (.claude/settings.json). Before an agent's `git push` that publishes code, runs the
# checks of dev / build-and-test that need no emulator or browser, and blocks the push (exit 2: Claude Code shows stderr
# to the agent) when one fails. Every other command passes untouched. Reads the hook's JSON on stdin.
# CLAUDE_SKIP_PRE_PUSH=1 (set before starting claude) lets every push through.
# CLAUDE_PRE_PUSH_RUNNER, for the tests: a command called as <runner> <check name> <shell command> in place of each check.
set -uo pipefail

# Splits a shell command into its simple commands on && || ; | ( ) and newlines, one per line. Quoting is ignored, so a
# commit message mentioning git push can match; that only costs a needless run of the checks.
split_commands() {
  local c="$1" sep
  for sep in '&&' '||' ';' '|' '(' ')'; do
    c="${c//"$sep"/$'\n'}"
  done
  printf '%s\n' "$c"
}

# Sets push_dir to the directory a gated push runs in (start: the hook's cwd), or returns 1 when the command holds no
# push that publishes code. Follows `cd <dir>` and `git -C <dir>`.
find_push() {
  local command="$1" dir="$2" line git_opts push_rest
  local cd_re='^cd[[:space:]]+([^[:space:]]+)[[:space:]]*$'
  local push_re='^([A-Za-z_][A-Za-z0-9_]*=[^[:space:]]*[[:space:]]+)*git(([[:space:]]+-[Cc][[:space:]]+[^[:space:]]+)*)[[:space:]]+push([[:space:]].*)?$'
  local c_re='-C[[:space:]]+([^[:space:]]+)'
  while IFS= read -r line; do
    line="${line#"${line%%[![:space:]]*}"}"
    if [[ "$line" =~ $cd_re ]]; then
      dir=$(resolve "$dir" "${BASH_REMATCH[1]}")
    elif [[ "$line" =~ $push_re ]]; then
      git_opts="${BASH_REMATCH[2]}"
      push_rest="${BASH_REMATCH[4]}"
      if [[ "$git_opts" =~ $c_re ]]; then
        dir=$(resolve "$dir" "${BASH_REMATCH[1]}")
      fi
      publishes_code "$push_rest" || continue
      push_dir="$dir"
      return 0
    fi
  done < <(split_commands "$command")
  return 1
}

# <base> <path>: the path, relative to base unless absolute; ~ is the home folder.
resolve() {
  local path="${2/#\~/$HOME}"
  [[ "$path" == /* ]] && printf '%s\n' "$path" || printf '%s\n' "$1/$path"
}

# False for a push that deletes, only tries, or only sends tags: autopilot's clean-up deletes branches this way.
publishes_code() {
  local arg args
  read -ra args <<< "$1"
  for arg in "${args[@]}"; do
    case "$arg" in
      --delete|-d|--dry-run|-n|--tags|:*|+:*|refs/tags/*) return 1 ;;
    esac
  done
  return 0
}

input=$(cat)
[[ "${CLAUDE_SKIP_PRE_PUSH:-}" == 1 ]] && exit 0
command=$(jq -r '.tool_input.command // empty' <<< "$input")
cwd=$(jq -r '.cwd // empty' <<< "$input")
[[ -n "$command" ]] || exit 0
find_push "$command" "${cwd:-$PWD}" || exit 0

root=$(git -C "$push_dir" rev-parse --show-toplevel 2> /dev/null) || exit 0
cd "$root" || exit 0

base=$(git rev-parse --verify --quiet '@{upstream}' || git rev-parse --verify --quiet origin/main || true)
changed=""
[[ -n "$base" ]] && changed=$(git diff --name-only "$base"...HEAD)

work=$(mktemp -d "${TMPDIR:-/tmp}/claude-pre-push.XXXXXX")
coverage="$work/coverage"
test_flags="--no-build --blame-hang-timeout 5m --collect \"Code Coverage;Format=cobertura\""
checks=(
  "filler comments|python3 -I .github/scripts/check-filler-comments.py"
  "docs|python3 -I .github/scripts/check-docs.py"
  "E2E step bindings|python3 -I .github/scripts/e2e-step-check.py tests/WorkoutLog.E2E.Tests"
  "build|dotnet build WorkoutLog.slnx"
  "code style|dotnet format WorkoutLog.slnx --verify-no-changes --no-restore"
  "Core tests|dotnet test tests/WorkoutLog.Core.Tests $test_flags --results-directory '$coverage/core'"
  "UI tests|dotnet test tests/WorkoutLog.UI.Tests $test_flags --results-directory '$coverage/ui'"
  "coverage floor|python3 -I .github/scripts/coverage-summary.py --baseline .github/coverage-baseline.tsv \
WorkoutLog.Core=\"\$(find '$coverage/core' -mindepth 2 -maxdepth 2 -name '*.cobertura.xml')\" \
WorkoutLog.UI=\"\$(find '$coverage/ui' -mindepth 2 -maxdepth 2 -name '*.cobertura.xml')\""
)
if grep -q '^\.github/' <<< "$changed"; then
  checks+=(
    "CI script tests|.github/scripts/tests/run.sh"
    "CI lint|.github/scripts/lint-ci.sh"
  )
fi

for check in "${checks[@]}"; do
  name="${check%%|*}"
  log="$work/${name// /-}.log"
  runner=(bash -c)
  [[ -n "${CLAUDE_PRE_PUSH_RUNNER:-}" ]] && runner=("$CLAUDE_PRE_PUSH_RUNNER" "$name")
  if ! "${runner[@]}" "${check#*|}" > "$log" 2>&1; then
    {
      echo "Push blocked: the $name check failed (full log: $log). Last lines:"
      tail -n 80 "$log"
      echo "Fix it, then push again. The hook reruns the checks before every push."
    } >&2
    exit 2
  fi
done
rm -rf "$work"
exit 0
