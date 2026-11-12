#!/usr/bin/env bash
# previous-apk.sh in a throwaway repo with a stub dotnet: which tag it builds, and when its restore is locked.
set -uo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
source "$here/lib.sh"
script="$here/../previous-apk.sh"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
repo="$work/repo"
mkdir -p "$work/bin"
cat > "$work/bin/dotnet" << 'STUB'
#!/usr/bin/env bash
verb=$1 app=$2 framework=- out= lock=locked
while (( $# )); do
  case "$1" in
    -f) framework=$2 ;;
    -o) out=$2 ;;
    -p:RestoreLockedMode=false) lock=unlocked ;;
  esac
  shift
done
echo "$verb $framework $lock" >> "$DOTNET_LOG"
case "$verb" in
  build) mkdir -p "$app/bin/Debug/$framework" && touch "$app/bin/Debug/$framework/uk.martinshields.workoutlog.debug-Signed.apk" ;;
  publish) mkdir -p "$out" && touch "$out/uk.martinshields.workoutlog-Signed.apk" ;;
esac
STUB
chmod +x "$work/bin/dotnet"

git_() { git -C "$repo" "$@"; }
# commit_net <NetVersion, or empty for none>
commit_net() {
  local props="<Project><PropertyGroup><NetVersion>$1</NetVersion></PropertyGroup></Project>"
  [[ -n "$1" ]] || props="<Project />"
  echo "$props" > "$repo/Directory.Build.props"
  git_ add -A; git_ commit -q --allow-empty -m "net $1"
}
fresh() {
  rm -rf "$repo"
  git init -q -b main "$repo"
  git_ config user.email t@t; git_ config user.name t
  echo '{}' > "$repo/global.json"
}

# run <name> <configuration> <expected tag, exit, copied APK and dotnet calls>
run() {
  local out
  : > "$work/dotnet.log"
  rm -f "$work/out.apk"
  out=$(cd "$repo" && PATH="$work/bin:$PATH" DOTNET_LOG="$work/dotnet.log" bash "$script" "$work/out.apk" "$2" 2> /dev/null
    echo "exit=$?")
  [[ -f "$work/out.apk" ]] && out+=$'\napk'
  [[ -s "$work/dotnet.log" ]] && out+=$'\n'"$(< "$work/dotnet.log")"
  check "$1" "$3" "$out"
}

fresh; commit_net net10.0; git_ tag v1.0.0; commit_net net11.0
run "a tag on an older NetVersion builds Debug for its own NetVersion, unlocked" Debug "v1.0.0
exit=0
apk
build net10.0-android unlocked"
run "a tag on an older NetVersion restores and publishes Release unlocked" Release "v1.0.0
exit=0
apk
restore - unlocked
publish net10.0-android unlocked"

git_ tag v1.1.0; commit_net net11.0
run "a tag on this commit's NetVersion builds Debug locked" Debug "v1.1.0
exit=0
apk
build net11.0-android locked"
run "a tag on this commit's NetVersion restores and publishes Release locked" Release "v1.1.0
exit=0
apk
restore - locked
publish net11.0-android locked"

fresh; commit_net ""; git_ tag v1.0.0; commit_net net11.0
run "a tag with no NetVersion fails before building" Debug "exit=1"

finish
