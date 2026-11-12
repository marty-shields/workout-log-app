# E2E tests
Running the E2E suite on a phone or an emulator, and where the results go. Set up first: [Windows](dev-setup-windows.md#e2e-one-time-setup) or [Linux](dev-setup-linux.md#setup). On Linux, `e2e-local.sh` runs the tests on a throwaway emulator ([details](dev-setup-linux.md#throwaway-emulator-for-agents)). CI runs them on emulators ([CI](dev-setup-build.md#ci)).

## Each run
1. Connect the device and unlock it; keep it unlocked (Developer options → **Stay awake** helps). A locked device fails each test.
2. Build the App: `dotnet build src/WorkoutLog.App -f net11.0-android`. The tests install this APK; they don't build it. Build and test from the same commit.
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
- `@release`: also runs on the Release build. These are the `@smoke` scenarios that don't seed, plus Export and Import, the code a trimmed build is most likely to break, and the upgrade seed.
- `@upgrade` and `@backup`: install other builds, so the shards leave them out and they run in their own workflows (below).
- `@upgrade-seed`: the first half of the next release's upgrade check ([Upgrade](#the-upgrade-check)). The shards and the Release E2E run it like any other scenario, so every release proves its own seed works.

## Other builds
Run settings choose the build under test. The Debug app is the default.
- `AppPackage` and `AppApk`: the app's package and its APK. A package other than `uk.martinshields.workoutlog.debug` runs only on an emulator (`emulator-*`), so the app on a phone is never touched. The run fails before it installs anything when `AppApk` or `PreviousApk` holds another package than `AppPackage` (`aapt2 dump packagename`, from the newest build-tools in `ANDROID_HOME`), so the rule covers the app that is really installed. The Release app has no run-as, so the tests read its files as root, which emulator images allow. They seed it by importing an export file instead of copying a database in, so a seeded workout can't be in the future.
- The Release app keeps WebView debugging off (`AddBlazorWebViewDeveloperTools` is Debug only), and the tests drive its screens through the WebView's DevTools socket. Only a `userdebug` or `eng` image opens that socket for every app, so a Release run needs one, such as the AOSP image `emulator-start.sh` installs. On another image the run fails at the start. If the app shows a WebView with no DevTools socket (a new image or WebView that stopped doing this), the launch fails with that reason and so does every later launch in the run, without waiting.
- `PreviousApk`: the last release's APK of the same app, for `@upgrade`. `.github/scripts/previous-apk.sh <out apk> [Debug|Release]` builds it from the latest `v*` tag: the Debug app by default, or the x64 Release app signed with the debug key. Build it on the machine that builds the App, or with the same debug keystore: one APK installs over another only when the same key signs both. With it set, the run installs nothing at the start: `upgrade-e2e.sh` has installed this APK with its workouts ([Upgrade](#the-upgrade-check)).

`e2e-run.sh` (and so `e2e-local.sh`) passes these from `E2E_APP_PACKAGE`, `E2E_APP_APK` and `E2E_PREVIOUS_APK`. With `E2E_PREVIOUS_APK` set, `e2e-local.sh` runs `upgrade-e2e.sh` instead, and the filter must be `Category=upgrade`:
```
dotnet restore src/WorkoutLog.App -p:Configuration=Release
dotnet publish src/WorkoutLog.App -c Release -f net11.0-android -p:AndroidPackageFormat=apk -r android-x64 --no-restore -o <folder>
E2E_APP_PACKAGE=uk.martinshields.workoutlog E2E_APP_APK=<folder>/uk.martinshields.workoutlog-Signed.apk \
  .github/scripts/e2e-local.sh "Category=release|Category=backup"

.github/scripts/previous-apk.sh <file>
E2E_PREVIOUS_APK=<file> .github/scripts/e2e-local.sh "Category=upgrade"

.github/scripts/previous-apk.sh <release file> Release
E2E_APP_PACKAGE=uk.martinshields.workoutlog E2E_APP_APK=<folder>/uk.martinshields.workoutlog-Signed.apk \
  E2E_PREVIOUS_APK=<release file> .github/scripts/e2e-local.sh "Category=upgrade"
```

The scenarios that need them:
- **Release build** (`release-e2e.yml`): one job builds the x64 Release APK, signed with the debug key and never published. On one emulator it starts cleanly (`release-smoke.sh`), then the `@release` and `@backup` scenarios run on it. On another, the upgrade check installs it over the last release's x64 Release APK, which `previous-apk.sh` builds with the same debug keystore, passed from the build job.
- **Upgrade** (`@upgrade`): the Debug app in `upgrade-e2e.yml` and the Release app in `release-e2e.yml`. See [The upgrade check](#the-upgrade-check).
- **Backup and restore** (`@backup`, Release app only): saves workouts, backs the app up with `bmgr backupnow` on Android's local transport, then uninstalls and installs it again (Android's "Test backup and restore" steps). No `bmgr restore` runs: Android restores the backup when it installs the app, as on a new phone. The scenario checks that the install restored the database, the workouts came back and no log line did. It leaves the emulator on the local transport.

## The upgrade check
`upgrade-e2e.sh <previous apk> <tries>` runs it in two halves, so this commit's steps never drive the last release's screens. A change to a screen's selectors can't fail it.
1. The last release saves the workouts. `upgrade-e2e.sh` uninstalls the app, builds the E2E project of the latest `v*` tag, and runs that project's `@upgrade-seed` scenario (`UpgradeSeed.feature`) on the previous APK with its own steps and Appium. The scenario saves two workouts through the last release's screens and leaves the app installed with them.
2. This build checks them. This commit's `@upgrade` scenario (`Upgrade.feature`) runs with `PreviousApk` set. It checks that the installed APK is `PreviousApk` (same SHA-256), installs this build over it with `adb install -r`, then checks the workouts, an edit and save, and the Exercise page.

The device state between the halves is the contract. The tag's project defines what the first half saves, and the current one checks it:
- The app is installed from `PreviousApk`, under `AppPackage` (`E2E_APP_PACKAGE`), and has not been cleared.
- It holds exactly two workouts, saved today. Workout 1 is Barbell Back Squat, 5 reps 60 kg and 5 reps 62.5 kg. Workout 2 is Treadmill 0:20:00, 3.2 km, incline 1.5, then Farmer's Walk 40 m, 32 kg, 0:00:45.
- The first half takes the run settings `AppPackage`, `AppApk` (the previous APK), `DeviceUdid` and `ChromedriverExecutable` and the `@upgrade-seed` category. Keep all of these, since a later release's check runs an older project with them.

A failed try starts both halves again from an uninstalled app. The tag `v1.3.0` and any older one have no `UpgradeSeed.feature`. For such a tag, the first half runs the E2E project of the commit that added the feature instead (`upgrade-seed-ref.sh` picks it). That commit's CI ran it against that release's build, and the commit never changes. `upgrade-seed-ref.sh` fails if a tag that contains that commit has no seed scenario.

## A red emulator shard
A failed shard's job summary says `infra` or `test`. `classify-shard-failure.sh <log>` decides it from the shard's output (`e2e-output.log` in the results artifact):
- `infra`: the log shows the emulator or its UiAutomator2 server failing, and no assertion failing. The signatures are `Could not find a connected Android device`, `adb: device offline`, `instrumentation process is not running`, `adb shell date +%s failed (255)`, `WEBVIEW_<package> didn't appear`, `The emulator did not recover`, and `An ANR dialog for <package>`, the warning `emulator-recover.sh` prints when it closes a system app's "isn't responding" dialog before a try. The app's own ANR dialog doesn't count.
- `test`: anything else. A log with an assertion failure (an NUnit `Expected:` line) is `test` even when it also shows a signature, so a real failure is never called infrastructure. That holds for the ANR signature too: the dialog is closed before every try, so an assertion that fails on the last try failed on a clear screen. A UI timeout alone is `test`.

The rule:
1. On `infra`, rerun the failed job once. GitHub keeps the first attempt's logs and summary in the run's attempt list.
2. If the rerun fails, or the verdict was `test`, the failure is real. Don't rerun again: find the cause. A second `infra` verdict does not license a third run, and a signature that keeps coming back is a bug in the emulator setup or the app to fix.
3. The `infra` verdict is a heuristic. `WEBVIEW didn't appear` can also come from an app that fails to start, so one rerun is the limit.

Between tries, `e2e-run.sh` also checks the emulator (`emulator-recover.sh`). A device that doesn't answer gets `adb reconnect`, then a restart of the emulator with the job's flags. Before a retry, the UiAutomator2 server is stopped and uninstalled, so Appium installs a fresh one. If the device still doesn't answer, the job fails with an `Emulator` error instead of running the retry on a dead device.

Before every try, the first one too, it closes any "isn't responding" (ANR) or "has stopped" dialog on the screen (`emulator-dialogs.sh`). Such a dialog takes Android back, covers the file picker and the share sheet and keeps the keyboard down, so every try behind it fails the same way. It taps the dialog's Wait button, else force-stops the app it names, else broadcasts `CLOSE_SYSTEM_DIALOGS`, and prints an `Emulator` warning naming the app (`An ANR dialog for com.android.launcher3 was on the screen; closed it by tapping Wait.`).

Most of those dialogs came from the boot. A guest still busy just after `sys.boot_completed` can leave the launcher without a focused window, and a key sent then makes it "not respond". So `emulator-wait.sh` hides error dialogs (`hide_error_dialogs`) before anything else, sends no key, waits up to 2 minutes for a window to have focus before `wm dismiss-keyguard`, and closes a dialog that is already up.

A failed or cancelled job's results artifact is kept 7 days, a passed one's 1 day. Each attempt uploads its own, named after the job's artifact with the attempt number added (`e2e-results-3-2` is shard 3's rerun).

## Screenshots of a screen state
A review sometimes needs a screenshot of a state the existing scenarios do not leave on screen, such as a screen with seeded data or dark mode. Take it from a throwaway scenario in a detached copy of the branch, never in the working tree under review:
1. `git worktree add --detach <dir> <commit>` and work only in `<dir>`. Delete it afterwards with `git worktree remove --force <dir>`. Never commit the scenario.
2. Add a feature file and a small step file under `tests/WorkoutLog.E2E.Tests/Features` and `Steps` in the copy. Seed data with the existing step `Given these saved workouts:` (a table with `started` and `exercises` columns; write times as the steps do, like "10 days ago 07:30" or "yesterday 18:05", and see `Seeding/SeedTime.cs` and `Features/Workouts.feature` for the cell format). Reuse the existing steps in `Steps/` to open the screen, such as `I open Workouts`, instead of writing new ones.
3. Capture the screen inside the scenario, while the emulator is up: `adb -s emulator-5554 exec-out screencap -p > <file>`, or the Appium driver's `GetScreenshot()` as `Hooks/AppHooks.cs` does for a failed scenario. `e2e-local.sh` stops the emulator when the run ends, so there is nothing to capture afterwards. For dark mode run `adb -s emulator-5554 shell cmd uimode night yes` (or `no` for light) and wait a few seconds before capturing. Save the images outside the repo.
4. Run only that scenario from the copy: `.github/scripts/e2e-local.sh "FullyQualifiedName~<FeatureName>"`. In an agent's shell, first run `adb start-server` in its own command (see [Throwaway emulator for agents](dev-setup-linux.md#throwaway-emulator-for-agents)).

## Results
- `tests/WorkoutLog.E2E.Tests/TestResults/E2E/<yyyyMMdd-HHmmss>/` per run. A failed scenario saves its screen, page source, app log and Appium log in its own folder there.
- A phone run's files can show notifications, the phone's serial number and what the tests typed. Keep them local; turn on Do Not Disturb during a run.
