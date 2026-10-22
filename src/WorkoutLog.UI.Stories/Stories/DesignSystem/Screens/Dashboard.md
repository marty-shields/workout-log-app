---
$attribute: CustomPage("Design System/Screens/Dashboard")
---

# Dashboard

Home screen: start a workout, see how often you train, open History (FR-30–FR-35). View: `DashboardView`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". No back arrow; back leaves the app. |
| Content | Last workout (date and day, or `EmptyState` "No workouts yet"); This week and This month as two `StatTile`s; History as a `ListRow`. |
| Bottom bar | New Workout, `AppButton` primary, full width. |

States: [Empty](/?path=/story/screens-dashboard--empty) · [With workouts](/?path=/story/screens-dashboard--with-workouts)

`data-testid`: `last-workout`, `last-workout-empty`, `count-week`, `count-month`, `history-link`, `new-workout`.
