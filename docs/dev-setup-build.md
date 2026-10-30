# Dev Setup & Build

## App identity
- Release: **Workout Log**, package id **`uk.martinshields.workoutlog`**. The id can't change without a reinstall, which wipes all data.
- Debug: **Workout Log (Dev)**, package id **`uk.martinshields.workoutlog.debug`**. It installs alongside the release app with its own data, so USB debugging never touches real workouts.
- Version: SemVer display version; the version code is the number of `v*` tags + 1. The release workflow sets both; local builds are `1.0.0` (1).

## Prerequisites (Windows)
- .NET 10 SDK: the version in `global.json` or a later patch.
- **Visual Studio** with the **.NET Multi-platform App UI development** workload (installs the MAUI workloads, Android SDK and OpenJDK), or **VS Code** with the **.NET MAUI** extension plus `dotnet workload install maui-android`, the Android SDK (API 36) and a JDK.
- For the E2E tests: Node.js 20.19+, 22.12+ or 24+, and `ANDROID_HOME` and `JAVA_HOME` ([E2E tests](#e2e-tests)).
- Git and the GitHub CLI (`gh`).

The stories need only the .NET SDK.

## Stories (design system)
`dotnet run --project src/WorkoutLog.UI.Stories`, then open http://localhost:5056. Story files are `*.stories.razor` in `src/WorkoutLog.UI.Stories/Stories/`. `main` is published at https://marty-shields.github.io/workout-log-app/.

### README screenshots
`README.md` shows screen stories, light and dark, from `docs/images/readme/`. To retake one, run the stories, then with Playwright set the colour scheme and open `http://localhost:5056/iframe.html?id=<story id>&viewMode=story`. Screenshot the `.phone` element with a transparent background to `<file>-light.png` and `<file>-dark.png`.

| File | Story id |
|---|---|
| `dashboard` | `screens-dashboard--with-workouts` |
| `new-workout` | `screens-new-workout--strength` |
| `cardio` | `screens-new-workout--cardio` |
| `add-exercise` | `screens-add-exercise--results` |
| `workouts` | `screens-workouts--with-workouts` |
| `settings` | `screens-settings--with-workouts` |
| `exercise-lift` | `screens-exercise--lift-top-set` |
| `exercise-cardio` | `screens-exercise--cardio-distance` |

`icon.svg` is the app icon's foreground on its background with rounded corners; rebuild it when the icon changes.

## Phone setup (Android 16)
1. Settings → About phone → tap **Build number** 7 times to enable Developer options.
2. Developer options → turn on **USB debugging**, connect by USB and accept the RSA prompt. `adb devices` should list the phone.

Without USB, use wireless debugging (same Wi-Fi): turn on **Wireless debugging**, **Pair device with pairing code**, run `adb pair <ip>:<pairing port> <code>`, then `adb connect <ip>:<port>` with the port on the main Wireless debugging screen. The port changes when wireless debugging restarts.

## Development: run on the phone
Pick the phone as the target in Visual Studio or VS Code, or run `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android`. Debug builds embed their assemblies, so the APK runs on its own.

## Reading the logs
- **Logcat:** `adb logcat -s WorkoutLog` (add `-d` to print and stop).
- **File:** `adb pull /sdcard/Android/data/<package id>/files/logs/`. It holds `workoutlog.log` and, past 1 MB, `workoutlog.1.log`. Without external storage it's in the app data folder; only the Debug app's can be read: `adb shell run-as uk.martinshields.workoutlog.debug cat files/logs/workoutlog.log`.

## E2E tests
### One-time setup
1. Set the user environment variables, then restart the terminal and IDE:
   - `ANDROID_HOME` = the Android SDK folder (holds `platform-tools\adb.exe`).
   - `JAVA_HOME` = the JDK folder (holds `bin\java.exe`).
2. In `tests/WorkoutLog.E2E.Tests`, run `npm ci --ignore-scripts` to install Appium and its driver. Run it again after either version changes.
3. Phone: USB or wireless debugging on ([Phone setup](#phone-setup-android-16)).

### Each run
1. Connect the device and unlock it; keep it unlocked (Developer options → **Stay awake** helps). A locked device fails each test.
2. Build the App: `dotnet build src/WorkoutLog.App -f net10.0-android`. The tests install this APK; they don't build it. Build and test from the same commit.
3. Run the tests: every scenario, the smoke scenarios, one story's or one feature's:
   ```
   dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~WorkoutLog.E2E.Tests"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=US-17"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~EditWorkoutFeature"
   ```
   With more than one device connected, add `-- 'TestRunParameters.Parameter(name="DeviceUdid", value="<id from adb devices>")'`.

- Every scenario deletes the workouts in "Workout Log (Dev)"; the Release app is never touched.
- A `@today` scenario fails within 5 minutes of midnight on the device; run it again after midnight.
- A plain `dotnet test` skips the E2E tests (`[Explicit]`).
- After a killed run: stop the `node` process running `node_modules\appium\index.js`, turn auto-rotate back on (`adb shell settings put system accelerometer_rotation 1`), and remove Appium's helper apps (`adb uninstall` `io.appium.uiautomator2.server`, `io.appium.uiautomator2.server.test`, `io.appium.settings`).

### Results
- `tests/WorkoutLog.E2E.Tests/TestResults/E2E/<yyyyMMdd-HHmmss>/` per run. A failed scenario saves its screen, page source, app log and Appium log in its own folder there.
- A phone run's files can show notifications, the phone's serial number and what the tests typed. Keep them local; turn on Do Not Disturb during a run.

### Use an emulator
The Debug APK also runs on an x86_64 emulator.
1. Turn on **Windows Hypervisor Platform** (Windows Features; admin).
2. From `%ANDROID_HOME%\cmdline-tools\latest\bin` in an admin terminal:
   ```
   sdkmanager "emulator" "system-images;android-36;google_apis;x86_64"
   avdmanager create avd -n WorkoutLog_API36 -k "system-images;android-36;google_apis;x86_64"
   ```
3. Start it: `%ANDROID_HOME%\emulator\emulator.exe -avd WorkoutLog_API36`. It shows as `emulator-5554`.
4. Run the tests as above, with `DeviceUdid` = `emulator-5554` if the phone is connected too.

## CI
Every PR to `main` runs these; the four checks must pass before it can merge:
- **PR build** (`pr-build.yml`): the dev pipeline (`dev-pipeline.yml`) builds, runs the Core and UI tests and checks `allowBackup` in the Debug and Release APKs (`dev / build-and-test`, `dev / backup-manifest`). Then the QA pipeline (`qa-pipeline.yml`) runs the E2E tests on emulators, sharded by feature, retrying failed scenarios up to 3 tries (`qa / emulator-e2e-tests`).
- **PR checks** (`pr-checks.yml`): a Conventional Commit title and exactly one `semver:major`, `semver:minor` or `semver:patch` label (`pr-checks`).

Keep in step:
- When the SDK in `global.json` changes, set the workflow's workload set `--version` to match (`dotnet workload search version`) in the same PR.
- Chromedriver is pinned in `qa-pipeline.yml` (`CHROMEDRIVER_VERSION`, `CHROMEDRIVER_SHA256`, from [Chrome for Testing](https://googlechromelabs.github.io/chrome-for-testing/)). When the emulator's WebView major version changes, the job fails: pin the matching version and its SHA-256.

Also: Dependabot opens weekly update PRs (`chore(deps): …`, `semver:patch`); every push to `main` publishes the stories to GitHub Pages (`stories-pages.yml`). Actions are GitHub-owned only, pinned by commit SHA.

## Release: signed APK
Run Actions → Release → **Run workflow** on `main`, then approve the `release` environment. `release.yml`:
1. Works out the version from the highest `semver:*` label among the PRs merged since the last `vX.Y.Z` tag (`1.0.0` for the first). No PRs ends it green with nothing released.
2. Runs the dev pipeline, the QA pipeline and the release smoke (the Release APK starts cleanly on an emulator).
3. After approval, builds the arm64 Release APK signed with the release keystore, checks it (`check-apk.sh`, `check-backup.sh`) and creates the release `vX.Y.Z`: the APK, its `.sha256`, and notes listing the merged PRs.

A failed run creates no tag or release: fix the cause and run it again.

Settings outside the repo: the `semver:*` labels; the `release` environment (owner is the required reviewer, admins can't bypass, `main` only) with secrets `RELEASE_KEYSTORE_BASE64`, `RELEASE_KEYSTORE_PASSWORD` and `RELEASE_KEY_ALIAS`; the ruleset `Protect release tags`, so `v*` tags can't be deleted or moved.

### Install or update on the phone
See the README's [Install](../README.md#install). An update installs over the app and keeps its data.

**The same keystore must sign every future APK.** If it's lost, updates can't be installed over the existing app; the only way back is to uninstall, which wipes all workout data.

## Keystore handling
- PKCS12, RSA 4096, valid 10000 days, one password for the store and the key.
- Release cert SHA-256: `5D:6A:C3:4B:86:B8:29:8D:04:DE:18:5C:9B:D8:3C:27:C8:31:76:CB:F9:BD:F8:35:A8:CA:C4:95:57:1B:CD:AC`. It is also `CERT_SHA256` in `release.yml`.
- The keystore file lives outside the repo and is gitignored. CI gets it only through the `release` environment's secrets (base64 for the file) and deletes it after the build.
- The owner keeps one backup of the keystore file with its passwords and alias, in a private location that is not recorded in the repo.
- Local Release builds are signed with the debug key.

## Source control
- GitHub: [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app), public, default branch `main`.
- `main` is protected by the `Protect main` ruleset: no direct pushes, force pushes or deletion; changes go through a PR with the [CI](#ci) checks passing. PRs are opened as drafts; the reviewer marks them ready and squash-merges. Merged branches are deleted.
- Branches: `type/us-xx-slug` (e.g. `feat/us-26-signed-release-apk`); work that isn't a story leaves out the ID (e.g. `chore/update-readme`).
