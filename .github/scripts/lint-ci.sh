#!/usr/bin/env bash
# Usage: lint-ci.sh [folder]
# Runs shellcheck over the shell scripts in .github/scripts and over the run steps of the composite actions (which neither
# tool reads; action-run-blocks.py extracts them), and actionlint (which also shellchecks each
# workflow's run steps) over .github/workflows. Needs shellcheck on the PATH (apt install shellcheck). Downloads the pinned actionlint into <folder>
# (default: the temp dir), checks its SHA-256 first, unless an earlier run left it there.
set -euo pipefail
version=1.7.12
sha=8aca8db96f1b94770f1b0d72b6dddcb1ebb8123cb3712530b08cc387b349a3d8
folder="${1:-${RUNNER_TEMP:-${TMPDIR:-/tmp}}}"
root="$(cd "$(dirname "$0")/../.." && pwd)"

if ! command -v shellcheck >/dev/null; then
  echo "::error title=Lint::shellcheck is not installed (apt install shellcheck)." >&2
  exit 1
fi

dir="$folder/actionlint-$version"
if [[ ! -x "$dir/actionlint" ]]; then
  mkdir -p "$dir"
  curl -fsSL -o "$dir/actionlint.tar.gz" \
    "https://github.com/rhysd/actionlint/releases/download/v$version/actionlint_${version}_linux_amd64.tar.gz"
  echo "$sha  $dir/actionlint.tar.gz" | sha256sum -c - >&2
  tar -xzf "$dir/actionlint.tar.gz" -C "$dir" actionlint
fi

cd "$root"
shellcheck .github/scripts/*.sh .github/scripts/tests/*.sh
blocks=$(mktemp -d)
trap 'rm -rf "$blocks"' EXIT
python3 -I .github/scripts/action-run-blocks.py "$blocks" .github/actions/*/action.yml
shellcheck -s bash "$blocks"/*.sh
"$dir/actionlint"
