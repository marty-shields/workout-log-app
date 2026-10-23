---
$attribute: CustomPage("Design System/Screens/New Workout")
---

# New Workout

Log a workout as you go (FR-01–FR-14). View: `WorkoutView` with `Mode` New.

`NewWorkoutPage` starts the workout through `IWorkoutService.StartNew()`, ticks the timer every second from `TimeProvider`, and renders `WorkoutView`. The date and day come from the start time in the phone's time zone; the timer shows 0:00:00 while the clock is before the start.

| Part | Content |
|---|---|
| Top bar | Back arrow; date and day; "Elapsed h:mm:ss". |
| Content | Workout error, if any; `EmptyState` "No exercises yet" or one `ExerciseCard` per exercise, in the order added. The last set of a carry is open. |
| Bottom bar | Add exercise (secondary) and Save (primary), side by side. |
| Overlays | `ExercisePicker` ([Exercise Picker](/?path=/custom/design-system-screens-exercise-picker)); `ConfirmDialog` "Discard this workout?" on back. |

Save runs `IWorkoutChecker.Check` (FR-09): every set and entry, then the workout and each exercise through `IWorkoutService.Validate`. With errors, focus moves to the first one in screen order (D171): Add exercise for the workout error, otherwise the first card at fault. Adding an exercise clears the workout error (D108).

A valid Save calls `IWorkoutService.SaveNewAsync` with `ISetChecker.Read`'s records, then raises `OnSaved`; the App goes to the Dashboard. If nothing was saved, the workout error shows "Couldn't save the workout. Try again." and focus moves to Save (D176). Taps on Save while a save runs are ignored (D177).

Back (arrow or Android) opens `ConfirmDialog` "Discard this workout?", even with no exercises (FR-12). Cancel, or Android back, closes it and the workout stays. Discard logs Information 2701 "New workout discarded with {ExerciseCount} exercises" and raises `OnDiscarded`; the App goes to the Dashboard. Discard runs once. Back is ignored while a save runs, after one succeeds and after a discard; Discard then closes the prompt and does nothing else (D179).

States: [Empty](/?path=/story/screens-new-workout--empty) · [Live timer](/?path=/story/screens-new-workout--live-timer) · [Strength sets](/?path=/story/screens-new-workout--strength-sets) · [Carry sets](/?path=/story/screens-new-workout--carry-sets) · [Cardio entries](/?path=/story/screens-new-workout--cardio-entries) · [Save errors](/?path=/story/screens-new-workout--save-errors) · [Save with no exercises](/?path=/story/screens-new-workout--save-with-no-exercises) · [Save failed](/?path=/story/screens-new-workout--save-failed) · [Discard prompt](/?path=/story/screens-new-workout--discard-prompt)

`data-testid`: `back`, `screen-title`, `screen-subtitle`, `workout-error`, `no-exercises`, `exercise-{n}` (and its parts, see [Workout](/?path=/custom/design-system-components-workout)), `add-exercise`, `save-workout`, `confirm-dialog`.
