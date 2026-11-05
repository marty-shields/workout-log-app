#!/usr/bin/env bash
# Usage: agent-install.sh <app id> [private key]   (default key: ~/.config/workout-log-agent/private-key.pem)
# Sets up the workout-log-agent GitHub App identity on this machine. Copies agent-token.sh, agent-git-credential.sh and
# agent-gh.sh (as gh) to ~/.local/share/workout-log-agent/bin, writes ~/.config/workout-log-agent/config with the App's
# one installation, and prints the bot's user id and commit email. Re-run it after these scripts change.
set -euo pipefail
app_id="$1"
key=$(realpath -- "${2:-$HOME/.config/workout-log-agent/private-key.pem}")
[[ -f "$key" ]] || { echo "agent-install.sh: no private key at $key" >&2; exit 1; }
here=$(dirname "$0")
bin="$HOME/.local/share/workout-log-agent/bin"
config="$HOME/.config/workout-log-agent/config"

umask 077
mkdir -p "$bin" "$(dirname "$config")"
install -m 755 "$here/agent-token.sh" "$here/agent-git-credential.sh" "$bin/"
install -m 755 "$here/agent-gh.sh" "$bin/gh"

write_config() {
  printf 'APP_ID=%q\nINSTALLATION_ID=%q\nKEY=%q\n' "$app_id" "$1" "$key" > "$config"
  chmod 600 "$config"
}

# Keeps the installation id of an earlier run until the lookup succeeds, so a failed re-run leaves the token working.
write_config "$(sed -n 's/^INSTALLATION_ID=//p' "$config" 2> /dev/null || true)"
mapfile -t installations < <("$bin/agent-token.sh" --installation-id)
if (( ${#installations[@]} != 1 )); then
  echo "agent-install.sh: the App needs exactly one installation, found ${#installations[@]}." >&2
  printf '%s\n' "${installations[@]}" >&2
  exit 1
fi
write_config "${installations[0]%% *}"

id=$("$bin/gh" api 'users/workout-log-agent%5Bbot%5D' -q .id)
echo "Installed to $bin, config in $config (installation ${installations[0]})."
echo "Bot user id: $id"
echo "Commit email: $id+workout-log-agent[bot]@users.noreply.github.com"
