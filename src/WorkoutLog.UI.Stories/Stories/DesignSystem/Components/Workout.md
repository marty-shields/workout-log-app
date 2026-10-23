---
$attribute: CustomPage("Design System/Components/Workout")
---

# Workout

## `ExerciseCard`
One exercise in a workout: name, Remove, and its sets (strength) or entry (cardio). Remove is small muted text, so the name leads (D120).
- Strength with up to two fields, or no duration: one `SetRow` per set and Add set.
- Strength with more than two fields, one a duration (carries): one `SetPanel` open, the other sets as `SetSummary` lines (D105).
- Cardio: one entry with every field labelled.
- Add set and opening another set are blocked while the current set is empty or has errors (D106).
- `data-testid`: `TestId`, `{TestId}-remove`, `{TestId}-error`, `{TestId}-add-set`, `{TestId}-set-{n}`, `{TestId}-{field}` (cardio).

## `SetRow`
One set on one row: number, a box per field, remove. Column headers come from the card.
- `data-testid`: `TestId`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-error`.

## `SetPanel`
The open set of a carry: every field labelled, duration full width. Aqua background; sets muted text, input border and focus to the text colour.
- `data-testid`: `TestId`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-error`.

## `SetSummary`
A closed set as one line, e.g. *40 m · 32 kg · 0:00:45*. Tap to open. With errors, the line has a red outline and the messages show under it (D118).
- `data-testid`: `TestId`, `{TestId}-open`, `{TestId}-error`.

## `EntryErrors`
One message per faulty field under a set or entry, in field order (D103).
- `data-testid`: `TestId`.

## `ExercisePicker`
Full-screen overlay: back, search box, results grouped Strength / Cardio. The back arrow and Android back raise `OnClose`. Focus moves to the search box when it opens (D109).
- `data-testid`: `exercise-picker`, `picker-back`, `picker-search`, `picker-group-{kind}`, `picker-item-{id}`, `picker-none`.

Stories: [ExerciseCard](/?path=/docs/components-workout-exercisecard--docs) · [SetRow](/?path=/docs/components-workout-setrow--docs) · [SetPanel](/?path=/docs/components-workout-setpanel--docs) · [SetSummary](/?path=/docs/components-workout-setsummary--docs) · [EntryErrors](/?path=/docs/components-workout-entryerrors--docs) · [ExercisePicker](/?path=/docs/components-workout-exercisepicker--docs)
