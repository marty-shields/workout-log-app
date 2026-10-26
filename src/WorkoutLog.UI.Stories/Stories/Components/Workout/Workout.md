---
$attribute: CustomPage("Components/Workout/Overview")
---

# Workout

## `ExerciseCard`
One exercise in a workout: name, Remove, and its sets (strength) or entry (cardio). Remove is small muted text, so the name leads (D120).
- Strength with up to two fields, or no duration: one `SetRow` per set and Add set.
- Strength with more than two fields, one a duration (carries): one `SetPanel` open, the other sets as `SetSummary` lines (D105).
- Cardio: one entry with every field labelled. Pace is two boxes, m:ss.
- With `BestKg` set, a muted line under the name: *Last 62.5kg · PB 70kg*, — for no `LastKg` (FR-15, D237). Not tappable.
- Add set and opening another set are blocked while the current set is empty or has errors (D106). Focus then moves to its first field at fault (D162).
- `ValidateSet` is `ISetChecker.Check` in the App: one message per field, format messages first, then Core's rules (D157–D163, D167, D169). A cardio entry is checked on Save (D166). Once a set or entry fails a check, every change re-checks it, even after it becomes valid, until it passes Add set or opening another set, or the whole workout passes Save (D174).
- `FocusFirstErrorAsync` (after a failed Save): Add set for an exercise with no sets, Remove for another exercise error, else the first field at fault in the first set or entry with errors, or a closed set's summary line (D171).
- `data-testid`: `TestId`, `{TestId}-remove`, `{TestId}-weights`, `{TestId}-error`, `{TestId}-add-set`, `{TestId}-set-{n}`, `{TestId}-{field}` (cardio).

## `SetRow`
One set on one row: number, a box per field, remove. Column headers come from the card.
- `data-testid`: `TestId`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-error`.

## `SetPanel`
The open set of a carry: every field labelled, duration full width. Aqua background; sets muted text, input border and focus to the text colour.
- `data-testid`: `TestId`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-error`.

## `SetSummary`
A closed set as one line, e.g. *40m · 32kg · 0:00:45*. Tap to open. With errors, the line has a red outline and the messages show under it (D118). `FocusAsync` focuses the line.
- `data-testid`: `TestId`, `{TestId}-open`, `{TestId}-error`.

## `EntryErrors`
One message per faulty field under a set or entry, in field order (D103).
- `data-testid`: `TestId`.

## `ExercisePicker`
Full-screen overlay: back, search box, results grouped Strength / Cardio. The back arrow and Android back raise `OnClose`. Focus moves to the search box when it opens (D109).
- `data-testid`: `exercise-picker`, `picker-back`, `picker-search`, `picker-group-{kind}`, `picker-item-{id}`, `picker-none`.

Stories: [ExerciseCard](./?path=/docs/components-workout-exercisecard--docs) · [SetRow](./?path=/docs/components-workout-setrow--docs) · [SetPanel](./?path=/docs/components-workout-setpanel--docs) · [SetSummary](./?path=/docs/components-workout-setsummary--docs) · [EntryErrors](./?path=/docs/components-workout-entryerrors--docs). `ExercisePicker` is shown on its screen: [Exercise Picker](./?path=/custom/screens-exercise-picker-overview).
