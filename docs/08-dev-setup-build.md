# 08 — Dev Setup & Build

## App identity
- Display name: **Workout Log**
- Android package id: **`uk.martinshields.workoutlog`**. This can't change later without a reinstall, which wipes all data.
- Debug builds: package id **`uk.martinshields.workoutlog.debug`**, name **Workout Log (Dev)**. They install alongside the release app with their own data, so USB debugging never touches real workouts.
- Version: display version is SemVer (`1.0.0`, `1.1.0`…). The version code goes up by 1 on every release APK.

## Prerequisites (Windows)
- .NET 10 SDK
- One of (D132):
  - **Visual Studio** with the **.NET Multi-platform App UI development** workload. It installs the MAUI workloads, the Android SDK (`Android\android-sdk` under Program Files (x86)) and OpenJDK.
  - **VS Code** with the **.NET MAUI** extension (brings C# Dev Kit), plus `dotnet workload install maui-android`, the Android SDK (platform API 36, build tools, platform tools) and the JDK the .NET 10 Android workload needs.
- Optional: an Android emulator for API 36, to run the E2E tests without the phone ([Use an emulator](#use-an-emulator)). Creating one needs admin rights when the SDK is in Program Files.
- For the E2E tests: Node.js 20.19+, 22.12+ or 24+, and the `ANDROID_HOME` and `JAVA_HOME` environment variables ([E2E tests](#e2e-tests)). Appium and its driver come from the repo; Chromedriver downloads itself.
- Git (local repo only)
- Optional: BlazingStory templates, to add story files with `dotnet new bstories`: `dotnet new install BlazingStory.ProjectTemplates::1.0.0-preview.96`

The stories need only the .NET 10 SDK. They don't need the MAUI workload, Android SDK or phone.

## Stories (design system and prototypes)
- `dotnet run --project src/WorkoutLog.UI.Stories`, then open http://localhost:5056
- Story files are `*.stories.razor` in `src/WorkoutLog.UI.Stories/Stories/`.
- Component tests: `dotnet test tests/WorkoutLog.UI.Tests`

## Phone setup (Android 16)
1. Settings → About phone → tap **Build number** 7 times to enable Developer options.
2. Developer options → turn on **USB debugging**.
3. Connect the phone by USB and accept the RSA prompt.
4. `adb devices` should list the phone.

Without USB, use wireless debugging (phone and PC on the same Wi-Fi):
1. Developer options → turn on **Wireless debugging**.
2. **Pair device with pairing code**, then on the PC: `adb pair <ip>:<pairing port> <code>`
3. `adb connect <ip>:<port>`, using the port on the main Wireless debugging screen. The port changes when wireless debugging restarts.

## Development: run on the phone
- Pick the phone as the target device in Visual Studio or VS Code and run/debug.
- Or from the CLI: `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android`
- Debug builds install as "Workout Log (Dev)". They embed their assemblies in the APK (no Fast Deployment), so the APK runs on its own (T78).

## Reading the logs
Debug and Release builds both log to logcat and to a file ([05](05-architecture.md#logging)).
- **Logcat** (phone connected): `adb logcat -s WorkoutLog`. Add `-d` to print what's there and stop.
- **File:** `adb pull /sdcard/Android/data/<package id>/files/logs/`, with the package id from [App identity](#app-identity). The folder holds `workoutlog.log` and, after it reaches 1 MB, the older `workoutlog.1.log`. Uninstalling the app deletes it.
- If the phone has no external storage, the file is in the app data folder instead. Only the Debug app's can be read: `adb shell run-as uk.martinshields.workoutlog.debug cat files/logs/workoutlog.log`.
- Each line: UTC time (file only), level, category, `[EventId]`, message, then any exception.

## E2E tests
`WorkoutLog.E2E.Tests` drives the Debug app on a device through Appium ([07](07-testing.md#e2e-tests-appium)).

### One-time setup
1. Set the user environment variables, then restart the terminal and IDE:
   - `ANDROID_HOME` = the Android SDK folder: `Android\android-sdk` in the folder it was installed to (Visual Studio uses Program Files (x86)). It holds `platform-tools\adb.exe`.
   - `JAVA_HOME` = the JDK folder: with Visual Studio, `Android\openjdk\jdk-<version>` in the folder it installed it to (Program Files); with VS Code, the JDK you installed. It holds `bin\java.exe`.
2. Install Appium and the UiAutomator2 driver from `package-lock.json`: `npm ci --ignore-scripts` in `tests/WorkoutLog.E2E.Tests`. Run it again after either version changes.
3. Phone: USB or wireless debugging on ([Phone setup](#phone-setup-android-16)).

### Each run
1. Connect the device and check `adb devices` lists it. A wireless debugging port changes when wireless debugging restarts: `adb connect <ip>:<new port>`.
2. Unlock the phone and keep it unlocked. A locked device fails each test with "The device is locked". Developer options → **Stay awake** keeps the screen on while charging.
3. Build the App: `dotnet build src/WorkoutLog.App -f net10.0-android`. The tests install this APK; they don't build it.
4. Run the tests:
   ```
   dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~WorkoutLog.E2E.Tests"
   ```
   With more than one device connected, name one: add `-- 'TestRunParameters.Parameter(name="DeviceUdid", value="<id from adb devices>")'`.

What a run does:
- Starts Appium from `node_modules` on a free port and stops it at the end.
- Installs the APK over the Debug app.
- Before each test, clears the Debug app's data and launches it. Any workouts in "Workout Log (Dev)" are deleted; the Release app is never touched.
- At the end, closes the Debug app. It stays installed with the last test's data, so a failure can be checked by hand.
- Downloads a Chromedriver that matches the device's WebView into `tests/WorkoutLog.E2E.Tests/.chromedriver/` when needed.
- Installs Appium's helper apps on the device (`io.appium.uiautomator2.server`, `io.appium.uiautomator2.server.test`, `io.appium.settings`) and removes them at the end. After a killed run, remove them with `adb uninstall <package>` for each.

A plain `dotnet test` or `dotnet test WorkoutLog.slnx` skips the E2E tests (`[Explicit]`) and doesn't start Appium.

If a run is killed (Ctrl+C, IDE stop), Appium may keep running: stop the `node` process whose command line has `node_modules\appium\index.js`. Turn off wireless debugging when you're done.

### Results
- `tests/WorkoutLog.E2E.Tests/TestResults/E2E/<yyyyMMdd-HHmmss>/` per run, holding the run's `appium.log`.
- Each test is numbered in the output, e.g. `E2E 20261024-091420/01 Launch_ShowsTheFreshInstallDashboard`.
- A failed test saves to `<run>/<nn>-<test>/`:
  - `screen.png`: the device screen, black if the device is locked
  - `webview.html`, or `native.xml` if it failed before reaching the WebView
  - `workoutlog.log`: the app's log, from the external files folder only
  - `appium.log`: the test's part of the server log
- The files are also attached to the test result, e.g. in a `--logger trx` file.
- A phone run's files can show whatever is on the screen (notifications, another app), the phone's serial number and what the tests typed. Keep them local; don't share them.

### Use an emulator
The Debug APK holds both arm64 (phone) and x86_64 (emulator) code, so the same build runs on an x86_64 emulator. Not yet tried on this project.
1. Turn on **Windows Hypervisor Platform** (Windows Features; admin).
2. Install the emulator and an API 36 image, from `%ANDROID_HOME%\cmdline-tools\latest\bin` in an admin terminal:
   ```
   sdkmanager "emulator" "system-images;android-36;google_apis;x86_64"
   avdmanager create avd -n WorkoutLog_API36 -k "system-images;android-36;google_apis;x86_64"
   ```
3. Start it: `%ANDROID_HOME%\emulator\emulator.exe -avd WorkoutLog_API36`. `adb devices` lists it as `emulator-5554`.
4. Run the tests as above, adding the run setting `DeviceUdid` = `emulator-5554` if the phone is connected too.

### CI later
A CI job needs:
- Node.js, `npm ci --ignore-scripts` in `tests/WorkoutLog.E2E.Tests`
- the .NET 10 SDK with the `maui-android` workload, a JDK, and `ANDROID_HOME` and `JAVA_HOME`
- an emulator: a Linux runner with KVM, started before the tests. Never the phone.
- Steps: build the App, run the test command with the emulator's `DeviceUdid`, then keep `TestResults/E2E/` as the job's artifact with a short retention. Never publish the Debug APK: it is debuggable and has WebView debugging on.

## Release: signed APK
1. Create a keystore once (`keytool -genkey ...`).
2. Set the signing properties in the app `.csproj` (`AndroidKeyStore`, `AndroidSigningKeyStore`, `AndroidSigningKeyAlias`). Passwords come from environment variables and are never stored in the repo.
3. Bump the version code (and the display version if needed).
4. `dotnet publish src/WorkoutLog.App -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
5. Copy the signed `.apk` to the phone and install it (allow "install unknown apps" for the file manager).

**The same keystore must sign every future APK.** If it's lost, updates can't be installed over the existing app. The only way back is to uninstall, which wipes all workout data.

## Keystore handling
- The keystore file lives outside the repo and is listed in `.gitignore`.
- Keep a backup copy. **The location is an open item and must be decided before the first release APK.**

## Source control
- `git init` locally. No remote.
- `.gitignore` (T39): build output and logs, test results, IDE and per-user files (`*.user`), APKs, signing keys and secrets (`*.apk`, `*.aab`, `*.keystore`, `*.jks`, `*.pfx`, `*.p12`, `*.pem`, `*.key`, `.env`), tool output (including `node_modules/` and `.chromedriver/`) and OS files.
- `nuget.config` restores from nuget.org only (T40).
- Commit messages: Conventional Commits.
