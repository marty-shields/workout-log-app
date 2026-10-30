---
$attribute: CustomPage("Screens/Dashboard/Overview")
---

# Dashboard

Home screen: start a workout, see how often you train, open Workouts, Exercises and Settings. View: `DashboardView`; page: `DashboardPage`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log"; Settings gear (`IconButton`) on the right in every state, opens Settings. No back arrow; back leaves the app. |
| Content | Last workout: date and day of the latest start, or `EmptyState` "No workouts yet". This week and This month as two `StatTile`s. Workouts and Exercises as `ListRow`s; Exercises opens [Exercises](./?path=/custom/screens-exercises-overview). |
| Loading | `Spinner` "Loading…" instead of the last workout and counts. Workouts, Exercises and New Workout work. |
| Load failed | Error "Couldn't load the Dashboard" and a Secondary full-width **Try again** that reloads, instead of the last workout and counts. Focus moves to Try again. Workouts, Exercises and New Workout work. |
| Bottom bar | New Workout, `AppButton` primary, full width. |

This week counts from Monday 00:00 to now, this month from the 1st at 00:00 to now. A workout dated after now shows as the last workout but isn't counted.

Loads each time it shows, and again when the app comes back on screen.

States: [Empty](./?path=/story/screens-dashboard--empty) · [With workouts](./?path=/story/screens-dashboard--with-workouts) · [Loading](./?path=/story/screens-dashboard--loading) · [Load failed](./?path=/story/screens-dashboard--load-failed)

`data-testid`: `last-workout`, `last-workout-empty`, `count-week`, `count-month`, `workouts-link`, `exercises-link`, `new-workout`, `dashboard-loading`, `dashboard-error`, `dashboard-retry`, `settings`.
