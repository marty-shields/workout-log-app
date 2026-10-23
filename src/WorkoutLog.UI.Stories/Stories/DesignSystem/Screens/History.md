---
$attribute: CustomPage("Design System/Screens/History")
---

# History

Saved workouts, newest first (FR-20, FR-21). View: `HistoryView`; page: `HistoryPage`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "History". |
| Content | One `ListRow` per workout: date, day and start time, e.g. *Tue 20 Oct 2026 · 18:05*. `EmptyState` "No workouts yet" when there are none. |
| Loading | `Spinner` "Loading…" centred until the workouts load (D181). |
| Load failed | Error "Couldn't load your history." and a Secondary **Try again** that reloads. Focus moves to Try again (D180). |

Tapping a row opens the workout in Edit Workout (D186). Back: Dashboard.

States: [Empty](/?path=/story/screens-history--empty) · [With workouts](/?path=/story/screens-history--with-workouts) · [Loading](/?path=/story/screens-history--loading) · [Load failed](/?path=/story/screens-history--load-failed)

`data-testid`: `back`, `history-empty`, `history-row-{id}`, `history-loading`, `history-error`, `history-retry`.
