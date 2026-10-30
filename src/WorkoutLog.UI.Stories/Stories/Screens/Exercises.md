---
$attribute: CustomPage("Screens/Exercises/Overview")
---

# Exercises

The exercises you've logged, opened from the Dashboard. View: `ExercisesView`, built from `ExerciseList` as [Add Exercise](./?path=/custom/screens-add-exercise-overview) is; page: `ExercisesPage`.

`ExercisesPage` loads the exercises through `ILoggedExercisesService.GetAsync` each time it opens, not on app start, and searches and filters them in memory. Every row opens its [Exercise](./?path=/custom/screens-exercise-overview) page. Back, or Android back, returns to the Dashboard.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Exercises". |
| Search | Search box with the search `Icon` and "Search exercises". |
| Chips | `FilterChips`: All, Strength, Cardio. |
| Rows | One `ListRow` per exercise: its name, Strength or Cardio muted at the right, and under the name "Last" with the last session's date and best, e.g. *Last Tue 20 Oct · 70kg × 3*. The best: a lift's top set (heaviest; tie → most reps); a bodyweight move's most reps, or its top set when a set is over 0 kg; an assisted machine's least assist (tie → most reps); a hold's longest; a carry's heaviest set over 0 kg with its distance (tie → longest); cardio's distance when the exercise has it, else its duration. The date has the year when it isn't this year; with no best, the date only. No search text: most recent first; exercises last done in the same workout A–Z. With search text: matches, ranked : every query word, in any order, starts a word of the name or a nickname; names that start with the query first, then other name matches, then nickname matches. Filtered by the chip. Every row has a chevron and opens its Exercise page. |
| No match | "No exercises match" under the chips. |
| Nothing logged yet | `EmptyState` "No exercises logged yet", with no search or chips. |
| Loading | `Spinner` "Loading…" centred. |
| Load failed | Error "Couldn't load your exercises" and a Secondary **Try again**. Focus moves to Try again. |

States: [List](./?path=/story/screens-exercises--list) · [Strength](./?path=/story/screens-exercises--strength) · [Cardio](./?path=/story/screens-exercises--cardio) · [No match](./?path=/story/screens-exercises--no-match) · [Nothing logged yet](./?path=/story/screens-exercises--nothing-logged-yet) · [Loading](./?path=/story/screens-exercises--loading) · [Load failed](./?path=/story/screens-exercises--load-failed)

`data-testid`: `back`, `exercises-search`, `exercises-filter`, `exercises-filter-all`, `exercises-filter-strength`, `exercises-filter-cardio`, `exercises-row-{id}`, `exercises-none`, `exercises-empty`, `exercises-loading`, `exercises-error`, `exercises-retry`.
