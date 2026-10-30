---
$attribute: CustomPage("Screens/Add Exercise/Overview")
---

# Add Exercise

Add an exercise to the workout. View: `AddExerciseView`, an overlay on `WorkoutView`, built from `ExerciseList` as the [Exercises](./?path=/custom/screens-exercises-overview) screen is. `WorkoutEditor` holds its search, chip and rows, and loads the last sessions through `ILoggedExercisesService.GetAsync` each time it opens.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Exercises". |
| Search | Search box with the search `Icon` and "Search exercises". Empty each time the list opens; it has focus once the list shows. |
| Chips | `FilterChips`: All, Strength, Cardio. All each time the list opens. |
| Rows | Every catalog exercise, as a tappable `ListRow` showing Strength or Cardio muted before the chevron. No search text: Strength then Cardio, A–Z in each. With search text: matches, ranked : every query word, in any order, starts a word of the name or a nickname; names that start with the query first, then other name matches, then nickname matches. Filtered by the chip. A logged exercise shows "Last" with its last session's date and best under its name, as on Exercises. Exercises already in the workout still show. |
| No match | "No exercises match" under the chips. |
| Loading | `Spinner` "Loading…" centred, while the last sessions load. |
| Load failed | The rows with no "Last" lines and no message; adding still works. |

Tapping an exercise adds it to the workout and closes the list. Picking one already in the workout moves its card to the top instead of adding it again. With 10 exercises, Add exercise is off, so the list can't open; see [New Workout](./?path=/custom/screens-new-workout-overview).

Back closes the list; with the soft keyboard open, Android back first only hides the keyboard.

States: [No search text](./?path=/story/screens-add-exercise--no-search-text) · [Strength](./?path=/story/screens-add-exercise--strength) · [Cardio](./?path=/story/screens-add-exercise--cardio) · [Results](./?path=/story/screens-add-exercise--results) · [No match](./?path=/story/screens-add-exercise--no-match) · [Loading](./?path=/story/screens-add-exercise--loading) · [Load failed](./?path=/story/screens-add-exercise--load-failed)

`data-testid`: `exercises-overlay`, `exercises-back`, `exercises-search`, `exercises-filter`, `exercises-filter-all`, `exercises-filter-strength`, `exercises-filter-cardio`, `exercises-row-{id}`, `exercises-none`, `exercises-loading`.
