# 07 — Testing

## Layers
| Type | Project | Tools | Covers |
|---|---|---|---|
| Unit | `WorkoutLog.Core.Tests` | NUnit, Moq, AutoFixture | Metric validation per exercise definition (FR-06, FR-07, FR-09), start/finish capture (FR-01, FR-10, FR-11), last workout and dashboard counts in the phone's time zone, DST at midnight included (FR-32, FR-33, FR-34, D196), exercise seeding, definition file checks (unique Ids, names and aliases; Ids 1…N without gaps; the documented counts; valid values; unknown fields fail to load; strength/cardio fields and metrics match the type; units only with their metric), search normalisation and ranking (FR-03), logged events with `FakeLogger` (T43), `CoreEvent` values unique, in range and fixed (T66) |
| Repository | `WorkoutLog.Core.Tests` | NUnit + temp SQLite file | Save, load, list newest first, list by start range and limit, edit, cascade delete, one CardioEntry per exercise |
| Component | `WorkoutLog.UI.Tests` | bUnit | Fields and labels shown per exercise definition, picker search, add/remove set, live timer, inline validation, Discard/Cancel and Delete prompts; every prototype scenario step finds its target; design-system checks (D117): tokens only, WCAG AA token pairs, touch targets, `data-testid` on every interactive element, screen stories match the tests' list of screen states, a Design System page for every component and screen; app start handlers; back and app start handlers per activity and the App's activity lifecycle wiring, read from its source (T74); Debug builds embedding their assemblies, read from the App's project file (T78); logged events with `FakeLogger`; `UiEvent` and `AppEvent` values unique, in range and fixed, retired values not reused (T73), and no `[LoggerMessage]` with a bare number (T66) |
| UI (automated) | `WorkoutLog.E2E.Tests` | Reqnroll + NUnit + Appium (UiAutomator2) | User flows end to end on the Debug build, on the phone or an emulator, as Gherkin features (D200, T80). The release workflow runs them all on emulators (T105) |
| Release smoke | `release.yml` (`release-smoke.sh`) | adb | The Release build of each release, x64 with the debug key, starts cleanly on an emulator (T107) |
| Manual | — | Checklist below | Real-phone checks on the release-signed APK of each release |

Time is injected through .NET `TimeProvider`; tests use `FakeTimeProvider`, so they never depend on the real clock (T31).

## E2E tests (Appium)
**Purpose:** user flows end to end, as close to real use as possible: the Debug build, installed and run on a device. A phone or an emulator, chosen by run setting (D200). How to set up and run: [08](08-dev-setup-build.md#e2e-tests).

**How it works**
- The UI is HTML inside an Android WebView. Appium switches from the `NATIVE_APP` context to `WEBVIEW_uk.martinshields.workoutlog.debug` to find elements. Debug builds turn on WebView debugging (`AddBlazorWebViewDeveloperTools`); Chromedriver for the device's WebView downloads itself (T76).
- `AppiumSetup` (`[SetUpFixture]`, `[Explicit]`) starts Appium, opens one session for the run and installs the Debug APK. At the end it closes the Debug app, leaving it installed, and removes Appium's helper apps from the device (T77).
- Scenarios are Gherkin in `tests/WorkoutLog.E2E.Tests/Features/`, run by Reqnroll on NUnit (T80). Scenarios are tagged with their stories, FRs and decisions, on the feature when every scenario shares them; `@smoke` marks the core journeys (D204, D208).
- `AppHooks` runs before each scenario: it fails at once if the device is locked, then clears the Debug app's data, launches the app and switches to the WebView. So every scenario starts as a fresh install.
- "Given these saved workouts" builds a database on the PC with Core and copies it into the app (T81). What a scenario is about is done through the UI.
- Expected dates and counts come from the device's clock and time zone (D205).
- Steps find elements by `data-testid` through the screen classes in `Screens/`. Typing goes a key at a time (T82).
- "Android back" is the back key. "I have …" steps end with the soft keyboard hidden (T84, D206).
- Device commands (the back key, the keyboard check and hide, backgrounding, the app's state) run from the WebView. The keyboard is hidden with the back key, only when an input has focus (T87). The screen is held in portrait for the run (T86).
- Steps check focus where [06](06-screens-ux.md) says it moves, the soft keyboard and keypad on number fields, and the app's log (T83, T84).
- Scenarios run one at a time on one device (`[assembly: NonParallelizable]`). In the release workflow a failed scenario runs again, up to 3 tries (T109).
- A failed scenario saves a screenshot, the page source, the app's log and its part of the Appium log (T79, [08](08-dev-setup-build.md#results)).
- A locked device: the page renders but isn't visible, so element text reads empty and screenshots are black. At the start of a run Appium unlocks a swipe-only lock screen, not a PIN; a lock later in the run fails each remaining scenario.

### Features
| Feature | Covers |
|---|---|
| [Launch](../tests/WorkoutLog.E2E.Tests/Features/Launch.feature) | A fresh install opens on the empty Dashboard |
| [Dashboard](../tests/WorkoutLog.E2E.Tests/Features/Dashboard.feature) | Last workout, this week and this month counts, a workout dated after now, the History and New Workout links |
| [NewWorkout](../tests/WorkoutLog.E2E.Tests/Features/NewWorkout.feature) | Today's date and the running timer |
| [ExercisePicker](../tests/WorkoutLog.E2E.Tests/Features/ExercisePicker.feature) | Focus and empty search on open, the full list A–Z, search by name and nickname, no match, 50 characters, adding and scrolling to the new exercise, picking an exercise already added, closing |
| [StrengthSets](../tests/WorkoutLog.E2E.Tests/Features/StrengthSets.feature) | Fields per exercise, Add set blocked and why, every value rule, re-checks, remove set and exercise, duration fill-in |
| [CarrySets](../tests/WorkoutLog.E2E.Tests/Features/CarrySets.feature) | The open set and summary lines |
| [CardioEntry](../tests/WorkoutLog.E2E.Tests/Features/CardioEntry.feature) | Fields per machine, one entry, checks on Save, every value rule, pace fill-in |
| [SaveChecks](../tests/WorkoutLog.E2E.Tests/Features/SaveChecks.feature) | Save blocked with no exercise or set, every error shown, focus on the first, fixing and saving |
| [SaveWorkout](../tests/WorkoutLog.E2E.Tests/Features/SaveWorkout.feature) | Strength, cardio and mixed workouts saved and seen again, zero values, two on one day, a double tap, the finish time |
| [DiscardNewWorkout](../tests/WorkoutLog.E2E.Tests/Features/DiscardNewWorkout.feature) | The Discard prompt from either back, Cancel, Discard |
| [History](../tests/WorkoutLog.E2E.Tests/Features/History.feature) | Empty, newest first, the same start, every workout loads at once, opening a row, back |
| [EditWorkout](../tests/WorkoutLog.E2E.Tests/Features/EditWorkout.feature) | The saved workout and total time, changing, adding and removing sets and exercises, checks, times unchanged |
| [DiscardEdits](../tests/WorkoutLog.E2E.Tests/Features/DiscardEdits.feature) | Back with and without changes, what counts as a change, Cancel, Discard |
| [DeleteWorkout](../tests/WorkoutLog.E2E.Tests/Features/DeleteWorkout.feature) | The Delete prompt, Cancel, Delete with and without edits, the Dashboard after |
| [Navigation](../tests/WorkoutLog.E2E.Tests/Features/Navigation.feature) | Android back on each screen, leaving and reopening the app |
| [AppLifecycle](../tests/WorkoutLog.E2E.Tests/Features/AppLifecycle.feature) | Data survives a restart, an unsaved workout doesn't, the Dashboard reloads |
| [Keyboard](../tests/WorkoutLog.E2E.Tests/Features/Keyboard.feature) | The soft keyboard and keypad on number fields and search; back with the keyboard open (D206) |
| [Logging](../tests/WorkoutLog.E2E.Tests/Features/Logging.feature) | Start, save, update, discard and delete events with ids and counts; no typed values or validation messages |

### In UI.Tests, not E2E
A user can't cause these on the phone, or they aren't behaviour (D203): startup failure (D133); load failures and Try again on the Dashboard, History and Edit (D180, D185, D195); Not found (D185); save and delete failures (D176, D191); the Error screen and Restart (D140); taps ignored while a save or delete runs, beyond a double Save (D177, D189, D191); DST at midnight (D196); exercise file checks; themes, tokens, contrast and touch targets.

Scenario outlines keep one row per device path; other values of the same rule are tested in UI.Tests or Core.Tests (T88).

## Manual on-phone checklist
Only what the E2E tests don't run (D202):
- [ ] Content stays clear of the status and navigation bars
- [ ] Data survives installing a new APK version over the old one
- [ ] The back gesture does what the Android back key does in the E2E tests (T84)
- [ ] Logs on a Release build: `adb logcat -s WorkoutLog` shows the start, the previous exit and lifecycle lines; the log file can be pulled ([08](08-dev-setup-build.md#reading-the-logs))
- [ ] App icon on the home screen and in the app drawer: the notebook, crisp and uncropped (D209)
- [ ] Themed icons on (Wallpaper & style): the icon shows the dumbbell, lines and binding (D212)
- [ ] Splash on a cold start: the notebook on dark teal, uncropped (D209)

## Dependencies
See [05 Architecture](05-architecture.md#dependencies) for packages.
