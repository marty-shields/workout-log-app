---
$attribute: CustomPage("Design System/Screens/History")
---

# History

Saved workouts, newest first (FR-20, FR-21). View: `HistoryView`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "History". |
| Content | One `ListRow` per workout: date, day and start time, e.g. *Tue 20 Oct 2026 · 18:05*. `EmptyState` "No workouts yet" when there are none. |

Rows open the workout in Edit Workout when `OnOpen` is set (M4); until then they are plain text (M3). Back: Dashboard.

States: [Empty](/?path=/story/screens-history--empty) · [With workouts](/?path=/story/screens-history--with-workouts) · [Rows not tappable](/?path=/story/screens-history--rows-not-tappable)

`data-testid`: `back`, `history-empty`, `history-row-{id}`.
