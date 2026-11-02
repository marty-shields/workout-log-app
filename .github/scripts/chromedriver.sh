#!/usr/bin/env bash
# Usage: chromedriver.sh <folder>
# Prints the path of the Chromedriver pinned in qa-pipeline.yml (CHROMEDRIVER_VERSION, CHROMEDRIVER_SHA256). Downloads it
# into <folder> and checks its SHA-256 first, unless an earlier run left it there. Fails when the connected device's WebView
# major version isn't the pin's.
set -euo pipefail
folder="$1"
workflow="$(dirname "$0")/../workflows/qa-pipeline.yml"

pin() { sed -n "s/^ *$1: *//p" "$workflow"; }
version=$(pin CHROMEDRIVER_VERSION)
sha=$(pin CHROMEDRIVER_SHA256)
webview=$(adb shell dumpsys webviewupdate | sed -n 's/.*Current WebView package (name, version): ([^,]*, \([0-9.]*\)).*/\1/p')
if [[ "${webview%%.*}" != "${version%%.*}" ]]; then
  echo "::error title=Chromedriver::WebView $webview, pinned Chromedriver $version: update the pin in qa-pipeline.yml (docs/dev-setup-build.md)." >&2
  exit 1
fi

dir="$folder/chromedriver-$version"
if [[ ! -x "$dir/chromedriver-linux64/chromedriver" ]]; then
  mkdir -p "$dir"
  curl -fsSL -o "$dir/chromedriver.zip" \
    "https://storage.googleapis.com/chrome-for-testing-public/$version/linux64/chromedriver-linux64.zip"
  echo "$sha  $dir/chromedriver.zip" | sha256sum -c - >&2
  unzip -q -o -d "$dir" "$dir/chromedriver.zip"
fi
echo "$dir/chromedriver-linux64/chromedriver"
