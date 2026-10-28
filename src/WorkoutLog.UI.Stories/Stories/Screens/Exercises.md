---
$attribute: CustomPage("Screens/Exercises/Overview")
---

# Exercises

Logged exercises, to open one's progress. View: `ExercisesView`. A design only: the App doesn't use it yet (D182).

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Exercises". |
| Search | Search box with the search `Icon` and "Search exercises". |
| Chips | `FilterChips`: All, Strength, Cardio. |
| Rows | One tappable `ListRow` per exercise: its name, and under it "Last" with the last session's date and best, e.g. *Last Tue 20 Oct · 70kg × 3* (D49). |
| No match | "No exercises match" under the chips. |
| Nothing logged yet | `EmptyState` "No exercises logged yet", with no search or chips. |
| Loading | `Spinner` "Loading…" centred. |
| Load failed | Error "Couldn't load your exercises" and a Secondary **Try again**. Focus moves to Try again. |

Tapping a row opens its [Exercise](./?path=/custom/screens-exercise-overview) screen.

States: [List](./?path=/story/screens-exercises--list) · [Strength](./?path=/story/screens-exercises--strength) · [Cardio](./?path=/story/screens-exercises--cardio) · [No match](./?path=/story/screens-exercises--no-match) · [Nothing logged yet](./?path=/story/screens-exercises--nothing-logged-yet) · [Loading](./?path=/story/screens-exercises--loading) · [Load failed](./?path=/story/screens-exercises--load-failed)

`data-testid`: `back`, `exercises-search`, `exercises-filter`, `exercises-filter-all`, `exercises-filter-strength`, `exercises-filter-cardio`, `exercises-row-{id}`, `exercises-none`, `exercises-empty`, `exercises-loading`, `exercises-error`, `exercises-retry`.
