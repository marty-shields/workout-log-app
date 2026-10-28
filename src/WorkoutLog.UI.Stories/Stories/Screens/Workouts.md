---
$attribute: CustomPage("Screens/Workouts/Overview")
---

# Workouts

Saved workouts, newest first (FR-20, FR-21). View: `WorkoutsView`; page: `WorkoutsPage`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Workouts". |
| Content | One `ListRow` per workout: date, day and start time, e.g. *Tue 20 Oct 2026 · 18:05*. `EmptyState` "No workouts yet" when there are none. |
| Loading | `Spinner` "Loading…" centred until the workouts load (D69). |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reloads. Focus moves to Try again (D69). |

Rows are newest first by start time (D70). All load at once (D70).

Tapping a row opens the workout in Edit Workout (D71). Back: Dashboard.

States: [Empty](./?path=/story/screens-workouts--empty) · [With workouts](./?path=/story/screens-workouts--with-workouts) · [Loading](./?path=/story/screens-workouts--loading) · [Load failed](./?path=/story/screens-workouts--load-failed)

`data-testid`: `back`, `workouts-empty`, `workouts-row-{id}`, `workouts-loading`, `workouts-error`, `workouts-retry`.
