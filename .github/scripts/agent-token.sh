#!/usr/bin/env bash
# Usage: agent-token.sh [--installation-id]
# Prints an installation token of the workout-log-agent GitHub App. Reads APP_ID, INSTALLATION_ID and KEY (the private
# key's path) from ~/.config/workout-log-agent/config, which agent-install.sh writes. Reuses the token cached in
# ~/.cache/workout-log-agent/token.json while more than 10 minutes remain. With --installation-id it prints
# "<id> <account>" for each installation of the App instead, so it needs no INSTALLATION_ID. Needs openssl, curl and jq.
set -euo pipefail
config="$HOME/.config/workout-log-agent/config"
cache="$HOME/.cache/workout-log-agent/token.json"

fail() {
  echo "agent-token.sh: $1. Run .github/scripts/agent-install.sh <app id> to set it up." >&2
  exit 1
}

[[ -f "$config" ]] || fail "no config at $config"
# shellcheck source=/dev/null
source "$config"
[[ -n "${APP_ID:-}" ]] || fail "APP_ID is empty in $config"
[[ -n "${KEY:-}" ]] || fail "KEY is empty in $config"
[[ -f "$KEY" ]] || fail "no private key at $KEY"

base64url() { openssl base64 -A | tr '+/' '-_' | tr -d '='; }

jwt() {
  local now header claims signature
  now=$(date +%s)
  header=$(printf '{"alg":"RS256","typ":"JWT"}' | base64url)
  claims=$(printf '{"iat":%d,"exp":%d,"iss":"%s"}' $((now - 60)) $((now + 540)) "$APP_ID" | base64url)
  signature=$(printf '%s.%s' "$header" "$claims" | openssl dgst -sha256 -sign "$KEY" | base64url)
  printf '%s.%s.%s' "$header" "$claims" "$signature"
}

# github <method> <path>
github() {
  local token
  token=$(jwt)
  curl -fsS -X "$1" -H "Authorization: Bearer $token" -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" "https://api.github.com$2"
}

if [[ "${1:-}" == --installation-id ]]; then
  github GET /app/installations | jq -r '.[] | "\(.id) \(.account.login)"'
  exit
fi

[[ -n "${INSTALLATION_ID:-}" ]] || fail "INSTALLATION_ID is empty in $config"

cached_token() {
  local expires
  expires=$(jq -r '.expires_at // empty' "$cache" 2> /dev/null) && [[ -n "$expires" ]] || return 1
  expires=$(date -d "$expires" +%s 2> /dev/null) || return 1
  (( expires - $(date +%s) > 600 )) || return 1
  jq -er .token "$cache"
}

if cached_token; then
  exit
fi

umask 077
mkdir -p "$(dirname "$cache")"
response=$(mktemp "$cache.XXXXXX")
trap 'rm -f "$response"' EXIT
github POST "/app/installations/$INSTALLATION_ID/access_tokens" > "$response" \
  || fail "GitHub refused the token request; check APP_ID, INSTALLATION_ID and the key"
token=$(jq -er .token "$response") || fail "GitHub's response has no token"
mv "$response" "$cache"
echo "$token"
