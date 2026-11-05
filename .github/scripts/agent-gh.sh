#!/usr/bin/env bash
# Usage: gh <arguments>   (agent-install.sh installs this script as gh)
# Runs /usr/bin/gh as the marty-dev-bot GitHub App, with a token from the agent-token.sh next to it.
set -euo pipefail
token=$("$(dirname "$0")/agent-token.sh")
GH_TOKEN="$token" exec /usr/bin/gh "$@"
