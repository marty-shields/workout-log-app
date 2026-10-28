---
$attribute: CustomPage("Components/Lists/Overview")
---

# Lists

## `ListRow`
A full-width row of text, with an optional muted `Detail` line under it and optional muted `Aside` text at the right. With `OnClick` it is a button ending in a muted chevron `Icon`; without, plain text.
- `data-testid`: `TestId`.

## `StatTile`
A number with a label, on a card. The number uses `--color-accent` at `--font-size-xxl`.
- `data-testid`: `TestId`.

## `EmptyState`
A message in a dashed box, for a list or section with nothing in it.
- `data-testid`: `TestId`.

## `RecordsCard`
A card titled "Records · " and the range, e.g. *Records · last 3 months*. One row per `RecordRow`: label, value in bold and date, muted. A record with no value shows "—" and no date.
- `data-testid`: `TestId`.

## `SessionRow`
One session of an exercise: date and headline in bold, with every set or the cardio entry muted under them. Not tappable.
- `data-testid`: `TestId`.

## `ExerciseList`
Search box with the search `Icon`, `FilterChips` All / Strength / Cardio, then one `ListRow` per exercise: its name, "Last" with the last session when it has one, and Strength or Cardio as its `Aside`. "No exercises match" when the list is empty. With `OnPick` the rows are tappable. Used by Exercises and Add Exercise.
- `data-testid`: `TestId` as the prefix: `{TestId}-search`, `{TestId}-filter`, `{TestId}-filter-all`, `{TestId}-filter-strength`, `{TestId}-filter-cardio`, `{TestId}-row-{id}`, `{TestId}-none`.

Stories: [ListRow](./?path=/docs/components-lists-listrow--docs) · [StatTile](./?path=/docs/components-lists-stattile--docs) · [EmptyState](./?path=/docs/components-lists-emptystate--docs) · [RecordsCard](./?path=/docs/components-lists-recordscard--docs) · [SessionRow](./?path=/docs/components-lists-sessionrow--docs) · [ExerciseList](./?path=/docs/components-lists-exerciselist--docs)
