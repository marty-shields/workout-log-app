#!/usr/bin/env bash
# Usage: agent-install.sh <app id> [private key]   (default key: ~/.config/marty-dev-bot/private-key.pem)
# Sets up the marty-dev-bot GitHub App identity for the ~/dev-bot folder. Copies agent-token.sh and
# agent-git-credential.sh to ~/.local/share/marty-dev-bot/bin and agent-gh.sh to ~/.local/bin/gh, writes
# ~/.config/marty-dev-bot/config with the App's one installation, writes ~/.config/marty-dev-bot/gitconfig with the
# bot's name, email and credential helper and includes it in the global git config for repos under ~/dev-bot, and
# prints the bot's login, user id and commit email. Re-run it after these scripts change. The real gh is /usr/bin/gh,
# or $AGENT_REAL_GH, which exists so the tests can stand in a fake.
set -euo pipefail
app_id="$1"
key=$(realpath -- "${2:-$HOME/.config/marty-dev-bot/private-key.pem}")
[[ -f "$key" ]] || { echo "agent-install.sh: no private key at $key" >&2; exit 1; }
here=$(dirname "$0")
real_gh="${AGENT_REAL_GH:-/usr/bin/gh}"
bin="$HOME/.local/share/marty-dev-bot/bin"
config="$HOME/.config/marty-dev-bot/config"
gitconfig="$HOME/.config/marty-dev-bot/gitconfig"
folder="$HOME/dev-bot"
settings="$folder/.claude/settings.local.json"

umask 077
mkdir -p "$bin" "$(dirname "$config")" "$HOME/.local/bin"
install -m 755 "$here/agent-token.sh" "$here/agent-git-credential.sh" "$bin/"
install -m 755 "$here/agent-gh.sh" "$HOME/.local/bin/gh"
rm -f "$bin/gh"

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
read -r installation account slug <<< "${installations[0]}"
write_config "$installation"

token=$("$bin/agent-token.sh")
id=$(GH_TOKEN="$token" "$real_gh" api "users/$slug%5Bbot%5D" -q .id)
email="$id+${slug}[bot]@users.noreply.github.com"

printf '[user]\n\tname = %s\n\temail = %s\n[credential "https://github.com"]\n\thelper =\n\thelper = !%s\n' \
  "${slug}[bot]" "$email" "$bin/agent-git-credential.sh" > "$gitconfig"
chmod 600 "$gitconfig"
# The empty helper in the included file clears gh's helper only if the include comes after it in the global config.
git config --global 'includeIf.gitdir:~/dev-bot/.path' "$gitconfig"

mkdir -p "$(dirname "$settings")"
if [[ ! -e "$settings" ]]; then
  printf '{"permissions":{"deny":["Bash(/usr/bin/gh:*)","Read(/%s/.config/gh/**)"]}}\n' "$HOME" > "$settings"
fi

echo "Installed to $bin and $HOME/.local/bin/gh, config in $config (installation $installation on $account)."
echo "Git in repos under $folder uses $gitconfig."
echo "Bot login: ${slug}[bot]"
echo "Bot user id: $id"
echo "Commit email: $email"
echo "Remove the old env block and SessionStart hook from any settings.local.json an earlier setup wrote."
