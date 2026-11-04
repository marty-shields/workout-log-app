---
$attribute: CustomPage("Components/Workout/Overview")
---

# Workout

## `ExerciseCard`
One exercise in a workout: name, Done, Remove, and its sets (strength) or entry (cardio). Done is a tick `IconButton` (Check) and Remove a muted trash `IconButton` (Delete), both on the right of the name.
- `ExerciseState.IsDone` shows the card as one line: the name, a muted pencil `IconButton` (Edit) and the trash. The sets, entry, last and PB line, last time values and Add set are hidden. The pencil sets `IsDone` false and shows the card with every set closed (the last stays open only when it is empty), except a set with errors, which opens. An exercise error still shows. `IsDone` is not saved and `OnChanged` isn't raised for it.
- Strength: one set open at a time, the others closed as `SetSummary` lines, then Add set. The open set is a `SetRow` under a header, or a `SetPanel` for more than two fields with a duration (carries). A new exercise opens its last set; an exercise opened from a saved workout starts with every set closed unless the last is empty. With `LastTime`, the header gains a muted **Last** column after Set, and the open row shows last time's set in it. A row with a duration and one more field (a hold) has no Last column: last time shows on a muted line under the row instead (see `SetRow`). The open panel of a carry shows a muted *Last time* line. Closed sets show none. Both leave a set number last time didn't have as *—* (row) or nothing (line).
- A set with errors is never closed. Add set, a pencil on a closed set and Done all check the open set first, as Save does, and when it fails they keep it open, show its errors and focus the field at fault. Only a valid set closes, so a closed set never shows errors. A set that already has errors when the card is given its exercise opens.
- Add set then closes the open set and opens the new one. The close button on the open set removes it at once and leaves every set closed. The trash on a closed set raises `OnRemoveClosedSet`; the screen removes it and offers Undo.
- Cardio: one entry with every field labelled. With `LastTime`, a muted line above the fields: *Last time 5.2km · 30:00 · 5:46/km*, the entry's values joined by " · ". No line when `LastTime` has no entry. Pace (Outdoor Run, Walk) or speed (Treadmill, Outdoor Cycle) shows as a muted line under the fields, worked out from distance and duration: *Pace 5:46/km*, *Speed 10.4 km/h*, or a — until both are filled and more than 0.
- With `BestKg` set, a muted line under the name: *Last 62.5kg · PB 70kg*, — for no `LastKg`. Not tappable.
- `LastTime` is what the exercise's last saved workout had, as text (`LastTime.Entries`: one per set, or the one cardio entry), shown on New Workout and never copied into a value. Null is the first time: no column and no line. `LastTime.Entry(n)` gives *—* for a set last time didn't have, for the Last column; `LastTime.EntryOrNull(n)` gives null, for the lines. The Last and PB line above is separate and stays.
- With `LastNote` set, a `LastNote` line directly under that line, or under the name when `BestKg` is not set. Strength and cardio.
- The note button is an `IconButton` (Note) named "Add note", or "Edit note" once there is a note. It is `Muted` with no note, and `Active` (filled round) with one. Strength and carry: at the right of the Add set row, so Add set keeps its width with and without a note. Cardio: on the card footer, right-aligned, on the same line as the muted Speed or Pace text when there is one. It is never in the card header. Tapping it opens the note text box at the bottom of the card. Leaving the text box with a note shows it as one plain line (`NoteField`); a cleared note shows nothing.
- A done card shows neither note nor the note button. They show again when the card is opened, with the text as left.
- `Note` is raised through `OnChanged`, which also clears `NoteError`. `NoteError` shows in the note field, not as the card error; the card's red outline is for `Error`.
- `FocusFirstErrorAsync` ends at the note field when only `NoteError` is set.
- At 10 sets Add set is off, with a muted note "At most 10 sets" under it.
- Add set and opening another set are blocked while the current set is empty or has errors. Focus then moves to its first field at fault.
- `ValidateSet` is `ISetChecker.Check` in the App: one message per field, format messages first, then Core's rules. A cardio entry is checked on Save. Once a set or entry fails a check, every change re-checks it, even after it becomes valid, until it passes Add set or opening another set, or the whole workout passes Save.
- `ValidateDone` is `IWorkoutChecker.CheckExercise` in the App: the Save checks for this exercise only. Done is blocked while it returns false; the card stays open and focus moves to the first error as `FocusFirstErrorAsync` does. Unset: Done is never blocked.
- Haptics, through the injected `IHaptics` ([Haptics](./?path=/custom/foundations-haptics)): Add set that closes a valid open set plays `SetLogged`, Done plays `ExerciseDone`, and Add set or Done blocked by `ValidateSet` or `ValidateDone` plays `Rejected`. Typing, opening another set, removing a set and the pencil play nothing.
- `FocusFirstErrorAsync` (after a failed Save or Done): Add set for an exercise with no sets, Remove for another exercise error, else the first field at fault in the first set or entry with errors.
- With `RecordEntry` set, that set or the cardio entry shows a `RecordBadge`.
- `data-testid`: `TestId`, `{TestId}-done` (open card), `{TestId}-edit` (done card), `{TestId}-remove`, `{TestId}-weights`, `{TestId}-last` (cardio), `{TestId}-error`, `{TestId}-add-set`, `{TestId}-sets-full`, `{TestId}-set-{n}` (and `-edit` on a closed set, `-remove` on any set), `{TestId}-{field}` (cardio), `{TestId}-worked-out`, `{TestId}-last-note`, `{TestId}-add-note`, `{TestId}-note` (and its parts, see [Inputs](./?path=/custom/components-inputs-overview)).

## `LastNote`
The note an exercise had in the last saved workout with it: one muted line, the note `Icon` (`Muted`, `Small`) and the text. Cut with … when long; tap to show the whole note, tap again to cut it. Read-only. Screen readers hear "Last note:" before the text.
- `data-testid`: `TestId`.

## `SetRow`
One set on one row: number, a box per field, remove (`IconButton` Close, muted, small). Column headers come from the card. With `IsRecord`, a `RecordBadge` shows under the row, in line with the fields.
- With `Last`, a muted small text after the number in a Last column the width of a field box: last time's set as the Exercise page writes it, e.g. *60×8*, *25kg×10*; *—* when last time had no set with that number (the caller passes it). Read out as "Last time 60×8".
- With `Last` on a row with a duration (a hold), no column: a muted small full-width line under the row, indented past the number: *Last time 1:30*, or *Last time 12.5kg · 1:30* when a weight was logged. The row's boxes keep the width they have without last time. The caller passes null for a set number last time didn't have, so no line shows.
- `data-testid`: `TestId`, `{TestId}-last`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-record`, `{TestId}-error`.

## `SetPanel`
The open set of a carry: number, remove (`IconButton` Close, muted, small), every field labelled, duration full width. Aqua background; sets muted text, input border and focus to the text colour. With `IsRecord`, a `RecordBadge` shows next to the title, in the text colour.
- With `Last`, a line under the title: *Last time 32kg · 40m · 0:45*. Small, in the panel's text colour. Null (last time had no set with that number) leaves it out.
- `data-testid`: `TestId`, `{TestId}-last`, `{TestId}-{field}`, `{TestId}-remove`, `{TestId}-record`, `{TestId}-error`.

## `SetSummary`
A closed set, one compact line about 40px tall: its number in the set-number column, then the values as typed with units, e.g. *1  8 reps · 60kg*, and on the right a muted, compact (`--touch-target-compact`) pencil `IconButton` (Edit, "Edit {exercise} set {n}") and trash (Delete, "Delete {exercise} set {n}"). A field with `HideZero` (weight on a bodyweight move, hold or carry) leaves out 0, e.g. *1  40m · 0:00:45*. A set with nothing typed reads *Empty*. The pencil raises `OnOpen`, the trash `OnRemove`.
- With `IsRecord`, a `RecordBadge` shows under the line, at its left edge under the set number.
- `data-testid`: `TestId`, `{TestId}-edit`, `{TestId}-remove`, `{TestId}-record`.

## `RecordBadge`
The Record `Icon` (`Success`, small) and "Record" in bold small text, in `--color-success`. Marks the set or cardio entry that beats the exercise's best. Not tappable.
- `ExerciseState.RecordEntry` is the one set or entry of an exercise that shows it, so an exercise has at most one. `ExerciseCard` passes `IsRecord` to its `SetRow`, `SetPanel` or `SetSummary`, and shows the badge under a cardio entry's fields. The page sets `RecordEntry`.
- The badge shows under a row or summary line, so the line below moves down when it appears.
- `data-testid`: `TestId`.

## `EntryErrors`
One message per faulty field under a set or entry, in field order.
- `data-testid`: `TestId`.

Stories: [ExerciseCard](./?path=/docs/components-workout-exercisecard--docs) · [LastNote](./?path=/docs/components-workout-lastnote--docs) · [SetRow](./?path=/docs/components-workout-setrow--docs) · [SetPanel](./?path=/docs/components-workout-setpanel--docs) · [SetSummary](./?path=/docs/components-workout-setsummary--docs) · [RecordBadge](./?path=/docs/components-workout-recordbadge--docs) · [EntryErrors](./?path=/docs/components-workout-entryerrors--docs).
