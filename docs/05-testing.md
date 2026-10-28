# 05 — Testing

## Layers
| Type | Project | Tools | Covers |
|---|---|---|---|
| Unit | `WorkoutLog.Core.Tests` | NUnit, Moq | Metric validation, start/finish capture, dashboard counts in the phone's time zone (D66), exercise seeding, exercise file checks, search (FR-03), logged events and EventIds (D132) |
| Repository | `WorkoutLog.Core.Tests` | NUnit + temp SQLite file | Save, load, list, edit, cascade delete |
| Component | `WorkoutLog.UI.Tests` | bUnit | Screens and components per exercise definition, design-system and story coverage checks (D97), back and app start handlers (D119), the App's project file (D143), logged events and EventIds (D132, D118) |
| UI (automated) | `WorkoutLog.E2E.Tests` | Reqnroll + NUnit + Appium (UiAutomator2) | User flows end to end on the Debug build, on the phone or an emulator, as Gherkin features (D137, D141). The QA pipeline runs them all on emulators, on every PR and in the release (D167, D165) |
| Release smoke | `release.yml` (`release-smoke.sh`) | adb | The Release build of each release, x64 with the debug key, starts cleanly on an emulator (D175) |

## E2E tests (Appium)
**Purpose:** user flows end to end, as close to real use as possible: the Debug build, installed and run on a device. A phone or an emulator, chosen by run setting (D137). How to set up and run: [06](06-dev-setup-build.md#e2e-tests).

**How it works**
- The UI is HTML inside an Android WebView. Appium switches from the `NATIVE_APP` context to `WEBVIEW_uk.martinshields.workoutlog.debug` to find elements. Debug builds turn on WebView debugging (`AddBlazorWebViewDeveloperTools`); Chromedriver for the device's WebView downloads itself (D140).
- `AppiumSetup` (`[SetUpFixture]`, `[Explicit]`) starts Appium, opens one session for the run and installs the Debug APK. At the end it closes the Debug app, leaving it installed, and removes Appium's helper apps from the device (D141).
- Scenarios are Gherkin in `tests/WorkoutLog.E2E.Tests/Features/`, run by Reqnroll on NUnit (D141). Each feature describes what it covers. Scenarios are tagged with their stories, FRs and decisions, on the feature when every scenario shares them; `@smoke` marks the core journeys (D154).
- `AppHooks` runs before each scenario: it fails at once if the device is locked, then clears the Debug app's data, so every scenario starts as a fresh install. Before the first step it launches the app and switches to the WebView, unless that step launches the app itself: the seeding steps and "Given I am on" (D142).
- "Given these saved workouts" builds a database on the PC with Core and copies it into the app (D145). What a scenario is about is done through the UI.
- Expected dates and counts come from the device's clock and time zone (D153).
- Steps find elements by `data-testid` through the screen classes in `Screens/`. Typing goes a key at a time (D146).
- "Android back" is the back key. "I have …" steps end with the soft keyboard hidden (D148, D78).
- Device commands (the back key, the keyboard check and hide, backgrounding, the app's state) run from the WebView. The keyboard is hidden with the back key, only when an input has focus (D150). The screen is held in portrait for the run (D149).
- Steps check focus where the [screen pages](../src/WorkoutLog.UI.Stories/Stories/Screens/) say it moves, the soft keyboard and keypad on number fields, and the app's log (D147, D148).
- Scenarios run one at a time on one device (`[assembly: NonParallelizable]`).
- A failed scenario saves its evidence ([06 Results](06-dev-setup-build.md#results), D144).

### In UI.Tests, not E2E
A user can't cause these on the phone, or they aren't behaviour (D138): startup failure (D75); load failures and Try again on the Dashboard, Workouts and Edit (D69, D72, D67); Not found (D72); save and delete failures (D40, D46); the Error screen and Restart (D76); taps ignored while a save or delete runs, beyond a double Save (D41, D73, D46); DST at midnight (D66); exercise file checks; themes, tokens, contrast and touch targets.

Scenario outlines keep one row per device path; other values of the same rule are tested in UI.Tests or Core.Tests (D151).
