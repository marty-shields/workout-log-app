#!/usr/bin/env bash
# agent-token.sh and agent-git-credential.sh with a throwaway key and a fake curl that records each request.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
token_script="$here/../agent-token.sh"
credential_script="$here/../agent-git-credential.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
export HOME="$work/home"
config="$HOME/.config/workout-log-agent/config"
cache="$HOME/.cache/workout-log-agent/token.json"
mkdir -p "$work/bin" "$(dirname "$config")"
openssl genrsa -out "$work/key.pem" 2048 2> /dev/null
openssl rsa -in "$work/key.pem" -pubout -out "$work/public.pem" 2> /dev/null

cat > "$work/bin/curl" << EOF
#!/usr/bin/env bash
printf '%s\n' "\$@" >> "$work/curl.log"
case "\${*: -1}" in
  */app/installations/178/access_tokens) printf '{"token":"ghs_fresh","expires_at":"%s"}\n' "\$(date -u -d '+1 hour' +%FT%TZ)" ;;
  */app/installations) echo '[{"id":178,"account":{"login":"marty-shields"}}]' ;;
  *) exit 22 ;;
esac
EOF
chmod +x "$work/bin/curl"
export PATH="$work/bin:$PATH"

write_config() { printf 'APP_ID=%s\nINSTALLATION_ID=%s\nKEY=%s\n' "$1" "$2" "$3" > "$config"; }
cache_token() { printf '{"token":"%s","expires_at":"%s"}\n' "$1" "$(date -u -d "$2" +%FT%TZ)" > "$cache"; }
base64url_decode() {
  local text
  text=$(tr -- '-_' '+/')
  while (( ${#text} % 4 )); do text+='='; done
  printf '%s' "$text" | openssl base64 -d -A
}

write_config 5623 178 "$work/key.pem"
rm -f "$cache" "$work/curl.log"
before=$(date +%s)
check "a token is fetched when there is no cache" ghs_fresh "$(bash "$token_script")"
check "the token request is a POST" "-X POST" \
  "$(grep -A1 -x -- -X "$work/curl.log" | paste -sd' ')"
check "the cache file is private" 600 "$(stat -c %a "$cache")"
check "the cache holds GitHub's response" ghs_fresh "$(jq -r .token "$cache")"

jwt=$(sed -n 's/^Authorization: Bearer //p' "$work/curl.log")
IFS=. read -r header claims signature <<< "$jwt"
check "the JWT has 3 parts" 3 "$(tr '.' '\n' <<< "$jwt" | grep -c .)"
check "the JWT header is RS256" '{"alg":"RS256","typ":"JWT"}' "$(base64url_decode <<< "$header")"
base64url_decode <<< "$signature" > "$work/signature"
check "the JWT signature verifies with the public key" "Verified OK" \
  "$(printf '%s.%s' "$header" "$claims" | openssl dgst -sha256 -verify "$work/public.pem" -signature "$work/signature")"
payload=$(base64url_decode <<< "$claims")
check "the JWT is issued by the App" '"5623"' "$(jq .iss <<< "$payload")"
check "the JWT's iat is a minute back" true "$(jq --argjson now "$before" '.iat >= $now - 61 and .iat <= $now - 55' <<< "$payload")"
check "the JWT lasts 10 minutes from iat" 600 "$(jq '.exp - .iat' <<< "$payload")"

rm -f "$work/curl.log"
cache_token ghs_cached '+1 hour'
check "a fresh cached token is reused" ghs_cached "$(bash "$token_script")"
check "a reused token calls no curl" absent "$([[ -e "$work/curl.log" ]] && echo called || echo absent)"

cache_token ghs_old '+5 minutes'
check "a token close to expiry is refreshed" ghs_fresh "$(bash "$token_script")"
check "the refreshed token is cached" ghs_fresh "$(jq -r .token "$cache")"

echo '{"token":"ghs_bad","expires_at":"not a time"}' > "$cache"
check "a cache with an unreadable expiry is refreshed" ghs_fresh "$(bash "$token_script")"

rm -f "$work/curl.log"
write_config 5623 "" "$work/key.pem"
check "--installation-id lists the installations without INSTALLATION_ID" "178 marty-shields" \
  "$(bash "$token_script" --installation-id)"
check "--installation-id asks GitHub for the App's installations" 1 \
  "$(grep -cx 'https://api.github.com/app/installations' "$work/curl.log")"

# fails <name> <expected message part>
fails() {
  local err status
  err=$(bash "$token_script" 2>&1 > /dev/null)
  status=$?
  check "$1 exits 1" 1 "$status"
  check "$1 says what is wrong and to run agent-install.sh" yes \
    "$([[ "$err" == *"$2"* && "$err" == *agent-install.sh* ]] && echo yes || echo "no: $err")"
}
fails "an empty INSTALLATION_ID" "INSTALLATION_ID is empty"
write_config "" 178 "$work/key.pem"
fails "an empty APP_ID" "APP_ID is empty"
write_config 5623 178 "$work/missing.pem"
fails "a missing key" "no private key at $work/missing.pem"
rm -f "$config"
fails "a missing config" "no config at $config"

write_config 5623 178 "$work/key.pem"
cache_token ghs_cached '+1 hour'
check "the credential helper answers for github.com" $'username=x-access-token\npassword=ghs_cached' \
  "$(printf 'protocol=https\nhost=github.com\n\n' | bash "$credential_script" get)"
check "the credential helper ignores other hosts" "" \
  "$(printf 'protocol=https\nhost=gitlab.com\n\n' | bash "$credential_script" get)"
check "the credential helper ignores store" "" \
  "$(printf 'protocol=https\nhost=github.com\nusername=x\npassword=y\n\n' | bash "$credential_script" store)"

finish
