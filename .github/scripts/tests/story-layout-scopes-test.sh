#!/usr/bin/env bash
# Runs the Node tests of story-layout-scopes.mjs, the scope and measure rules behind check-story-layout.mjs.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
if ! command -v node >/dev/null; then
  echo "node is not installed" >&2
  exit 1
fi
node --test "$here/story-layout-scopes.test.mjs"
