#!/usr/bin/env bash
# Usage: gh <arguments>   (agent-install.sh installs this script as ~/.local/bin/gh)
# Runs the real gh as the marty-dev-bot GitHub App when the current folder is inside ~/dev-bot, with a token from
# ~/.local/share/marty-dev-bot/bin/agent-token.sh, and as you everywhere else. The real gh is /usr/bin/gh, or
# $AGENT_REAL_GH, which exists so the tests can stand in a fake.
set -euo pipefail
real_gh="${AGENT_REAL_GH:-/usr/bin/gh}"
if [[ "$PWD/" == "$HOME/dev-bot/"* ]]; then
  # Its own assignment, so a failed token stops here instead of running gh as you.
  token=$("$HOME/.local/share/marty-dev-bot/bin/agent-token.sh")
  GH_TOKEN="$token" exec "$real_gh" "$@"
fi
exec "$real_gh" "$@"
