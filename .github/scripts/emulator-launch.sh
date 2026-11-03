# shellcheck shell=bash
# Sourced by emulator-start.sh and emulator-recover.sh, so a restart boots the emulator the way the job did.
AVD="${AVD:-WorkoutLog_API36}"

# Starts the emulator $AVD in the background: 3 of the runner's 4 vCPUs, 4 GB of memory, a 2 GB data partition, software
# GPU through ANGLE. Writes $RUNNER_TEMP/emulator.log and emulator.pid.
launch_emulator() {
  nohup "$ANDROID_HOME/emulator/emulator" -avd "$AVD" -no-window -no-audio -no-boot-anim -no-snapshot \
    -cores 3 -memory 4096 -partition-size 2048 -gpu swangle_indirect -verbose > "$RUNNER_TEMP/emulator.log" 2>&1 &
  echo $! > "$RUNNER_TEMP/emulator.pid"
}
