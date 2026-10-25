#!/usr/bin/env bash
# SPIKE ONLY. Enables KVM, installs the emulator and $SYSTEM_IMAGE, creates the AVD $AVD and starts it in the background.
# Writes $RUNNER_TEMP/emulator.log and emulator.pid; adds platform-tools to the PATH and ANDROID_AVD_HOME to the job's env.
set -euo pipefail
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
(yes || true) | "$sdk/sdkmanager" --licenses > /dev/null
"$sdk/sdkmanager" --install emulator "$SYSTEM_IMAGE"
"$sdk/sdkmanager" --list_installed | grep -E "emulator|system-images|platform-tools|build-tools"
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

# The runner's 4 vCPUs and 4 GB; the pixel_7 profile's default of 2 cores and 1.5 GB starves the keyboard and WebView.
nohup "$ANDROID_HOME/emulator/emulator" -avd "$AVD" -no-window -no-audio -no-boot-anim -no-snapshot \
  -cores 4 -memory 4096 -gpu swiftshader_indirect -verbose > "$RUNNER_TEMP/emulator.log" 2>&1 &
echo $! > "$RUNNER_TEMP/emulator.pid"
echo "Emulator started, pid $(cat "$RUNNER_TEMP/emulator.pid")"
echo "$ANDROID_HOME/platform-tools" >> "$GITHUB_PATH"
