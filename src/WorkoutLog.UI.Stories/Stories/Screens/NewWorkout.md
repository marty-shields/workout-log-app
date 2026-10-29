---
$attribute: CustomPage("Screens/New Workout/Overview")
---

# New Workout

Log a workout as you go (FR-01–FR-15). View: `WorkoutView` with `Mode` New; page: `NewWorkoutPage`.

`NewWorkoutPage` starts the workout through `IWorkoutService.StartNew()`, ticks every second from `TimeProvider`, shows `IWorkoutService.Elapsed` as the timer, and renders `WorkoutView`. The date and day come from the start time in the phone's time zone. The timer counts while the phone sleeps and doesn't move when the phone's clock changes (D199).

| Part | Content |
|---|---|
| Top bar | Back arrow; date and day; "Elapsed h:mm:ss". |
| Content | Workout error, if any; `EmptyState` "No exercises yet" or one `ExerciseCard` per exercise, newest first (D42). Sets in a card are in the order added. At 10 exercises, a muted centred note "At most 10 exercises" under the cards (D198). |
| Bottom bar | Add exercise (secondary) and Save (primary), side by side. Add exercise is off at 10 exercises (D198). |
| Overlays | `AddExerciseView` ([Add Exercise](./?path=/custom/screens-add-exercise-overview)); `ConfirmDialog` "Discard this workout?" on back. |

## Exercises
Each card shows the fields its definition declares ([exercise list](https://github.com/marty-shields/workout-log-app/blob/main/docs/03-exercise-list.md)), with the exercise's label where it has one (e.g. Assist (kg), Resistance, Damper). Card parts: [Workout components](./?path=/custom/components-workout-overview).
- Strength: rows of sets (e.g. reps and weight, duration for Plank), Add set, remove set. Starts with one empty set. Add set does nothing while the last set (or the open set) is empty or has errors; the errors show on that set and focus moves to its first field at fault (D37). At 10 sets Add set is off, with a muted note "At most 10 sets" under it (D198).
- Weight is required (`*`). On a bodyweight move, assisted machine, hold or carry, a blank weight becomes 0 before every check: Add set, opening another set and Save. A lift's blank weight shows "Enter weight" (D23).
- Carries (more than two fields, one a duration): one set is open, with every field labelled; the others show as a summary line, e.g. *40m · 32kg · 0:00:45*. Tap a summary to open it, once the open set is valid; Add set opens the new set. The last set is open. Errors on a closed set show under its summary, which gets a red outline (D80).
- Cardio: one entry, duration and the exercise's other metrics, with units. Starts empty; not removable on its own. Outdoor Run and Walk show their pace, and Treadmill and Outdoor Cycle their speed, worked out from distance and duration, as a muted line under the fields: *Pace 5:46/km*, *Speed 10.4 km/h*, or *Pace —* / *Speed —* until both are filled and more than 0 (D197). They are never typed.
- A partly filled duration fills in when focus leaves it: empty boxes become 0, minutes and seconds two digits (e.g. *1:00:00*, *0:00:22*) (D31). Format errors and Core's rules: [entry formats](https://github.com/marty-shields/workout-log-app/blob/main/docs/02-data-model.md#entry-formats-ui).
- Remove takes the exercise out at once, with no confirm (D43).

## Adding an exercise
Add exercise opens [Add Exercise](./?path=/custom/screens-add-exercise-overview), which loads the last sessions through `ILoggedExercisesService.GetAsync` each time it opens. A picked exercise goes on top, and focus moves to its first field, scrolling it into view (D42). Picking an exercise already in the workout moves its card to the top: strength adds a set as Add set does, unless it has 10 sets, and cardio focuses its entry (D42, D198).

A strength exercise just added loads its last and PB weights through `IWeightHistoryService.GetAsync` and shows them under its name, e.g. *Last 60kg · PB 70kg* (FR-15, D48). Nothing shows while they load, when there are none, or when the load fails; Core logs the failure. On a bodyweight move, hold or carry, 0 kg counts as none: no line when the PB is 0, and *Last —* when the last is 0. They don't change while logging. Picking it again doesn't reload; adding it again after Remove does.

## Save
Save runs `IWorkoutChecker.Check` (FR-09): every set and entry, then the workout and each exercise through `IWorkoutService.Validate`.
- Errors show on the exercise, set or entry at fault: each faulty field is outlined, with one message per problem under its set or entry (D36).
- Focus moves to the first error in screen order (D40): Add exercise for the workout error, an exercise's Add set or Remove, the first field at fault, or a closed set's summary line.
- Once a set or entry fails a check, every change re-checks it until it passes Add set or opening another set, or the whole workout passes Save (D38). Adding a set clears "Add at least one set"; adding an exercise clears the workout error (D38). Cardio entries are first checked on Save (D39).

A valid Save calls `IWorkoutService.SaveNewAsync` with `ISetChecker.Read`'s records, oldest first, then raises `OnSaved`; the App goes to the Dashboard. If nothing was saved, the workout error shows "Couldn't save the workout. Try again", the workout stays and focus moves to Save (D40). Taps on Save while a save runs are ignored (D41).

## Back
Back (arrow or Android) opens `ConfirmDialog` "Discard this workout?", even with no exercises (FR-12). Cancel, or Android back, closes it and the workout stays. Discard logs Information 2701 "New workout discarded with {ExerciseCount} exercises" and raises `OnDiscarded`; the App goes to the Dashboard. Back is ignored while a save runs, after one succeeds and after a discard. Discard runs once; while a save runs or after one succeeds it closes the prompt and does nothing else (D45).

With the soft keyboard open, Android back only hides the keyboard (D78).

States: [Empty](./?path=/story/screens-new-workout--empty) · [Strength](./?path=/story/screens-new-workout--strength) · [Carry](./?path=/story/screens-new-workout--carry) · [Cardio](./?path=/story/screens-new-workout--cardio) · [Exercises full](./?path=/story/screens-new-workout--exercises-full) · [Save errors](./?path=/story/screens-new-workout--save-errors) · [No exercises error](./?path=/story/screens-new-workout--no-exercises-error) · [Save failed](./?path=/story/screens-new-workout--save-failed) · [Discard prompt](./?path=/story/screens-new-workout--discard-prompt)

`data-testid`: `back`, `screen-title`, `screen-subtitle`, `workout-error`, `no-exercises`, `exercises-full`, `exercise-{n}` (and its parts, see [Workout](./?path=/custom/components-workout-overview)), `add-exercise`, `save-workout`, `confirm-dialog`.
