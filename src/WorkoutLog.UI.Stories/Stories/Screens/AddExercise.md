---
$attribute: CustomPage(StoryTitles.Prefix + "/Screens/Add Exercise/Overview")
---

# Add Exercise

Add an exercise to the workout. View: `AddExerciseView`, the one list of exercises, an overlay on `WorkoutView` with the same `AppScreen` shell. It is not a route: the app has no Exercises screen, and an exercise's page opens from Trends or This week. `WorkoutEditor` holds its search, chip and rows, and loads the last sessions through `ILoggedExercisesService.GetAsync` each time it opens and groups the rows by whether they have one.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Exercises". |
| Toolbar | The kit's `SearchField` (with a polite result count) and `FilterChips`, pinned under the top bar with the toolbar divider; the rows scroll beneath. Shows from the moment the list opens, while the last sessions load too. |
| Search | Search box with the search `Icon` and "Search exercises", up to the catalog's search limit. Under it a polite line with how many exercises match across both groups, e.g. *12 exercises* (empty while the list loads). Empty each time the list opens, with focus. Text typed while loading is kept. |
| Chips | kit `FilterChips`: All, Strength, Cardio. All each time the list opens. |
| Rows | Every catalog exercise, as a tappable `ListRow` showing Strength or Cardio muted before the chevron, in two groups, each exercise in exactly one. **Recent Exercises**: every exercise with a saved workout (the open workout doesn't count), not capped. **Other Exercises**: the rest. Each group has a sticky `SectionHeader` with its name and row count, and a group with no rows after the search or chip is left out with its header; a lone group keeps its header. The list is flat with no headers when nothing has been logged, while loading and when the load failed. Filtered by the chip. A logged exercise shows "Last" with its last session's date and best under its name, e.g. *Last Tue 20 Oct · 70kg × 3*. The best: a lift's top set (heaviest; tie → most reps); a bodyweight move's most reps, or its top set when a set is over 0 kg; an assisted machine's least assist (tie → most reps); a hold's longest; a carry's heaviest set over 0 kg with its distance (tie → longest); cardio's distance when the exercise has it, else its duration. The date has the year when it isn't this year; with no best, the date only. Exercises already in the workout still show. |
| Order | No search text: Recent Exercises most recent first (same workout → A–Z), then Other Exercises Strength then Cardio, A–Z in each. With search text, in each group matches are ranked: every query word, in any order, starts a word of the name or a nickname; names that start with the query first, then other name matches, then nickname matches. Recent Exercises are most recent first within a rank, so a done exercise is always above a better text match that has not been done; Other Exercises are A–Z within a rank. |
| No match | "No exercises match" under the toolbar. |
| Loading | `Spinner` "Loading…" centred under the toolbar, in place of the rows, while the last sessions load. |
| Load failed | The rows in one flat list with no "Last" lines, no headers and no message; adding still works. |

Tapping an exercise adds it to the workout and closes the list. Picking one already in the workout moves its card to the top instead of adding it again. With 10 exercises, Add exercise is off, so the list can't open; see [New Workout](./?path=/custom/workout-log-screens-new-workout-overview).

Back closes the list; with the soft keyboard open, Android back first only hides the keyboard. No bottom nav and no pull to refresh.

States: [No search text](./?path=/story/workout-log-screens-add-exercise--no-search-text) · [Strength](./?path=/story/workout-log-screens-add-exercise--strength) · [Cardio](./?path=/story/workout-log-screens-add-exercise--cardio) · [Results](./?path=/story/workout-log-screens-add-exercise--results) · [Search with done first](./?path=/story/workout-log-screens-add-exercise--search-with-done-first) · [No match](./?path=/story/workout-log-screens-add-exercise--no-match) · [Loading](./?path=/story/workout-log-screens-add-exercise--loading) · [Load failed](./?path=/story/workout-log-screens-add-exercise--load-failed)

`data-testid`: `add-exercise-overlay`, `add-exercise-back`, `add-exercise-search`, `add-exercise-filter`, `add-exercise-filter-all`, `add-exercise-filter-strength`, `add-exercise-filter-cardio`, `add-exercise-row-{id}`, `add-exercise-group-recent`, `add-exercise-group-other`, `add-exercise-none`, `add-exercise-loading`.
