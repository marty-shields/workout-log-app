# 07 — Testing

## Layers
| Type | Project | Tools | Covers |
|---|---|---|---|
| Unit | `WorkoutLog.Core.Tests` | NUnit, Moq, AutoFixture | Metric validation per exercise definition (FR-06, FR-07, FR-09), start/finish capture (FR-01, FR-10, FR-11), last workout and dashboard counts in the phone's time zone, DST at midnight included (FR-32, FR-33, FR-34, D196), exercise seeding, definition file checks (unique Ids, names and aliases; Ids 1…N without gaps; the documented counts; valid values; unknown fields fail to load; strength/cardio fields and metrics match the type; units only with their metric), search normalisation and ranking (FR-03), logged events with `FakeLogger` (T43), `CoreEvent` values unique, in range and fixed (T66) |
| Repository | `WorkoutLog.Core.Tests` | NUnit + temp SQLite file | Save, load, list newest first, list by start range and limit, edit, cascade delete, one CardioEntry per exercise |
| Component | `WorkoutLog.UI.Tests` | bUnit | Fields and labels shown per exercise definition, picker search, add/remove set, live timer, inline validation, Discard/Cancel and Delete prompts; every prototype scenario step finds its target; design-system checks (D117): tokens only, WCAG AA token pairs, touch targets, `data-testid` on every interactive element, screen stories match the tests' list of screen states, a Design System page for every component and screen; app start handlers; back and app start handlers per activity and the App's activity lifecycle wiring, read from its source (T74); Debug builds embedding their assemblies, read from the App's project file (T78); logged events with `FakeLogger`; `UiEvent` and `AppEvent` values unique, in range and fixed, retired values not reused (T73), and no `[LoggerMessage]` with a bare number (T66) |
| UI (automated) | `WorkoutLog.E2E.Tests` | NUnit + Appium (UiAutomator2) | Typical user flows end to end on the Debug build, on the phone or an emulator (D200) |
| Manual | — | Checklist below | Real-phone checks before each release APK |

Time is injected through .NET `TimeProvider`; tests use `FakeTimeProvider`, so they never depend on the real clock (T31).

## E2E tests (Appium)
**Purpose:** typical user flows end to end, as close to real use as possible: the Debug build, installed and run on a device. The phone is used now; an emulator is chosen by run setting (D200). How to set up and run: [08](08-dev-setup-build.md#e2e-tests).

**How it works**
- The UI is HTML inside an Android WebView. Appium switches from the `NATIVE_APP` context to `WEBVIEW_uk.martinshields.workoutlog.debug` to find elements. Debug builds turn on WebView debugging (`AddBlazorWebViewDeveloperTools`); Chromedriver for the device's WebView downloads itself (T76).
- `AppiumSetup` (`[SetUpFixture]`, `[Explicit]`) starts Appium, opens one session for the run and installs the Debug APK. At the end it closes the Debug app, leaving it installed, and removes Appium's helper apps from the device (T77).
- Each test derives from `AppTest`. Its SetUp fails at once if the device is locked, then clears the Debug app's data, launches the app and switches to the WebView. So every test starts as a fresh install.
- Tests find elements by `data-testid` with `FindByTestId` and `WaitForTestId`.
- Tests run one at a time on one device (`[assembly: NonParallelizable]`).
- The Android back button is pressed from the native context (US-24).
- A failed test saves a screenshot, the page source, the app's log and its part of the Appium log (T79, [08](08-dev-setup-build.md#results)).
- A locked device: the page renders but isn't visible, so element text reads empty and screenshots are black. At the start of a run Appium unlocks a swipe-only lock screen, not a PIN; a lock later in the run fails each remaining test.

| Test | Checks |
|---|---|
| `LaunchTests.Launch_ShowsTheFreshInstallDashboard` | A fresh install opens on the Dashboard: "No workouts yet", 0 this week and this month, History and New Workout, no startup error |

## Automated UI flows (v1)
1. Start a workout, add a strength exercise with 2 sets, save. It lands on the Dashboard and appears in History.
2. Add Plank: only duration (required) and weight are shown.
3. Add Stair Climber: duration, floors, level, calories and steps/min are shown; save with duration only.
4. Save with no exercises. It is blocked and an error shows.
5. Back on a new workout → Cancel: still on the page. Back → Discard: nothing is saved.
6. Edit a saved workout, change a set, press back → Discard. The change is not saved.
7. Edit a saved workout and add a set, save. The times are unchanged.
8. Delete a workout and confirm. It is gone from History.

## Manual on-phone checklist
- [ ] Fresh install opens to the Dashboard with an empty state
- [ ] New Workout: the date + day are correct and the live timer runs
- [ ] Picker search finds exercises by name and nickname, words in any order (e.g. "incline dumbbell press", "pec deck")
- [ ] Search keeps matching while typing a joined word ("pullu", "pushups") and after punctuation ("bench."); the box takes at most 50 characters
- [ ] Strength fields match [04](04-exercise-list.md) per exercise; weight decimals accepted (e.g. 22.75, .5); 22.755, 5. and 22,75 rejected; assisted machines show Assist (kg)
- [ ] Add set on an empty or faulty set is blocked, shows why and moves focus to the first field at fault; Remove set is never blocked
- [ ] Cardio: duration h:mm:ss required; other fields match [04](04-exercise-list.md), with the correct units and labels
- [ ] Outdoor Run pace is entered and shown as m:ss (e.g. 5:30); 5:75 and 61:00 are errors on Save
- [ ] Only one cardio entry per cardio exercise
- [ ] Adding an exercise starts it with one empty set (strength) or its empty entry (cardio)
- [ ] Picking an exercise already in the workout adds a set to its card (strength) or focuses its entry (cardio); no second card
- [ ] Two workouts can be saved on the same day
- [ ] Remove a set and an exercise
- [ ] Save blocked when there's no exercise, an exercise has no set/entry, or a set/entry misses a required metric; the error shows on the item at fault
- [ ] Back on a new workout shows Discard / Cancel
- [ ] Back on an edited workout shows Discard / Cancel; with no changes it goes straight to History
- [ ] Save returns to the Dashboard
- [ ] History is newest first and shows date, day and start time; empty state shows "No workouts yet"
- [ ] Back: picker closes; History → Dashboard; Dashboard leaves the app
- [ ] Content stays clear of the status and navigation bars
- [ ] Edit saves changes; start/finish unchanged
- [ ] Delete asks for confirmation, then removes the workout
- [ ] Dashboard: the last workout and the this week / this month counts are correct (week starts Monday)
- [ ] Leave the app on the Dashboard and come back: it reloads; logcat shows "Activity started" (5014) and, on a Debug build, "Dashboard loaded" (1401)
- [ ] Data survives closing and reopening the app
- [ ] Data survives installing a new APK version over the old one
- [ ] Logs: `adb logcat -s WorkoutLog` shows the start, the previous exit and lifecycle lines; the log file can be pulled ([08](08-dev-setup-build.md#reading-the-logs))

## Dependencies
See [05 Architecture](05-architecture.md#dependencies) for packages and versions.
