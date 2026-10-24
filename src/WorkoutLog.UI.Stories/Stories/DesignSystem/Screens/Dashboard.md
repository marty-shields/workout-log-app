---
$attribute: CustomPage("Design System/Screens/Dashboard")
---

# Dashboard

Home screen: start a workout, see how often you train, open History (FR-30–FR-35). View: `DashboardView`; page: `DashboardPage`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". No back arrow; back leaves the app. |
| Content | Last workout (date and day, or `EmptyState` "No workouts yet"); This week and This month as two `StatTile`s; History as a `ListRow`. |
| Loading | `Spinner` "Loading…" instead of the last workout and counts. History and New Workout work (D195). |
| Load failed | Error "Couldn't load the Dashboard." and a Secondary full-width **Try again** that reloads, instead of the last workout and counts. Focus moves to Try again. History and New Workout work (D195). |
| Bottom bar | New Workout, `AppButton` primary, full width. |

Loads each time it shows, and again when the app comes back on screen (D197). Live page runs `DashboardPage` on sample values.

States: [Empty](/?path=/story/screens-dashboard--empty) · [With workouts](/?path=/story/screens-dashboard--with-workouts) · [Loading](/?path=/story/screens-dashboard--loading) · [Load failed](/?path=/story/screens-dashboard--load-failed) · [Live page](/?path=/story/screens-dashboard--live-page)

`data-testid`: `last-workout`, `last-workout-empty`, `count-week`, `count-month`, `history-link`, `new-workout`, `dashboard-loading`, `dashboard-error`, `dashboard-retry`.
