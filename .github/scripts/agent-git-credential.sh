#!/usr/bin/env bash
# Usage: git -c 'credential.https://github.com.helper=!<folder>/agent-git-credential.sh' ...
# A git credential helper: on get for github.com it answers with a token of the marty-dev-bot GitHub App, from the
# agent-token.sh next to it. It ignores other hosts and the store and erase operations.
set -euo pipefail
[[ "${1:-}" == get ]] || exit 0
host=
while IFS='=' read -r key value && [[ -n "$key" ]]; do
  [[ "$key" == host ]] && host="$value"
done
[[ "$host" == github.com ]] || exit 0
token=$("$(dirname "$0")/agent-token.sh")
echo username=x-access-token
echo "password=$token"
