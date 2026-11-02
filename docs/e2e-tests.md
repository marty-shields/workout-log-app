# E2E tests
Running the E2E suite on a phone or an emulator, and where the results go. Set up first: [Windows](dev-setup-windows.md#e2e-one-time-setup) or [Linux](dev-setup-linux.md#setup). On Linux, `e2e-local.sh` runs the tests on a throwaway emulator ([details](dev-setup-linux.md#throwaway-emulator-for-agents)). CI runs them on emulators ([CI](dev-setup-build.md#ci)).

## Each run
1. Connect the device and unlock it; keep it unlocked (Developer options → **Stay awake** helps). A locked device fails each test.
2. Build the App: `dotnet build src/WorkoutLog.App -f net10.0-android`. The tests install this APK; they don't build it. Build and test from the same commit.
3. Run the tests: every scenario the Debug app runs (all but `@upgrade` and `@backup`, see [Tags](#tags)), the smoke scenarios, one feature's:
   ```
   dotnet test tests/WorkoutLog.E2E.Tests --filter "Category!=upgrade&Category!=backup"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"
   dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~.EditAWorkoutFeature."
   ```
   A feature's class is its `Feature:` title in PascalCase plus `Feature`: `Feature: Edit a workout` is `EditAWorkoutFeature`. Keep the dots around it: `~` matches any part of the name, and `EditWorkoutFeature` would match `MarkAnExerciseDoneOnEditWorkoutFeature` instead.
   Every `dotnet test` command above also needs `-- 'TestRunParameters.Parameter(name="ChromedriverExecutable", value="<path>")'` (combine it with the other run settings in the same `--`). Without it the run fails: Appium's own Chromedriver download is not allowed, because it fetches an unpinned binary. Download the Chromedriver at `CHROMEDRIVER_VERSION` in `.github/workflows/qa-pipeline.yml` from [Chrome for Testing](https://googlechromelabs.github.io/chrome-for-testing/), check it against `CHROMEDRIVER_SHA256` there, and pass its path. It must match the device's WebView major version. `e2e-local.sh` does all of this for you.
   With more than one device connected, add `-- 'TestRunParameters.Parameter(name="DeviceUdid", value="<id from adb devices>")'`.

- Every scenario deletes the workouts of the app under test, "Workout Log (Dev)" by default. Another build runs only on an emulator, so the app on a phone is never touched.
- A `@today` scenario fails within 5 minutes of midnight on the device; run it again after midnight.
- A plain `dotnet test` skips the E2E tests (`[Explicit]`).
- After a killed run: stop the `node` process running Appium (`node_modules/appium/index.js`), turn auto-rotate back on (`adb shell settings put system accelerometer_rotation 1`), and remove Appium's helper apps (`adb uninstall` `io.appium.uiautomator2.server`, `io.appium.uiautomator2.server.test`, `io.appium.settings`).

## Tags
- `@smoke`: a quick check of the main paths.
- `@today`: checks today's date (see above).
- `@release`: also runs on the Release build. These are the `@smoke` scenarios that don't seed, plus Export and Import, the code a trimmed build is most likely to break.
- `@upgrade` and `@backup`: install other builds, so the shards leave them out and they run in their own workflows (below).

## Other builds
Run settings choose the build under test. The Debug app is the default.
- `AppPackage` and `AppApk`: the app's package and its APK. A package other than `uk.martinshields.workoutlog.debug` runs only on an emulator (`emulator-*`), so the app on a phone is never touched. The run fails before it installs anything when `AppApk` or `PreviousApk` holds another package than `AppPackage` (`aapt2 dump packagename`, from the newest build-tools in `ANDROID_HOME`), so the rule covers the app that is really installed. The Release app has no run-as, so the tests read its files as root, which emulator images allow. They seed it by importing an export file instead of copying a database in, so a seeded workout can't be in the future.
- The Release app keeps WebView debugging off (`AddBlazorWebViewDeveloperTools` is Debug only), and the tests drive its screens through the WebView's DevTools socket. Only a `userdebug` or `eng` image opens that socket for every app, so a Release run needs one, such as the AOSP image `emulator-start.sh` installs. On another image the run fails at the start. If the app shows a WebView with no DevTools socket (a new image or WebView that stopped doing this), the launch fails with that reason and so does every later launch in the run, without waiting.
- `PreviousApk`: the last release's Debug APK, for `@upgrade`. `.github/scripts/previous-apk.sh <out apk>` builds it from the latest `v*` tag. Build it on the machine that builds the App: one APK installs over another only when the same debug key signs both.

`e2e-run.sh` (and so `e2e-local.sh`) passes these from `E2E_APP_PACKAGE`, `E2E_APP_APK` and `E2E_PREVIOUS_APK`:
```
dotnet restore src/WorkoutLog.App
dotnet publish src/WorkoutLog.App -c Release -f net10.0-android -p:AndroidPackageFormat=apk -r android-x64 --no-restore -o <folder>
E2E_APP_PACKAGE=uk.martinshields.workoutlog E2E_APP_APK=<folder>/uk.martinshields.workoutlog-Signed.apk \
  .github/scripts/e2e-local.sh "Category=release|Category=backup"

.github/scripts/previous-apk.sh <file>
E2E_PREVIOUS_APK=<file> .github/scripts/e2e-local.sh "Category=upgrade"
```

The scenarios that need them:
- **Release build** (`release-e2e.yml`): the x64 Release APK, signed with the debug key and never published, starts cleanly (`release-smoke.sh`), then the `@release` and `@backup` scenarios run on it.
- **Upgrade** (`@upgrade`, `upgrade-e2e.yml`): installs the last release's build, saves workouts through its screens, installs this build over it with `adb install -r`, then checks the workouts, an edit and save, and the Exercise page.
- **Backup and restore** (`@backup`, Release app only): saves workouts, backs the app up with `bmgr backupnow` on Android's local transport, then uninstalls and installs it again (Android's "Test backup and restore" steps). No `bmgr restore` runs: Android restores the backup when it installs the app, as on a new phone. The scenario checks that the install restored the database, the workouts came back and no log line did. It leaves the emulator on the local transport.

## Results
- `tests/WorkoutLog.E2E.Tests/TestResults/E2E/<yyyyMMdd-HHmmss>/` per run. A failed scenario saves its screen, page source, app log and Appium log in its own folder there.
- A phone run's files can show notifications, the phone's serial number and what the tests typed. Keep them local; turn on Do Not Disturb during a run.
