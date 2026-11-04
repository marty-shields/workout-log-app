---
$attribute: CustomPage("Screens/Workouts/Overview")
---

# Workouts

Saved workouts, newest first. View: `WorkoutsView`; page: `WorkoutsPage`.

| Part | Content |
|---|---|
| Top bar | Title "Workouts". No back arrow: Workouts is a top-level screen. |
| Content | One `ListRow` per workout: date, day and start time, e.g. *Tue 20 Oct 2026 · 18:05*. `EmptyState` "No workouts yet", hint "Tap New Workout on the Dashboard to log one", when there are none. |
| Loading | `Spinner` "Loading…" centred until the workouts load. |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reloads. Focus moves to Try again. |
| Navigation bar | `BottomNav` with Workouts current. |

Rows are newest first by start time. All load at once.

Tapping a row opens the workout in Edit Workout. Android back, or Dashboard in the navigation bar: Dashboard. Exercises in the navigation bar: Exercises.

States: [Empty](./?path=/story/screens-workouts--empty) · [With workouts](./?path=/story/screens-workouts--with-workouts) · [Loading](./?path=/story/screens-workouts--loading) · [Load failed](./?path=/story/screens-workouts--load-failed)

`data-testid`: `workouts-empty`, `workouts-row-{id}`, `workouts-loading`, `workouts-error`, `workouts-retry`, and the navigation bar's `bottom-nav`, `dashboard-link`, `workouts-link`, `exercises-link`.
