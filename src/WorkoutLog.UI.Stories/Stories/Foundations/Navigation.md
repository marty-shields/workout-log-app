---
$attribute: CustomPage(StoryTitles.Prefix + "/Foundations/Navigation")
---

# Navigation

The app opens on the kit's start-up screen (`StartupGate`), then [This week](./?path=/custom/workout-log-screens-this-week-overview). Add Exercise is an overlay on the workout screen, not a route.

This week and [Trends](./?path=/custom/workout-log-screens-trends-overview) are the two top-level screens. Each shows the bottom navigation bar (the kit's `BottomNav`, whose stories are in the Shields.UI kit's story book): **This week**, a raised **+** in the middle and **Trends**. They have no back arrow. Android back from Trends goes to This week, and from This week leaves the app. + opens New Workout from either tab.

[Workouts](./?path=/custom/workout-log-screens-workouts-overview) is not a tab. It opens from Trends for the dates tapped (a day in the month calendar, a week row, a day's bars in the week view, a month's bar in the year view), with those dates already chosen, a back arrow and no bottom nav. There is no Exercises list: an [Exercise](./?path=/custom/workout-log-screens-exercise-overview) page opens from a record or a Most trained row, and [Add Exercise](./?path=/custom/workout-log-screens-add-exercise-overview) is the only list of exercises.

| From | Action | To |
|---|---|---|
| This week | Trends in the bottom nav | Trends |
| This week | + in the bottom nav | New Workout |
| This week | Tap a record | Exercise |
| This week | Settings gear | Settings |
| This week | Tap a workout | Edit Workout |
| This week | Last week row (an empty week) | Trends, last week |
| This week | Back | Leaves the app |
| Trends | This week in the bottom nav, or back | This week |
| Trends | + in the bottom nav | New Workout |
| Trends | Week, Month or Year in the kind control | Trends, the week, month or year holding the day shown |
| Trends | Previous or next period | Trends, that week, month or year |
| Trends | Tap a day with a workout in the month calendar, a week row, a day's bars in the week view, or a month's bars in the year view | Workouts, those dates |
| Trends | Tap a record, or a Most trained row (month) | Exercise |
| Trends | Settings gear | Settings |
| Workouts | Tap a row | Edit Workout |
| Workouts | Back arrow, or Android back | Trends, the same period |
| Exercise | Back | Where it was opened from: This week, or the Trends week, month or year |
| Settings | Back | Where it was opened from: This week, or Trends at the same period |
| Settings | Import, then pick a file | Import Review |
| Import Review | Import, or Replace all then Replace | Settings |
| Import Review | Cancel, or back | Settings |
| New Workout | Add exercise | Add Exercise |
| New Workout | Save | Workout Summary |
| New Workout | Back, then Discard | The tab it was started from: This week, or Trends at the same period |
| Workout Summary | Done, or back | This week |
| Edit Workout | Add exercise | Add Exercise |
| Edit Workout | Save, Back with no changes, Back with changes then Discard, or Delete then Delete | Where it was opened from: This week, or Workouts with the same dates |
| Add Exercise | Pick an exercise, or back | The workout it opened from |
| Error | Restart | This week |

## Where a screen came from
A link that opens a screen carries where it was opened from in `?from=`, and the screen returns there. The values (`Origin`):

| `from` | Returns to |
|---|---|
| `this-week`, or none | This week |
| `trends:month:2026-11-01` | Trends, the week, month or year (`week`, `month` or `year`) starting on that day. The older `trends:2026-11-09` still means the week holding it. |
| `workouts:2026-11-02:2026-11-08` | Workouts with those first and last days. Anything after the second date is where Workouts itself was opened from, so back from it still reaches that, e.g. `workouts:2026-11-02:2026-11-08:trends:week:2026-11-02`. |

A value that doesn't parse returns to This week. Workouts takes its dates from `workouts?first=2026-11-02&last=2026-11-08`; without valid dates it shows the last 3 months.

## Back
- The back arrow and Android back do the same, except with the soft keyboard open: Android back then only hides the keyboard, and the next back acts as below. The back arrow acts at once.
- An open `ConfirmDialog` (the kit's) closes on Android back, as Cancel does.
- A `Snackbar` is not a dialog: Android back does what it does on the screen and leaves the snackbar to time out.
- What back does on each screen is on its page: [New Workout](./?path=/custom/workout-log-screens-new-workout-overview), [Edit Workout](./?path=/custom/workout-log-screens-edit-workout-overview), [Add Exercise](./?path=/custom/workout-log-screens-add-exercise-overview), [Workouts](./?path=/custom/workout-log-screens-workouts-overview), [Exercise](./?path=/custom/workout-log-screens-exercise-overview), [Settings](./?path=/custom/workout-log-screens-settings-overview), [Import Review](./?path=/custom/workout-log-screens-import-review-overview). Workout Summary has no back arrow: Android back does what Done does, [Workout Summary](./?path=/custom/workout-log-screens-workout-summary-overview).

## Startup and error
- `Routes.razor` wraps the router in `StartupGate` and `AppErrorBoundary`, both titled "Workout Log". `HostBridges` (the kit's) comes first, before the gate.
- Start-up deletes leftover export files and opens the database, before This week shows. While it runs the screen shows "Loading…".
- If that fails, the screen shows "Couldn't open the workout database" and the kit's Try again button, which runs start-up again (`Retryable` is left at its default of true).
- An unhandled error in any screen shows the kit's error screen with "Something went wrong" and no details. **Restart** goes to This week; an unsaved workout is lost.
- The start-up gate, the error boundary and the confirm dialog come from the kit (`StartupGate`, `AppErrorBoundary`, `ConfirmDialog`). Their stories are in the Shields.UI kit's story book, under Components/Layout (`StartupGate`) and Components/Overlays (`ConfirmDialog`).
- The E2E tests find the kit's parts by `data-testid`: `startup-loading`, `startup-failed` (the failed state, with or without Try again), `startup-retry`, `error-message`, `error-restart`. `ConfirmDialog` uses `confirm-dialog` for the dialog and `{TestId}-cancel` and `{TestId}-confirm` for its buttons, where `TestId` is `confirm-dialog` unless a screen sets another.
