---
$attribute: CustomPage("Design System/Screens/New Workout")
---

# New Workout

Log a workout as you go (FR-01–FR-14). View: `WorkoutView` with `Mode` New.

| Part | Content |
|---|---|
| Top bar | Back arrow; date and day; "Elapsed h:mm:ss". |
| Content | Workout error, if any; `EmptyState` "No exercises yet" or one `ExerciseCard` per exercise, in the order added. The last set of a carry is open. |
| Bottom bar | Add exercise (secondary) and Save (primary), side by side. |
| Overlays | `ExercisePicker` ([Exercise Picker](/?path=/custom/design-system-screens-exercise-picker)); `ConfirmDialog` "Discard this workout?" on back. |

Back: Discard / Cancel, then Dashboard.

States: [Empty](/?path=/story/screens-new-workout--empty) · [Strength sets](/?path=/story/screens-new-workout--strength-sets) · [Carry sets](/?path=/story/screens-new-workout--carry-sets) · [Cardio entries](/?path=/story/screens-new-workout--cardio-entries) · [Save errors](/?path=/story/screens-new-workout--save-errors) · [Save with no exercises](/?path=/story/screens-new-workout--save-with-no-exercises) · [Discard prompt](/?path=/story/screens-new-workout--discard-prompt)

`data-testid`: `back`, `screen-title`, `screen-subtitle`, `workout-error`, `no-exercises`, `exercise-{n}` (and its parts, see [Workout](/?path=/custom/design-system-components-workout)), `add-exercise`, `save-workout`, `confirm-dialog`.
