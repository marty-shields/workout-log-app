# 08 — Dev Setup & Build

## App identity
- Display name: **Workout Log**
- Android package id: **`uk.martinshields.workoutlog`**. This can't change later without a reinstall, which wipes all data.
- Debug builds: package id **`uk.martinshields.workoutlog.debug`**, name **Workout Log (Dev)**. They install alongside the release app with their own data, so USB debugging never touches real workouts.
- Version: display version is SemVer (`1.0.0`, `1.1.0`…). The version code goes up by 1 on every release APK.

## Prerequisites (Windows)
- .NET 10 SDK: the version in `global.json` or a later patch (T91)
- One of (D132):
  - **Visual Studio** with the **.NET Multi-platform App UI development** workload. It installs the MAUI workloads, the Android SDK (`Android\android-sdk` under Program Files (x86)) and OpenJDK.
  - **VS Code** with the **.NET MAUI** extension (brings C# Dev Kit), plus `dotnet workload install maui-android`, the Android SDK (platform API 36, build tools, platform tools) and the JDK the .NET 10 Android workload needs.
- Optional: an Android emulator for API 36, to run the E2E tests without the phone ([Use an emulator](#use-an-emulator)). Creating one needs admin rights when the SDK is in Program Files.
- For the E2E tests: Node.js 20.19+, 22.12+ or 24+, and the `ANDROID_HOME` and `JAVA_HOME` environment variables ([E2E tests](#e2e-tests)). Appium and its driver come from the repo; Chromedriver downloads itself.
- Git, and the GitHub CLI (`gh`) signed in as the repo owner ([Source control](#source-control))
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
4. Run the tests: every scenario, the smoke scenarios, one story's or one feature's:
   ```
   dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~WorkoutLog.E2E.Tests"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=US-17"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~EditWorkoutFeature"
   ```
   Run every scenario before each release APK (D204). With more than one device connected, name one: add `-- 'TestRunParameters.Parameter(name="DeviceUdid", value="<id from adb devices>")'`.

What a run does:
- Starts Appium from `node_modules` on a free port and stops it at the end.
- Installs the APK over the Debug app.
- Before each scenario, clears the Debug app's data and launches it. Any workouts in "Workout Log (Dev)" are deleted; the Release app is never touched.
- A scenario that starts from saved workouts builds a database on the PC with Core and copies it into the Debug app with `run-as`, which needs the debuggable Debug build (T81). Build the App and run the tests from the same commit: a database from a newer Core can stop the app starting.
- A `@today` scenario fails at its start within 5 minutes of midnight on the device; run it again after midnight (D205).
- At the end, closes the Debug app. It stays installed with the last test's data, so a failure can be checked by hand.
- Downloads a Chromedriver that matches the device's WebView into `tests/WorkoutLog.E2E.Tests/.chromedriver/` when needed.
- Turns auto-rotate off and holds the screen in portrait, then puts both rotation settings back at the end (T86). After a killed run, set auto-rotate back yourself: `adb shell settings put system accelerometer_rotation 1` (on) or `0` (off).
- Installs Appium's helper apps on the device (`io.appium.uiautomator2.server`, `io.appium.uiautomator2.server.test`, `io.appium.settings`) and removes them at the end. After a killed run, remove them with `adb uninstall <package>` for each.

A plain `dotnet test` or `dotnet test WorkoutLog.slnx` skips the E2E tests (`[Explicit]`) and doesn't start Appium.

If a run is killed (Ctrl+C, IDE stop), Appium may keep running: stop the `node` process whose command line has `node_modules\appium\index.js`. Turn off wireless debugging when you're done.

### Results
- `tests/WorkoutLog.E2E.Tests/TestResults/E2E/<yyyyMMdd-HHmmss>/` per run, holding the run's `appium.log`.
- Each scenario is numbered in the output, e.g. `E2E 20261024-091420/01 A fresh install opens on the empty Dashboard`.
- A failed scenario saves to `<run>/<nn>-<scenario>/`, named by its title with dashes for spaces, at most 60 characters:
  - `screen.png`: the device screen, black if the device is locked
  - `webview.html`, or `native.xml` if it failed before reaching the WebView
  - `workoutlog.log`: the app's log, from the external files folder only
  - `appium.log`: the test's part of the server log
- The files are also attached to the test result, e.g. in a `--logger trx` file.
- A phone run's files can show whatever is on the screen (notifications, another app), the phone's serial number and what the tests typed. Keep them local; don't share them.
- Turn on Do Not Disturb during a run on your own phone, so notifications stay out of the screenshots.

### Use an emulator
The Debug APK holds both arm64 (phone) and x86_64 (emulator) code, so the same build runs on an x86_64 emulator.
1. Turn on **Windows Hypervisor Platform** (Windows Features; admin).
2. Install the emulator and an API 36 image, from `%ANDROID_HOME%\cmdline-tools\latest\bin` in an admin terminal:
   ```
   sdkmanager "emulator" "system-images;android-36;google_apis;x86_64"
   avdmanager create avd -n WorkoutLog_API36 -k "system-images;android-36;google_apis;x86_64"
   ```
3. Start it: `%ANDROID_HOME%\emulator\emulator.exe -avd WorkoutLog_API36`. `adb devices` lists it as `emulator-5554`.
4. Run the tests as above, adding the run setting `DeviceUdid` = `emulator-5554` if the phone is connected too.

### E2E in CI later
The [PR build](#ci) doesn't run the E2E tests. A CI job for them needs:
- Node.js, `npm ci --ignore-scripts` in `tests/WorkoutLog.E2E.Tests`
- the .NET 10 SDK with the `maui-android` workload, a JDK, and `ANDROID_HOME` and `JAVA_HOME`
- an emulator: a Linux runner with KVM, started before the tests. Never the phone.
- Steps: build the App, run the test command with the emulator's `DeviceUdid`, then keep `TestResults/E2E/` as the job's artifact with a short retention. Never publish the Debug APK: it is debuggable and has WebView debugging on.

## CI
- `.github/workflows/pr-build.yml` ("PR build") runs on every PR to `main` (T91). On `ubuntu-latest` it installs the SDK from `global.json` and the `maui-android` workload at a pinned workload set, runs `dotnet build WorkoutLog.slnx`, then the Core and UI tests. Its `build-and-test` check must pass before the PR can merge.
- Failed tests show as annotations on the PR, linked to the test's source line, and each test step adds a results table to the run's summary (T95).
- The workload set matches the SDK: when the SDK in `global.json` changes, set the workflow's `--version` to the matching set (`dotnet workload search version`) in the same PR.
- `.github/dependabot.yml` opens update PRs weekly for NuGet, the E2E project's npm packages, the SDK in `global.json` and the GitHub Actions (T92). Minor and patch bumps come grouped, one PR per ecosystem; each major bump has its own PR. The PR build runs on them like any other PR.
- Repo Actions settings: only GitHub-owned actions, pinned by full commit SHA; workflows from outside contributors' PRs wait for approval (T91).

## Release: signed APK
1. Create a keystore once (`keytool -genkey ...`).
2. Set the signing properties in the app `.csproj` (`AndroidKeyStore`, `AndroidSigningKeyStore`, `AndroidSigningKeyAlias`). Passwords come from environment variables and are never stored in the repo.
3. Bump the version code (and the display version if needed).
4. `dotnet publish src/WorkoutLog.App -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
5. Copy the signed `.apk` to the phone and install it (allow "install unknown apps" for the file manager).

**The same keystore must sign every future APK.** If it's lost, updates can't be installed over the existing app. The only way back is to uninstall, which wipes all workout data.

## Keystore handling
- The keystore file lives outside the repo and is listed in `.gitignore`.
- Keep a backup copy. **The location is still open ([US-25](https://github.com/marty-shields/workout-log-app/issues/34)) and must be decided before the first release APK.**

## Source control
- GitHub: [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app), public, default branch `main` (T89).
- The repo's own git config sets the author (`user.name`, `user.email`) and `credential.username`. The remote URL includes the username, so Git Credential Manager signs in as the owner in this repo only.
- `gh` reads its login from the folder in `GH_CONFIG_DIR`, which holds only the owner's account. `.claude/settings.local.json` sets it for Claude Code and `.vscode/settings.json` for VS Code terminals; both are local and gitignored. Sign in once with `$env:GH_CONFIG_DIR="$HOME\.config\gh-personal"; gh auth login --web --scopes project,workflow`, and answer No to authenticating Git.
- `main` is protected by the `Protect main` ruleset: no direct pushes, force pushes or deletion. Changes go through a PR, with no required approvals, and the PR build's `build-and-test` check must pass. PRs are opened as drafts; the reviewer marks them ready and squash-merges. The PR title becomes the commit title; merged branches are deleted.
- Branches: `type/us-xx-slug`, the Conventional Commit type and the story ID (e.g. `feat/us-26-signed-release-apk`). Work that isn't a story leaves out the ID (e.g. `chore/update-readme`).
- Backlog: the [Workout Log project](https://github.com/users/marty-shields/projects/2) ([ways of working](README.md#ways-of-working)).
- `.gitignore` (T39, T90): build output and logs, test results, IDE and per-user files (`*.user`, `.vscode/`, `.claude/settings.local.json`), APKs, signing keys and secrets (`*.apk`, `*.aab`, `*.keystore`, `*.jks`, `*.pfx`, `*.p12`, `*.pem`, `*.key`, `.env`), tool output (including `node_modules/` and `.chromedriver/`) and OS files.
- `nuget.config` restores from nuget.org only (T40).
- Commit messages and PR titles: Conventional Commits.
