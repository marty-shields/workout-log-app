# E2E tests
Running the E2E suite on a phone or an emulator, and where the results go. Set up first: [Windows](dev-setup-windows.md#e2e-one-time-setup) or [Linux](dev-setup-linux.md#setup). On Linux, `e2e-local.sh` runs the tests on a throwaway emulator ([details](dev-setup-linux.md#throwaway-emulator-for-agents)). CI runs them on emulators ([CI](dev-setup-build.md#ci)).

## Each run
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
- After a killed run: stop the `node` process running Appium (`node_modules/appium/index.js`), turn auto-rotate back on (`adb shell settings put system accelerometer_rotation 1`), and remove Appium's helper apps (`adb uninstall` `io.appium.uiautomator2.server`, `io.appium.uiautomator2.server.test`, `io.appium.settings`).

## Results
- `tests/WorkoutLog.E2E.Tests/TestResults/E2E/<yyyyMMdd-HHmmss>/` per run. A failed scenario saves its screen, page source, app log and Appium log in its own folder there.
- A phone run's files can show notifications, the phone's serial number and what the tests typed. Keep them local; turn on Do Not Disturb during a run.
