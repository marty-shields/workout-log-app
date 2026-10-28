---
$attribute: CustomPage("Screens/Exercises/Overview")
---

# Exercises

The exercises you've logged, opened from the Dashboard (FR-60). View: `ExercisesView`, built from `ExerciseList` as [Add Exercise](./?path=/custom/screens-add-exercise-overview) is; page: `ExercisesPage`.

`ExercisesPage` loads the exercises through `ILoggedExercisesService.GetAsync` each time it opens, not on app start, and searches and filters them in memory (D191). A lift or bodyweight row opens its [Exercise](./?path=/custom/screens-exercise-overview) page (D192). Back, or Android back, returns to the Dashboard.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Exercises". |
| Search | Search box with the search `Icon` and "Search exercises". |
| Chips | `FilterChips`: All, Strength, Cardio. |
| Rows | One `ListRow` per exercise: its name, Strength or Cardio muted at the right, and under the name "Last" with the last session's date and best, e.g. *Last Tue 20 Oct · 70kg × 3* (D49). The date has the year when it isn't this year; with no best, the date only. No search text: most recent first; exercises last done in the same workout A–Z. With search text: matches, ranked ([how it matches](https://github.com/marty-shields/workout-log-app/blob/main/docs/02-data-model.md#exercise-search)). Filtered by the chip. A lift or bodyweight row has a chevron and opens its Exercise page; other rows aren't tappable. |
| No match | "No exercises match" under the chips. |
| Nothing logged yet | `EmptyState` "No exercises logged yet", with no search or chips. |
| Loading | `Spinner` "Loading…" centred. |
| Load failed | Error "Couldn't load your exercises" and a Secondary **Try again**. Focus moves to Try again. |

States: [List](./?path=/story/screens-exercises--list) · [Strength](./?path=/story/screens-exercises--strength) · [Cardio](./?path=/story/screens-exercises--cardio) · [No match](./?path=/story/screens-exercises--no-match) · [Nothing logged yet](./?path=/story/screens-exercises--nothing-logged-yet) · [Loading](./?path=/story/screens-exercises--loading) · [Load failed](./?path=/story/screens-exercises--load-failed)

`data-testid`: `back`, `exercises-search`, `exercises-filter`, `exercises-filter-all`, `exercises-filter-strength`, `exercises-filter-cardio`, `exercises-row-{id}`, `exercises-none`, `exercises-empty`, `exercises-loading`, `exercises-error`, `exercises-retry`.
