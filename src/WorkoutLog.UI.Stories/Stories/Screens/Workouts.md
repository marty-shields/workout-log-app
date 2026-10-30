---
$attribute: CustomPage("Screens/Workouts/Overview")
---

# Workouts

Saved workouts, newest first. View: `WorkoutsView`; page: `WorkoutsPage`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Workouts". |
| Content | One `ListRow` per workout: date, day and start time, e.g. *Tue 20 Oct 2026 · 18:05*. `EmptyState` "No workouts yet" when there are none. |
| Loading | `Spinner` "Loading…" centred until the workouts load. |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reloads. Focus moves to Try again. |

Rows are newest first by start time. All load at once.

Tapping a row opens the workout in Edit Workout. Back: Dashboard.

States: [Empty](./?path=/story/screens-workouts--empty) · [With workouts](./?path=/story/screens-workouts--with-workouts) · [Loading](./?path=/story/screens-workouts--loading) · [Load failed](./?path=/story/screens-workouts--load-failed)

`data-testid`: `back`, `workouts-empty`, `workouts-row-{id}`, `workouts-loading`, `workouts-error`, `workouts-retry`.
