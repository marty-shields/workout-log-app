#!/usr/bin/env bash
# Usage: upgrade-seed-ref.sh
# Prints the commit whose E2E project saves the workouts for the upgrade check (upgrade-e2e.sh): the latest v* tag before
# HEAD. A tag older than UpgradeSeed.feature has no seed scenario, so for one the commit that added the feature does it
# instead: that commit's CI ran it against that release's build, and it never changes. Fails when a tag that holds that
# commit has no seed scenario. Needs the tags and the history (fetch-depth: 0).
set -euo pipefail
feature=tests/WorkoutLog.E2E.Tests/Features/UpgradeSeed.feature

tag=$(git describe --tags --abbrev=0 --match 'v[0-9]*' HEAD)
if git cat-file -e "$tag:$feature" 2> /dev/null; then
  echo "$tag"
  exit 0
fi

added=$(git log --diff-filter=A --format=%H HEAD -- "$feature" | tail -n 1)
if [[ -z "$added" ]] || git merge-base --is-ancestor "$added" "$tag"; then
  echo "::error title=Upgrade::$tag has no $feature to save the workouts an upgrade must keep." >&2
  exit 1
fi
echo "::notice title=Upgrade::$tag predates $feature: the E2E project of $(git rev-parse --short "$added"), the commit that added it, saves the workouts." >&2
echo "$added"
