# Dev setup: Linux
Running the E2E tests on an emulator on Linux, mainly for agents working in worktrees. Running the tests: [E2E tests](e2e-tests.md). Windows: [Dev setup: Windows](dev-setup-windows.md).

## Setup
One time per machine. `e2e-local.sh` (below) skips, and names what is missing, until all of this is in place.
1. A JDK (`JAVA_HOME` set), and Node.js and npm at the versions `tests/WorkoutLog.E2E.Tests/package.json` and the QA pipeline use.
2. The .NET SDK version in `global.json`.
3. The Android command-line tools in `$ANDROID_HOME/cmdline-tools/latest`.
4. Environment: `JAVA_HOME`, `ANDROID_HOME` and (if the SDK isn't installed system-wide) `DOTNET_ROOT`; on the `PATH`, `dotnet`, `$ANDROID_HOME/cmdline-tools/latest/bin` and `$ANDROID_HOME/platform-tools`. Set them in your shell profile and restart the shell. `e2e-local.sh` also runs in shells that don't read your profile (an agent's): if they're unset it looks for the SDK in `~/Android/Sdk`, the JDK behind `java` on the `PATH`, the .NET SDK in `~/.dotnet`, and Node.js in `~/.local/node` (ahead of any system Node).
5. Accept the Android licences (`sdkmanager --licenses`), then install `platform-tools`, `emulator` and the system image named in `emulator-start.sh` (`SYSTEM_IMAGE`).
6. The `maui-android` workload, at the `--version` the workflows use.
7. The platform and build-tools the App needs: `dotnet build src/WorkoutLog.App -t:InstallAndroidDependencies -f net10.0-android -p:AndroidSdkDirectory="$ANDROID_HOME" -p:AcceptAndroidSDKLicenses=True`. A first run can print an `aapt2` error after installing them; build again.

Also needs read-write `/dev/kvm` (virtualisation on; your user in the `kvm` group) and the `libpulse` library.

## Throwaway emulator for agents
`.github/scripts/e2e-local.sh "<dotnet test filter>" [tries]` does CI's emulator steps on this machine: boots the AOSP API 36 emulator headless, builds the App and the tests, runs the filter (e.g. `Category=US-17`, `FullyQualifiedName~EditWorkoutFeature`) with CI's Chromedriver pin and retry logic, then stops the emulator whatever the result, including on Ctrl-C, `kill` and a closed pipe. A run takes a boot, a build and the tests (10+ minutes) plus any wait, so run it in the background.
- **Output:** `tests/WorkoutLog.E2E.Tests/TestResults/E2E-emulator/` holds the results (`trx/`), logcat, the health log and the emulator log. A failed scenario's screen, page source and Appium log are in `TestResults/E2E/<yyyyMMdd-HHmmss>/`, as on a phone.
- **Needs, or it skips (exit 2):** everything in [Setup](#setup): `ANDROID_HOME` with the command-line tools, emulator, platform-tools and the system image from `emulator-start.sh`; `JAVA_HOME`; a new enough Node.js; the .NET SDK in `global.json` and the `maui-android` workload at the version the workflows pin; `libpulse`; read-write `/dev/kvm`; 4+ CPUs and 8 GB. It never installs these. It does run `npm ci` (when Appium's lockfile is newer than what's installed) and downloads the pinned Chromedriver (cached).
- **Waits, then skips:** one emulator runs per machine, so runs from other worktrees queue. It also waits for 3 free CPUs, 6 GB of free memory and 10 GB of free disk. After `MAX_WAIT_SECONDS` (default 3600) in all it exits 2.
- **Exit codes:** 0 passed, 1 failed, 2 skipped. A skip is not a pass: report it as not run.
- **The AVD:** it creates and reuses `WorkoutLog_Local_API36` (override with `AVD`, and `SYSTEM_IMAGE`), recreates it if it was made from a different image, and wipes its data on every run. An emulator left by a killed run is stopped when the next run takes the slot.
- `E2E_TRIES` (default 1) is the total number of tries, so 1 means no retry; CI uses 3. The second argument overrides it.

## Keeping it current
Repeat the matching step of [Setup](#setup) after each change:
- **SDK in `global.json` bumped** (Dependabot does this): install that SDK, then the workload at the workflows' version (step 2, then 6). `e2e-local.sh` skips until the SDK matches.
- **Workload `--version` changed in the workflows:** step 6 again; `e2e-local.sh` skips until the installed set equals the pin in `qa-pipeline.yml`. It is set in `dev-pipeline.yml`, `qa-pipeline.yml` and `release.yml`; change every occurrence together.
- **Chromedriver pin changed:** nothing; `e2e-local.sh` reads the pin from `qa-pipeline.yml` and caches the download in `~/.cache/workoutlog-e2e`.
- **Emulator image changed** (`SYSTEM_IMAGE` in `emulator-start.sh`): change the default in `e2e-local.sh` too, then step 5 for the new image; the script recreates its AVD from it. The new image's WebView may need a new Chromedriver pin.
- **Android platform or build-tools changed by the App:** step 7.
- The command-line tools, JDK and Node are not pinned; keep them current. Only a Node or JDK below what Appium or MAUI needs matters.
