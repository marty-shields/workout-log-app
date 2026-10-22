# 07 — Testing

## Layers
| Type | Project | Tools | Covers |
|---|---|---|---|
| Unit | `WorkoutLog.Core.Tests` | NUnit, Moq, AutoFixture | Metric validation per exercise definition (FR-06, FR-07, FR-09), start/finish capture (FR-01, FR-10, FR-11), dashboard counts (FR-33, FR-34), exercise seeding, definition file checks (unique Ids, names and aliases; valid values; strength/cardio fields match the type), search normalisation and ranking (FR-03) |
| Repository | `WorkoutLog.Core.Tests` | NUnit + temp SQLite file | Save, load, edit, cascade delete, one CardioEntry per exercise |
| Component | `WorkoutLog.UI.Tests` | bUnit | Fields and labels shown per exercise definition, picker search, add/remove set, live timer, inline validation, Discard/Cancel prompts; every prototype scenario step finds its target; design-system checks (D117): tokens only, WCAG AA token pairs, touch targets, `data-testid` on every interactive element, screen stories match the tests' list of screen states, a Design System page for every component and screen |
| UI (automated) | `WorkoutLog.E2E.Tests` | NUnit + Appium (UiAutomator2) | End-to-end flows on an emulator or the Debug build on the phone |
| Manual | — | Checklist below | Real-phone checks before each release APK |

Time is injected through `IClock`, so tests never depend on the real clock.

## Appium + Blazor Hybrid
- The UI is HTML inside an Android WebView. Appium must switch from the `NATIVE_APP` context to the `WEBVIEW_<package>` context to find elements.
- This needs WebView debugging turned on in Debug builds, and a matching Chromedriver.
- Elements are found by `data-testid` attributes on the Razor components.
- The Android back button is pressed from the native context.

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
- [ ] Strength fields match [04](04-exercise-list.md) per exercise; weight decimals accepted (e.g. 22.75); assisted machines show Assist (kg)
- [ ] Cardio: duration h:mm:ss required; other fields match [04](04-exercise-list.md), with the correct units and labels
- [ ] Outdoor Run pace is entered and shown as m:ss (e.g. 5:30)
- [ ] Only one cardio entry per cardio exercise
- [ ] Adding an exercise starts it with one empty set (strength) or its empty entry (cardio)
- [ ] Same exercise can be added twice
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
- [ ] This week / this month counts are correct (week starts Monday)
- [ ] Data survives closing and reopening the app
- [ ] Data survives installing a new APK version over the old one

## Dependencies
See [05 Architecture](05-architecture.md#dependencies) for packages and versions.
