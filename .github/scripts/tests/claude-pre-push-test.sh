#!/usr/bin/env bash
# claude-pre-push.sh on sample hook input: which commands it gates, which checks it runs, and how it blocks a push.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../claude-pre-push.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# A stub for every check: records its name, and fails the one named in FAIL_AT.
cat > "$work/runner" <<'S'
#!/usr/bin/env bash
echo "$1" >> "$RAN"
[[ "$1" != "${FAIL_AT:-}" ]]
S
chmod +x "$work/runner"
export CLAUDE_PRE_PUSH_RUNNER="$work/runner" RAN="$work/ran" TMPDIR="$work"

repo="$work/repo"
git init -q -b main "$repo"
git -C "$repo" -c user.name=t -c user.email=t@t commit -q --allow-empty -m base
git -C "$repo" update-ref refs/remotes/origin/main HEAD
mkdir "$repo/sub"

# hook <command> [cwd]: prints the exit code, then the checks that ran.
hook() {
  rm -f "$RAN"
  jq -n --arg c "$1" --arg d "${2:-$repo}" '{tool_name: "Bash", tool_input: {command: $c}, cwd: $d}' \
    | bash "$script" 2> "$work/stderr"
  echo "exit=$?"
  [[ -f "$RAN" ]] && paste -sd, "$RAN"
}

all="filler comments,docs,E2E step bindings,build,code style,Core tests,UI tests,coverage floor"

check "not git" "exit=0" "$(hook 'ls -la')"
check "other git command" "exit=0" "$(hook 'git status')"
check "push inside echo" "exit=0" "$(hook 'echo git push')"
check "plain push" "$(printf 'exit=0\n%s' "$all")" "$(hook 'git push')"
check "push with upstream" "$(printf 'exit=0\n%s' "$all")" "$(hook 'git push -u origin feat/x')"
check "push after cd" "$(printf 'exit=0\n%s' "$all")" "$(hook "cd $repo && git push" "$work")"
check "push with -C" "$(printf 'exit=0\n%s' "$all")" "$(hook 'git -C repo push' "$work")"
check "env before push" "$(printf 'exit=0\n%s' "$all")" "$(hook 'GIT_TRACE=0 git push')"
check "push outside a repo" "exit=0" "$(hook 'git push' "$work")"
check "delete" "exit=0" "$(hook 'git push origin --delete feat/x')"
check "delete short" "exit=0" "$(hook 'git push -d origin feat/x')"
check "delete refspec" "exit=0" "$(hook 'git push origin :feat/x')"
check "tags" "exit=0" "$(hook 'git push --tags')"
check "dry run" "exit=0" "$(hook 'git push --dry-run')"

check "failing check blocks" "$(printf 'exit=2\nfiller comments,docs,E2E step bindings,build')" \
  "$(FAIL_AT=build hook 'git push')"
check "block message names the check" "1" "$(grep -c '^Push blocked: the build check failed' "$work/stderr")"

git -C "$repo" -c user.name=t -c user.email=t@t commit -q --allow-empty -m code
check "no CI checks without a .github change" "$(printf 'exit=0\n%s' "$all")" "$(hook 'git push')"
mkdir -p "$repo/.github/scripts"
touch "$repo/.github/scripts/x.sh"
git -C "$repo" add .github
git -C "$repo" -c user.name=t -c user.email=t@t commit -q -m ci
check "CI checks after a .github change" "$(printf 'exit=0\n%s,CI script tests,CI lint' "$all")" "$(hook 'git push')"

finish
