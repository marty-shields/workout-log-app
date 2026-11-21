#!/usr/bin/env bash
# Runs the Node tests of screenshot-diff.mjs, the pixel comparison behind readme-screenshots.mjs --check.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
if ! command -v node >/dev/null; then
  echo "node is not installed" >&2
  exit 1
fi
node --test "$here/screenshot-diff.test.mjs"
