# 06 — Dev Setup & Build

## App identity
- Display name: **Workout Log**
- Android package id: **`uk.martinshields.workoutlog`**. This can't change later without a reinstall, which wipes all data.
- Debug builds: package id **`uk.martinshields.workoutlog.debug`**, name **Workout Log (Dev)**. They install alongside the release app with their own data, so USB debugging never touches real workouts.
- Version: display version is SemVer (`1.0.0`, `1.1.0`…); the version code is the number of `v*` tags + 1. The [release workflow](#release-signed-apk) sets both. The `.csproj` sets neither, so local builds are `1.0.0` (1).

## Prerequisites (Windows)
- .NET 10 SDK: the version in `global.json` or a later patch (D165)
- One of (D157):
  - **Visual Studio** with the **.NET Multi-platform App UI development** workload. It installs the MAUI workloads, the Android SDK (`Android\android-sdk` under Program Files (x86)) and OpenJDK.
  - **VS Code** with the **.NET MAUI** extension (brings C# Dev Kit), plus `dotnet workload install maui-android`, the Android SDK (platform API 36, build tools, platform tools) and the JDK the .NET 10 Android workload needs.
- Optional: an Android emulator for API 36, to run the E2E tests without the phone ([Use an emulator](#use-an-emulator)). Creating one needs admin rights when the SDK is in Program Files.
- For the E2E tests: Node.js 20.19+, 22.12+ or 24+, and the `ANDROID_HOME` and `JAVA_HOME` environment variables ([E2E tests](#e2e-tests)). Appium and its driver come from the repo; Chromedriver downloads itself.
- Git, and the GitHub CLI (`gh`) signed in as the repo owner ([Source control](#source-control))

The stories need only the .NET 10 SDK. They don't need the MAUI workload, Android SDK or phone.

## Stories (design system)
- `dotnet run --project src/WorkoutLog.UI.Stories`, then open http://localhost:5056
- Story files are `*.stories.razor` in `src/WorkoutLog.UI.Stories/Stories/`.
- `main` is published at https://marty-shields.github.io/workout-log-app/ ([CI](#ci)).

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
- Debug builds install as "Workout Log (Dev)". They embed their assemblies in the APK (no Fast Deployment), so the APK runs on its own (D143).

## Reading the logs
Debug and Release builds both log to logcat and to a file ([04](04-architecture.md#logging)).
- **Logcat** (phone connected): `adb logcat -s WorkoutLog`. Add `-d` to print what's there and stop.
- **File:** `adb pull /sdcard/Android/data/<package id>/files/logs/`, with the package id from [App identity](#app-identity). The folder holds `workoutlog.log` and, after it reaches 1 MB, the older `workoutlog.1.log`. Uninstalling the app deletes it.
- If the phone has no external storage, the file is in the app data folder instead. Only the Debug app's can be read: `adb shell run-as uk.martinshields.workoutlog.debug cat files/logs/workoutlog.log`.
- Each line: UTC time (file only), level, category, `[EventId]`, message, then any exception.

## E2E tests
`WorkoutLog.E2E.Tests` drives the Debug app on a device through Appium ([05](05-testing.md#e2e-tests-appium)).

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
   With more than one device connected, name one: add `-- 'TestRunParameters.Parameter(name="DeviceUdid", value="<id from adb devices>")'`.

What a run does:
- Starts Appium from `node_modules` on a free port and stops it at the end.
- Installs the APK over the Debug app.
- Before each scenario, clears the Debug app's data, then launches the app once: before the first step, or after seeding when the scenario starts from saved workouts (D142). Any workouts in "Workout Log (Dev)" are deleted; the Release app is never touched.
- A scenario that starts from saved workouts builds a database on the PC with Core and copies it into the Debug app with `run-as`, which needs the debuggable Debug build (D145). Build the App and run the tests from the same commit: a database from a newer Core can stop the app starting.
- A `@today` scenario fails at its start within 5 minutes of midnight on the device; run it again after midnight (D153).
- At the end, closes the Debug app. It stays installed with the last test's data, so a failure can be checked by hand.
- Downloads a Chromedriver that matches the device's WebView into `tests/WorkoutLog.E2E.Tests/.chromedriver/` when needed, unless the run setting `ChromedriverExecutable` names one.
- Turns auto-rotate off and holds the screen in portrait, then puts both rotation settings back at the end (D149). After a killed run, set auto-rotate back yourself: `adb shell settings put system accelerometer_rotation 1` (on) or `0` (off).
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

### QA pipeline
`.github/workflows/qa-pipeline.yml` ("QA pipeline") runs all the E2E tests on emulators on every PR to `main` once the [dev pipeline](#ci) passes, and in the [release workflow](#release-signed-apk) (D167, D165).
- Its `shard` matrix runs one job per shard, each with one API 36 emulator (`emulator-start.sh`, `emulator-wait.sh`). The script sets the AVD and the AOSP system image.
- Its `emulator-e2e-tests` job passes only when every shard passes. It is a required check on PRs, as `qa / emulator-e2e-tests` ([Source control](#source-control)).
- `e2e-shard.sh` splits the scenarios by whole feature, the largest first, each to the job with the fewest scenarios. A job that gets no features fails.
- Each job builds the Debug App and runs its features with `DeviceUdid` = `emulator-5554` and the run setting `ChromedriverExecutable` (`e2e-run.sh`). Failed scenarios show as annotations and each run adds a results table to the run's summary.
- Only the scenarios that failed run again, up to 3 tries in all; the job fails if one still fails. A scenario that passes on a retry gets a warning annotation, and its failed try stays in the summary (D155). Local runs don't retry.
- Chromedriver is pinned in `qa-pipeline.yml` (`CHROMEDRIVER_VERSION`, `CHROMEDRIVER_SHA256`, from [Chrome for Testing](https://googlechromelabs.github.io/chrome-for-testing/)). The job fails when the emulator's WebView major version differs: pin a Chromedriver of that major version and its zip's SHA-256. Local runs still download their own.
- Each job keeps `TestResults/E2E/` (with logcat, a health log and the emulator log) as the artifact `e2e-results-<n>`, for 1 day. The Debug APK is never uploaded: it is debuggable and has WebView debugging on.
- In the release only, a `release-smoke` job in `release.yml` builds the Release APK for x64 with the debug key, installs it on an emulator and launches it (`release-smoke.sh`). It fails unless the app's log shows the Release start line, the exercise list seeded and "Startup finished", with no error and no crash.

## CI
- `.github/workflows/pr-build.yml` ("PR build") runs on every PR to `main`: its `dev` job runs `.github/workflows/dev-pipeline.yml` ("Dev pipeline"), then its `qa` job runs the [QA pipeline](#qa-pipeline) if the dev pipeline passes (D165). The dev pipeline, on `ubuntu-latest`, installs the exact SDK in `global.json` (D165) and the `maui-android` workload at a pinned workload set, runs `dotnet build WorkoutLog.slnx`, then the Core and UI tests. Its `dev / build-and-test` check must pass before the PR can merge.
- `check-backup.sh` fails unless an APK's `allowBackup` has the expected value (D05). `build-and-test` checks `false` in the Debug APK. The dev pipeline's `backup-manifest` job builds the Release APK (x64, debug key, never published) and checks `true`; its `dev / backup-manifest` check must pass before the PR can merge.
- Failed tests show as annotations on the PR, linked to the test's source line, and each test step adds a results table to the run's summary (D165).
- The workload set matches the SDK: when the SDK in `global.json` changes, set the workflow's `--version` to the matching set (`dotnet workload search version`) in the same PR.
- `.github/workflows/pr-checks.yml` ("PR checks") runs on every PR to `main` when it opens, changes or is relabelled. Its `pr-checks` check fails unless:
  - the title is a Conventional Commit: `type(scope)!: description`, type one of `build`, `chore`, `ci`, `docs`, `feat`, `fix`, `perf`, `refactor`, `revert`, `style`, `test`; scope and `!` optional
  - the PR has exactly one of the labels `semver:major`, `semver:minor`, `semver:patch`. The [release](#release-signed-apk) bumps the version by the highest label among its PRs.
- `.github/dependabot.yml` opens update PRs weekly for NuGet, the E2E project's npm packages, the SDK in `global.json` and the GitHub Actions (D168). Minor and patch bumps come grouped, one PR per ecosystem; each major bump has its own PR. Each PR is labelled `semver:patch` and titled `chore(deps): …`. The PR build and checks run on them like any other PR.
- `.github/workflows/stories-pages.yml` ("Stories site") runs on every push to `main`, or by hand: it publishes the stories in Release, sets `<base href>` to the Pages base path, and deploys the site to GitHub Pages through the `github-pages` environment (D177). The repo's Pages source is GitHub Actions.
- Repo Actions settings: only GitHub-owned actions, pinned by full commit SHA; workflows from outside contributors' PRs wait for approval (D165).

## Release: signed APK
`.github/workflows/release.yml` ("Release") builds, tests, signs and publishes from `main`. Run it from Actions → Release → **Run workflow** on `main`.

- `version`: fails off `main`. Works out the next version from the highest `semver:*` label among the PRs merged since the last `vX.Y.Z` tag, or `1.0.0` for the first (`next-version.sh`). Version code: number of `v*` tags + 1. A PR without exactly one label fails the run; no PRs ends it green with nothing released.
- `build`: the [dev pipeline](#ci).
- `e2e`, `release-smoke`: the [QA pipeline](#qa-pipeline) and the release smoke.
- `release`: waits for the owner to approve the `release` environment, builds the arm64 Release APK signed with the release keystore, checks it (`check-apk.sh`: the pinned cert `CERT_SHA256`, version, not debuggable, arm64 only; `check-backup.sh`: `allowBackup` `true`), then creates the release `vX.Y.Z` and its tag on the run's commit.

A failed run creates no tag or release: fix the cause and run it again. The release holds `WorkoutLog-X.Y.Z.apk`, its `.sha256`, and notes listing the merged PRs under the signing cert's fingerprint.

Settings outside the repo (D172, D160, D166):
- Labels `semver:major`, `semver:minor`, `semver:patch`.
- Environment `release`: the owner is the required reviewer, admins can't bypass, `main` only. Secrets `RELEASE_KEYSTORE_BASE64`, `RELEASE_KEYSTORE_PASSWORD` (store and key), `RELEASE_KEY_ALIAS`.
- Ruleset `Protect release tags`: `v*` tags can't be deleted or moved, with no bypass, so the version code only goes up.

### Install or update on the phone
See the README's [Install](../README.md#install). An update installs over the app and keeps its data.

**The same keystore must sign every future APK.** If it's lost, updates can't be installed over the existing app. The only way back is to uninstall, which wipes all workout data.

## Keystore handling
- PKCS12, RSA 4096, valid 10000 days, one password for the store and the key (D171).
- Release cert SHA-256: `5D:6A:C3:4B:86:B8:29:8D:04:DE:18:5C:9B:D8:3C:27:C8:31:76:CB:F9:BD:F8:35:A8:CA:C4:95:57:1B:CD:AC`. It is also `CERT_SHA256` in `release.yml`.
- The keystore file lives outside the repo and is listed in `.gitignore`. CI gets it only through the `release` environment's secrets; the owner sets them, base64 for the file. The workflow writes it to the runner's temp folder and deletes it after the build.
- The owner keeps one backup of the keystore file with its passwords and alias, in a private location that is not recorded in the repo (D176).
- Local Release builds are signed with the debug key.

## Source control
- GitHub: [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app), public, default branch `main` (D161).
- The repo's own git config sets the author (`user.name`, `user.email`) and `credential.username`. The remote URL includes the username, so Git Credential Manager signs in as the owner in this repo only.
- `gh` reads its login from the folder in `GH_CONFIG_DIR`, which holds only the owner's account. `.claude/settings.local.json` sets it for Claude Code and `.vscode/settings.json` for VS Code terminals; both are local and gitignored. Sign in once with `$env:GH_CONFIG_DIR="$HOME\.config\gh-personal"; gh auth login --web --scopes project,workflow`, and answer No to authenticating Git.
- `main` is protected by the `Protect main` ruleset: no direct pushes, force pushes or deletion. Changes go through a PR, with no required approvals, and the `dev / build-and-test`, `dev / backup-manifest`, `pr-checks` and `qa / emulator-e2e-tests` checks must pass. PRs are opened as drafts; the reviewer marks them ready and squash-merges. The PR title becomes the commit title; merged branches are deleted.
- Branches: `type/us-xx-slug`, the Conventional Commit type and the story ID (e.g. `feat/us-26-signed-release-apk`). Work that isn't a story leaves out the ID (e.g. `chore/update-readme`).
- `.gitignore` keeps build output, logs, test results, per-user files, APKs, signing keys and secrets out of the repo (D162).
- `nuget.config` restores from nuget.org only (D129).
