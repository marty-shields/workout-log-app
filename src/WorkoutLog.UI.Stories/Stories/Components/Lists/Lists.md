---
$attribute: CustomPage(StoryTitles.Prefix + "/Components/Lists/Overview")
---

# Lists

## `RecordsCard`
A compact kit `Card` titled "Records" with the range as its aside at the right, e.g. *last 3 months*. One kit `ListRow` (headline layout) per `RecordRow`: the label and the value in bold, with the date muted under them. A record with no value shows "—" and no date. The last row has no divider.
- `data-testid`: `TestId`.

## `ExerciseList`
One kit `ListRow` per exercise: its name, "Last" with the last session when it has one, and Strength or Cardio as its `Aside`. "No exercises match" when the list is empty. With `OnPick` the rows are tappable, with a chevron. Used under the search and chips of Add Exercise.
- `data-testid`: `TestId` as the prefix: `{TestId}-row-{id}`, `{TestId}-none`.

Stories: [RecordsCard](./?path=/docs/workout-log-components-lists-recordscard--docs) · [ExerciseList](./?path=/docs/workout-log-components-lists-exerciselist--docs)
