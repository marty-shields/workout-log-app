---
$attribute: CustomPage("Screens/Add Exercise/Overview")
---

# Add Exercise

Add an exercise to the workout. View: `ExercisesView`, the one list of exercises, an overlay on `WorkoutView` with the same `AppScreen` shell. It is not a route: the app has no Exercises screen, and an exercise's page opens from Trends or This week. `WorkoutEditor` holds its search, chip and rows, and loads the last sessions through `ILoggedExercisesService.GetAsync` each time it opens.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Exercises". |
| Toolbar | `ExerciseSearch`, pinned under the top bar with a line under it; the rows scroll beneath. Shows from the moment the list opens, while the last sessions load too. |
| Search | Search box with the search `Icon` and "Search exercises". Empty each time the list opens, with focus. Text typed while loading is kept. |
| Chips | `FilterChips`: All, Strength, Cardio. All each time the list opens. |
| Rows | Every catalog exercise, as a tappable `ListRow` showing Strength or Cardio muted before the chevron. No search text: Strength then Cardio, A–Z in each. With search text: matches, ranked : every query word, in any order, starts a word of the name or a nickname; names that start with the query first, then other name matches, then nickname matches. Filtered by the chip. A logged exercise shows "Last" with its last session's date and best under its name, e.g. *Last Tue 20 Oct · 70kg × 3*. The best: a lift's top set (heaviest; tie → most reps); a bodyweight move's most reps, or its top set when a set is over 0 kg; an assisted machine's least assist (tie → most reps); a hold's longest; a carry's heaviest set over 0 kg with its distance (tie → longest); cardio's distance when the exercise has it, else its duration. The date has the year when it isn't this year; with no best, the date only. Exercises already in the workout still show. |
| No match | "No exercises match" under the toolbar. |
| Loading | `Spinner` "Loading…" centred under the toolbar, in place of the rows, while the last sessions load. |
| Load failed | The rows with no "Last" lines and no message; adding still works. |

Tapping an exercise adds it to the workout and closes the list. Picking one already in the workout moves its card to the top instead of adding it again. With 10 exercises, Add exercise is off, so the list can't open; see [New Workout](./?path=/custom/screens-new-workout-overview).

Back closes the list; with the soft keyboard open, Android back first only hides the keyboard. No tab bar and no pull to refresh.

States: [No search text](./?path=/story/screens-add-exercise--no-search-text) · [Strength](./?path=/story/screens-add-exercise--strength) · [Cardio](./?path=/story/screens-add-exercise--cardio) · [Results](./?path=/story/screens-add-exercise--results) · [No match](./?path=/story/screens-add-exercise--no-match) · [Loading](./?path=/story/screens-add-exercise--loading) · [Load failed](./?path=/story/screens-add-exercise--load-failed)

`data-testid`: `exercises-overlay`, `exercises-back`, `exercises-search`, `exercises-filter`, `exercises-filter-all`, `exercises-filter-strength`, `exercises-filter-cardio`, `exercises-row-{id}`, `exercises-none`, `exercises-loading`.
