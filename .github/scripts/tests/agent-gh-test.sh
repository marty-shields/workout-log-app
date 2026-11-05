#!/usr/bin/env bash
# The gh shim (agent-gh.sh) and agent-install.sh in a temp HOME, with a fake real gh, a fake curl and a throwaway key.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
scripts="$here/.."

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
export HOME="$work/home"
export GIT_CONFIG_GLOBAL="$HOME/.gitconfig" GIT_CONFIG_NOSYSTEM=1
unset GH_TOKEN GIT_CONFIG_COUNT GIT_CONFIG_PARAMETERS
bin="$HOME/.local/share/marty-dev-bot/bin"
mkdir -p "$work/bin" "$bin" "$HOME/.config/marty-dev-bot" "$HOME/.cache/marty-dev-bot" "$HOME/dev-bot/repo/src" \
  "$HOME/dev-bot-other"
openssl genrsa -out "$work/key.pem" 2048 2> /dev/null

cat > "$work/bin/gh" << 'FAKE'
#!/usr/bin/env bash
if [[ "$*" == "api users/marty-dev-bot%5Bbot%5D -q .id" && "${GH_TOKEN:-}" == ghs_* ]]; then
  echo 352787229
  exit
fi
echo "token=${GH_TOKEN-unset}"
printf 'arg=%s\n' "$@"
FAKE
cat > "$work/bin/curl" << 'FAKE'
#!/usr/bin/env bash
case "${*: -1}" in
  */app/installations/178/access_tokens) printf '{"token":"ghs_fresh","expires_at":"%s"}\n' "$(date -u -d '+1 hour' +%FT%TZ)" ;;
  */app/installations) echo '[{"id":178,"account":{"login":"marty-shields"},"app_slug":"marty-dev-bot"}]' ;;
  *) exit 22 ;;
esac
FAKE
chmod +x "$work/bin/gh" "$work/bin/curl"
export PATH="$work/bin:$PATH" AGENT_REAL_GH="$work/bin/gh"

install -m 755 "$scripts/agent-token.sh" "$bin/"
printf 'APP_ID=5623\nINSTALLATION_ID=178\nKEY=%s\n' "$work/key.pem" > "$HOME/.config/marty-dev-bot/config"
printf '{"token":"ghs_cached","expires_at":"%s"}\n' "$(date -u -d '+1 hour' +%FT%TZ)" \
  > "$HOME/.cache/marty-dev-bot/token.json"

# shim <folder> <arguments>
shim() { (cd "$1" && shift && bash "$scripts/agent-gh.sh" "$@"); }

check "inside ~/dev-bot the shim passes the bot token" token=ghs_cached "$(shim "$HOME/dev-bot" api user | head -1)"
check "in a subfolder of ~/dev-bot the shim passes the bot token" token=ghs_cached \
  "$(shim "$HOME/dev-bot/repo/src" api user | head -1)"
check "in HOME the shim passes no token" token=unset "$(shim "$HOME" api user | head -1)"
check "in a sibling of ~/dev-bot the shim passes no token" token=unset "$(shim "$HOME/dev-bot-other" api user | head -1)"
check "in /tmp the shim passes no token" token=unset "$(shim /tmp api user | head -1)"
check "outside ~/dev-bot the shim keeps an inherited GH_TOKEN" token=ghs_mine \
  "$(GH_TOKEN=ghs_mine shim "$HOME" api user | head -1)"
check "the shim passes the arguments through unchanged" $'arg=pr\narg=view\narg=a b\narg=\narg=--json=x' \
  "$(shim "$HOME/dev-bot" pr view 'a b' '' --json=x | tail -n +2)"
rm "$HOME/.config/marty-dev-bot/config"
check "inside ~/dev-bot the shim stops when there is no token" "1 " \
  "$(out=$(shim "$HOME/dev-bot" api user 2> /dev/null); echo "$? $out")"

rm -rf "$HOME/.config" "$HOME/.cache" "$HOME/dev-bot" "$HOME/.local"
printf '[credential "https://github.com"]\n\thelper =\n\thelper = !/usr/bin/gh auth git-credential\n' > "$GIT_CONFIG_GLOBAL"
mkdir -p "$bin"
touch "$bin/gh"
bash "$scripts/agent-install.sh" 5623577 "$work/key.pem" > "$work/install.log" 2>&1
check "the installer succeeds" 0 "$?"
check "the installer prints the commit email" "Commit email: 352787229+marty-dev-bot[bot]@users.noreply.github.com" \
  "$(grep '^Commit email' "$work/install.log")"
gitconfig="$HOME/.config/marty-dev-bot/gitconfig"
check "the bot gitconfig sets the identity and the credential helper" \
  "$(printf '[user]\n\tname = marty-dev-bot[bot]\n\temail = 352787229+marty-dev-bot[bot]@users.noreply.github.com\n[credential "https://github.com"]\n\thelper =\n\thelper = !%s/agent-git-credential.sh' "$bin")" \
  "$(cat "$gitconfig")"
check "the bot gitconfig is private" 600 "$(stat -c %a "$gitconfig")"
check "the shim is installed as ~/.local/bin/gh" yes "$(cmp -s "$scripts/agent-gh.sh" "$HOME/.local/bin/gh" && echo yes)"
check "the old gh in the bin folder is removed" absent "$([[ -e "$bin/gh" ]] && echo present || echo absent)"
check "the settings deny the real gh and its token" \
  "{\"permissions\":{\"deny\":[\"Bash(/usr/bin/gh:*)\",\"Read(/$HOME/.config/gh/**)\"]}}" \
  "$(cat "$HOME/dev-bot/.claude/settings.local.json")"

echo '{"mine":true}' > "$HOME/dev-bot/.claude/settings.local.json"
bash "$scripts/agent-install.sh" 5623577 "$work/key.pem" > /dev/null 2>&1
check "a re-run succeeds" 0 "$?"
check "a re-run keeps existing settings" '{"mine":true}' "$(cat "$HOME/dev-bot/.claude/settings.local.json")"
check "the include comes once, after gh's helper" \
  "$(printf 'credential.https://github.com.helper=\ncredential.https://github.com.helper=!/usr/bin/gh auth git-credential\nincludeif.gitdir:~/dev-bot/.path=%s' "$gitconfig")" \
  "$(git config --global --list)"

git init -q "$HOME/dev-bot/repo"
git init -q "$HOME/dev-bot-other"
check "a repo in ~/dev-bot commits as the bot" "marty-dev-bot[bot]" "$(git -C "$HOME/dev-bot/repo" config user.name)"
check "a repo in ~/dev-bot ends with the bot's credential helper" "!$bin/agent-git-credential.sh" \
  "$(git -C "$HOME/dev-bot/repo" config --get-all credential.https://github.com.helper | tail -1)"
check "a repo outside ~/dev-bot has no bot name" "" "$(git -C "$HOME/dev-bot-other" config user.name)"
check "a repo outside ~/dev-bot keeps gh's credential helper" "!/usr/bin/gh auth git-credential" \
  "$(git -C "$HOME/dev-bot-other" config --get-all credential.https://github.com.helper | tail -1)"

finish
