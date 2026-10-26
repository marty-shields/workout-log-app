---
$attribute: CustomPage("Screens/History/Overview")
---

# History

Saved workouts, newest first (FR-20, FR-21). View: `HistoryView`; page: `HistoryPage`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "History". |
| Content | One `ListRow` per workout: date, day and start time, e.g. *Tue 20 Oct 2026 · 18:05*. `EmptyState` "No workouts yet" when there are none. |
| Loading | `Spinner` "Loading…" centred until the workouts load (D69). |
| Load failed | Error "Couldn't load your history." and a Secondary **Try again** that reloads. Focus moves to Try again (D69). |

Rows are newest first by start time; with the same start, newest saved first (D70). All load at once (D70).

Tapping a row opens the workout in Edit Workout (D71). Back: Dashboard.

States: [Empty](./?path=/story/screens-history--empty) · [With workouts](./?path=/story/screens-history--with-workouts) · [Loading](./?path=/story/screens-history--loading) · [Load failed](./?path=/story/screens-history--load-failed)

`data-testid`: `back`, `history-empty`, `history-row-{id}`, `history-loading`, `history-error`, `history-retry`.
