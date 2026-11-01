# Dev setup: Windows
Building the app and setting up the E2E tests on Windows, with a phone or an emulator. Running the tests: [E2E tests](e2e-tests.md). Linux: [Dev setup: Linux](dev-setup-linux.md).

## Prerequisites
- **Visual Studio** with the **.NET Multi-platform App UI development** workload (installs the MAUI workloads, Android SDK and OpenJDK), or **VS Code** with the **.NET MAUI** extension plus `dotnet workload install maui-android`, the Android SDK (API 36) and a JDK.
- For the E2E tests: Node.js 20.19+, 22.12+ or 24+, and `ANDROID_HOME` and `JAVA_HOME` ([One-time setup for the E2E tests](#e2e-one-time-setup)).

The .NET SDK, Git and `gh` are in [Dev setup & build](dev-setup-build.md#prerequisites).

## E2E one-time setup
1. Set the user environment variables, then restart the terminal and IDE:
   - `ANDROID_HOME` = the Android SDK folder (holds `platform-tools\adb.exe`).
   - `JAVA_HOME` = the JDK folder (holds `bin\java.exe`).
2. In `tests/WorkoutLog.E2E.Tests`, run `npm ci --ignore-scripts` to install Appium and its driver. Run it again after either version changes.
3. Phone: USB or wireless debugging on ([Phone setup](dev-setup-build.md#phone-setup-android-16)).

## E2E on an emulator
The Debug APK also runs on an x86_64 emulator.
1. Turn on **Windows Hypervisor Platform** (Windows Features; admin).
2. From `%ANDROID_HOME%\cmdline-tools\latest\bin` in an admin terminal:
   ```
   sdkmanager "emulator" "system-images;android-36;google_apis;x86_64"
   avdmanager create avd -n WorkoutLog_API36 -k "system-images;android-36;google_apis;x86_64"
   ```
3. Start it: `%ANDROID_HOME%\emulator\emulator.exe -avd WorkoutLog_API36`. It shows as `emulator-5554`.
4. Run the tests as in [E2E tests](e2e-tests.md), with `DeviceUdid` = `emulator-5554` if the phone is connected too.
