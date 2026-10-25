#!/usr/bin/env bash
# SPIKE ONLY. Usage: next-version.sh <tag prefix> <target sha>. Needs GH_TOKEN and GITHUB_REPOSITORY.
# Writes release, version and code to $GITHUB_OUTPUT (or stdout).
set -euo pipefail
prefix="$1"
target="$2"
out="${GITHUB_OUTPUT:-/dev/stdout}"

tags=$(git tag --list "${prefix}[0-9]*.[0-9]*.[0-9]*")
code=$(( $(grep -c . <<<"$tags" || true) + 1 ))
last=$(git tag --list "${prefix}[0-9]*.[0-9]*.[0-9]*" --merged "$target" --sort=-v:refname | head -n 1)

if [[ -z "$last" ]]; then
  version=1.0.0
  echo "No ${prefix} tag yet: first release."
else
  bump=0
  for sha in $(git rev-list "$last..$target"); do
    prs=$(gh api "repos/$GITHUB_REPOSITORY/commits/$sha/pulls" --jq '[.[] | select(.merged_at != null and .base.ref == "main")]')
    if [[ $(jq length <<<"$prs") -ne 1 ]]; then
      echo "::error title=Version::Commit $sha isn't the merge of exactly one PR into main."
      exit 1
    fi
    number=$(jq -r '.[0].number' <<<"$prs")
    labels=$(jq -r '[.[0].labels[].name | select(test("^semver:(major|minor|patch)$"))] | join(" ")' <<<"$prs")
    case "$labels" in
      semver:major) level=3 ;;
      semver:minor) level=2 ;;
      semver:patch) level=1 ;;
      *)
        echo "::error title=Version::PR #$number needs exactly one semver label; it has: ${labels:-none}. Fix the labels and run the release again."
        exit 1
        ;;
    esac
    echo "PR #$number: $labels"
    if (( level > bump )); then bump=$level; fi
  done

  if (( bump == 0 )); then
    echo "::notice title=Version::No PRs merged since $last: nothing to release."
    echo "release=false" >> "$out"
    exit 0
  fi

  IFS=. read -r major minor patch <<<"${last#"$prefix"}"
  case $bump in
    3) version="$((major + 1)).0.0" ;;
    2) version="$major.$((minor + 1)).0" ;;
    1) version="$major.$minor.$((patch + 1))" ;;
  esac
  echo "Last release $last."
fi

echo "Next release ${prefix}${version}, version code $code."
{
  echo "release=true"
  echo "version=$version"
  echo "code=$code"
} >> "$out"
