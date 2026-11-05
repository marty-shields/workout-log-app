#!/usr/bin/env bash
# Enables KVM, gets the emulator and the system image pinned in .github/android-emulator.env, creates the AVD $AVD and
# starts it in the background. Run by the start-emulator action, which restores both packages from the cache first: with
# EMULATOR_CACHE_HIT=true and the pinned versions in place, sdkmanager doesn't run. Otherwise `emulator-sdk.sh install`
# downloads the latest stable ones. Writes $RUNNER_TEMP/emulator.log and emulator.pid; adds platform-tools to the PATH and
# ANDROID_AVD_HOME to the job's env.
set -euo pipefail
here="$(dirname "$0")"
source "$here/emulator-launch.sh"
# The pinned image is AOSP: no Play services or Gboard.
# shellcheck source=.github/android-emulator.env
source "$here/../android-emulator.env"
sdk="$ANDROID_HOME/cmdline-tools/latest/bin"

echo "::group::Host"
echo "CPUs: $(nproc)"; free -m; df -h / | tail -n 1
echo "CPU virtualisation flags: $(grep -c -E '(vmx|svm)' /proc/cpuinfo || true)"
echo "::endgroup::"

echo 'KERNEL=="kvm", GROUP="kvm", MODE="0666", OPTIONS+="static_node=kvm"' | sudo tee /etc/udev/rules.d/99-kvm4all.rules
sudo udevadm control --reload-rules
sudo udevadm trigger --name-match=kvm
ls -l /dev/kvm

# avdmanager and the emulator look for AVDs in different default folders; ANDROID_AVD_HOME sets one for both.
export ANDROID_AVD_HOME="$HOME/.android/avd"
mkdir -p "$ANDROID_AVD_HOME"
echo "ANDROID_AVD_HOME=$ANDROID_AVD_HOME" >> "$GITHUB_ENV"

echo "::group::Emulator libraries"
sudo apt-get install -y --no-install-recommends libpulse0 > /dev/null
echo "::endgroup::"

echo "::group::Install the emulator and image"
if [[ "${EMULATOR_CACHE_HIT:-}" == true ]] && "$here/emulator-sdk.sh" check; then
  echo "Restored from the cache; sdkmanager did not run."
else
  if [[ "${EMULATOR_CACHE_HIT:-}" == true ]]; then
    echo "::warning title=Emulator cache::The restored cache doesn't hold the pinned versions; installing with sdkmanager."
  else
    echo "::notice title=Emulator cache::The emulator cache missed; downloading with sdkmanager. The Emulator cache workflow (emulator-cache.yml) fills it."
  fi
  "$here/emulator-sdk.sh" install
fi
echo "::endgroup::"

echo "::group::Create the AVD"
echo no | "$sdk/avdmanager" create avd -n "$AVD" -k "$SYSTEM_IMAGE" -d pixel_7
"$sdk/avdmanager" list avd
avd_path=$("$sdk/avdmanager" list avd | sed -n 's/^ *Path: //p' | head -n 1)
cat "$avd_path/config.ini"
echo "::endgroup::"

echo "::group::Emulator version and acceleration"
"$ANDROID_HOME/emulator/emulator" -version | head -n 3 || true
"$ANDROID_HOME/emulator/emulator" -accel-check
echo "::endgroup::"

launch_emulator
echo "Emulator started, pid $(cat "$RUNNER_TEMP/emulator.pid")"
echo "$ANDROID_HOME/platform-tools" >> "$GITHUB_PATH"
