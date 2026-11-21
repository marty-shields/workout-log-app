---
$attribute: CustomPage("Components/Lists/Overview")
---

# Lists

## `RecordsCard`
A kit `Card` titled "Records" with the range as its aside at the right, e.g. *last 3 months*. One row per `RecordRow`: label, value in bold and date, muted. A record with no value shows "—" and no date.
- `data-testid`: `TestId`.

## `SessionRow`
One session of an exercise: date and headline in bold, with every set or the cardio entry muted under them. Not tappable.
- `data-testid`: `TestId`.

## `AddExerciseSearch`
Search box with the search `Icon`, then `FilterChips` All / Strength / Cardio. No wrapper: the screen lays them out, in its toolbar or above an `ExerciseList`. Typing past the catalog's search limit is cut to it. With `FocusSearch` the search box takes focus when first shown. Used by Add Exercise.
- `data-testid`: `TestId` as the prefix: `{TestId}-search`, `{TestId}-filter`, `{TestId}-filter-all`, `{TestId}-filter-strength`, `{TestId}-filter-cardio`.

## `ExerciseList`
One kit `ListRow` per exercise: its name, "Last" with the last session when it has one, and Strength or Cardio as its `Aside`. "No exercises match" when the list is empty. With `OnPick` the rows are tappable, with a chevron. Used under an `AddExerciseSearch` by Add Exercise.
- `data-testid`: `TestId` as the prefix: `{TestId}-row-{id}`, `{TestId}-none`.

Stories: [RecordsCard](./?path=/docs/components-lists-recordscard--docs) · [SessionRow](./?path=/docs/components-lists-sessionrow--docs) · [AddExerciseSearch](./?path=/docs/components-lists-addexercisesearch--docs) · [ExerciseList](./?path=/docs/components-lists-exerciselist--docs)
