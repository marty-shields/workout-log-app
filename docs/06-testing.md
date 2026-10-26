# 06 — Testing

## Layers
| Type | Project | Tools | Covers |
|---|---|---|---|
| Unit | `WorkoutLog.Core.Tests` | NUnit, Moq | Metric validation, start/finish capture, dashboard counts in the phone's time zone (D196), exercise seeding, exercise file checks, search (FR-03), logged events and EventIds (T43, T66) |
| Repository | `WorkoutLog.Core.Tests` | NUnit + temp SQLite file | Save, load, list, edit, cascade delete |
| Component | `WorkoutLog.UI.Tests` | bUnit | Screens and components per exercise definition, prototype scenarios, design-system checks (D117), back and app start handlers (T74), the App's project file (T78), logged events and EventIds (T66, T73) |
| UI (automated) | `WorkoutLog.E2E.Tests` | Reqnroll + NUnit + Appium (UiAutomator2) | User flows end to end on the Debug build, on the phone or an emulator, as Gherkin features (D200, T80). The QA pipeline runs them all on emulators, on every PR and in the release (T105, T113, T114) |
| Release smoke | `release.yml` (`release-smoke.sh`) | adb | The Release build of each release, x64 with the debug key, starts cleanly on an emulator (T107) |

## E2E tests (Appium)
**Purpose:** user flows end to end, as close to real use as possible: the Debug build, installed and run on a device. A phone or an emulator, chosen by run setting (D200). How to set up and run: [07](07-dev-setup-build.md#e2e-tests).

**How it works**
- The UI is HTML inside an Android WebView. Appium switches from the `NATIVE_APP` context to `WEBVIEW_uk.martinshields.workoutlog.debug` to find elements. Debug builds turn on WebView debugging (`AddBlazorWebViewDeveloperTools`); Chromedriver for the device's WebView downloads itself (T76).
- `AppiumSetup` (`[SetUpFixture]`, `[Explicit]`) starts Appium, opens one session for the run and installs the Debug APK. At the end it closes the Debug app, leaving it installed, and removes Appium's helper apps from the device (T77).
- Scenarios are Gherkin in `tests/WorkoutLog.E2E.Tests/Features/`, run by Reqnroll on NUnit (T80). Each feature describes what it covers. Scenarios are tagged with their stories, FRs and decisions, on the feature when every scenario shares them; `@smoke` marks the core journeys (D204, D208).
- `AppHooks` runs before each scenario: it fails at once if the device is locked, then clears the Debug app's data, so every scenario starts as a fresh install. Before the first step it launches the app and switches to the WebView, unless that step launches the app itself: the seeding steps and "Given I am on" (T115).
- "Given these saved workouts" builds a database on the PC with Core and copies it into the app (T81). What a scenario is about is done through the UI.
- Expected dates and counts come from the device's clock and time zone (D205).
- Steps find elements by `data-testid` through the screen classes in `Screens/`. Typing goes a key at a time (T82).
- "Android back" is the back key. "I have …" steps end with the soft keyboard hidden (T84, D206).
- Device commands (the back key, the keyboard check and hide, backgrounding, the app's state) run from the WebView. The keyboard is hidden with the back key, only when an input has focus (T87). The screen is held in portrait for the run (T86).
- Steps check focus where [05](05-screens-ux.md) says it moves, the soft keyboard and keypad on number fields, and the app's log (T83, T84).
- Scenarios run one at a time on one device (`[assembly: NonParallelizable]`).
- A failed scenario saves its evidence ([07 Results](07-dev-setup-build.md#results), T79).

### In UI.Tests, not E2E
A user can't cause these on the phone, or they aren't behaviour (D203): startup failure (D133); load failures and Try again on the Dashboard, History and Edit (D180, D185, D195); Not found (D185); save and delete failures (D176, D191); the Error screen and Restart (D140); taps ignored while a save or delete runs, beyond a double Save (D177, D189, D191); DST at midnight (D196); exercise file checks; themes, tokens, contrast and touch targets.

Scenario outlines keep one row per device path; other values of the same rule are tested in UI.Tests or Core.Tests (T88).
