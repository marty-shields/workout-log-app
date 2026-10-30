---
$attribute: CustomPage("Components/Workout/Overview")
---

# Workout

## `ExerciseCard`
One exercise in a workout: name, Remove, and its sets (strength) or entry (cardio). Remove is a muted trash `IconButton` (Delete), so the name leads.
- Strength with up to two fields, or no duration: one `SetRow` per set and Add set.
- Strength with more than two fields, one a duration (carries): one `SetPanel` open, the other sets as `SetSummary` lines.
- Cardio: one entry with every field labelled. Pace (Outdoor Run, Walk) or speed (Treadmill, Outdoor Cycle) shows as a muted line under the fields, worked out from distance and duration: *Pace 5:46/km*, *Speed 10.4 km/h*, or a — until both are filled and more than 0.
- With `BestKg` set, a muted line under the name: *Last 62.5kg · PB 70kg*, — for no `LastKg`. Not tappable.
- At 10 sets Add set is off, with a muted note "At most 10 sets" under it.
- Add set and opening another set are blocked while the current set is empty or has errors. Focus then moves to its first field at fault.
- `ValidateSet` is `ISetChecker.Check` in the App: one message per field, format messages first, then Core's rules. A cardio entry is checked on Save. Once a set or entry fails a check, every change re-checks it, even after it becomes valid, until it passes Add set or opening another set, or the whole workout passes Save.
- `FocusFirstErrorAsync` (after a failed Save): Add set for an exercise with no sets, Remove for another exercise error, else the first field at fault in the first set or entry with errors, or a closed set's summary line.
- `data-testid`: `TestId`, `{TestId}-remove`, `{TestId}-weights`, `{TestId}-error`, `{TestId}-add-set`, `{TestId}-sets-full`, `{TestId}-set-{n}`, `{TestId}-{field}` (cardio), `{TestId}-worked-out`.

## `SetRow`
One set on one row: number, a box per field, remove (`IconButton` Close, muted, small). Column headers come from the card.
- `data-testid`: `TestId`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-error`.

## `SetPanel`
The open set of a carry: number, remove (`IconButton` Close, muted, small), every field labelled, duration full width. Aqua background; sets muted text, input border and focus to the text colour.
- `data-testid`: `TestId`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-error`.

## `SetSummary`
A closed set as one line, e.g. *40m · 32kg · 0:00:45*, ending in a small muted chevron `Icon`. A field with `HideZero` (weight on a bodyweight move, hold or carry) leaves out 0, e.g. *40m · 0:00:45*. Tap to open. With errors, the line has a red outline and the messages show under it. `FocusAsync` focuses the line.
- `data-testid`: `TestId`, `{TestId}-open`, `{TestId}-error`.

## `EntryErrors`
One message per faulty field under a set or entry, in field order.
- `data-testid`: `TestId`.

Stories: [ExerciseCard](./?path=/docs/components-workout-exercisecard--docs) · [SetRow](./?path=/docs/components-workout-setrow--docs) · [SetPanel](./?path=/docs/components-workout-setpanel--docs) · [SetSummary](./?path=/docs/components-workout-setsummary--docs) · [EntryErrors](./?path=/docs/components-workout-entryerrors--docs).
