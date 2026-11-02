---
$attribute: CustomPage("Screens/New Workout/Overview")
---

# New Workout

Log a workout as you go. View: `WorkoutView` with `Mode` New; page: `NewWorkoutPage`.

`NewWorkoutPage` starts the workout through `IWorkoutService.StartNew()`, ticks every second from `TimeProvider`, shows `IWorkoutService.Elapsed` as the timer, and renders `WorkoutView`. The date and day come from the start time in the phone's time zone. The timer counts while the phone sleeps and doesn't move when the phone's clock changes.

| Part | Content |
|---|---|
| Top bar | Back arrow; date and day; "Elapsed h:mm:ss". |
| Content | Workout error, if any; `EmptyState` "No exercises yet" or one `ExerciseCard` per exercise, newest first. Sets in a card are in the order added. At 10 exercises, a muted centred note "At most 10 exercises" under the cards. |
| Bottom bar | Add exercise (secondary) and Save (primary), side by side. Add exercise is off at 10 exercises. |
| Overlays | `AddExerciseView` ([Add Exercise](./?path=/custom/screens-add-exercise-overview)); `ConfirmDialog` "Discard this workout?" on back; "Delete {exercise name}?" on an exercise's trash. |

## Exercises
Each card shows the fields its definition declares ([exercise list](https://github.com/marty-shields/workout-log-app/blob/main/docs/exercise-list.md)), with the exercise's label where it has one (e.g. Assist (kg), Resistance, Damper). Card parts: [Workout components](./?path=/custom/components-workout-overview).
- Strength: rows of sets (e.g. weight and reps, duration for Plank), Add set, remove set. Starts with one empty set. Add set does nothing while the last set (or the open set) is empty or has errors; the errors show on that set and focus moves to its first field at fault. At 10 sets Add set is off, with a muted note "At most 10 sets" under it.
- Weight is required (`*`). On a bodyweight move, assisted machine, hold or carry, a blank weight becomes 0 before every check: Add set, opening another set and Save. A lift's blank weight shows "Enter weight".
- Carries (more than two fields, one a duration): one set is open, with every field labelled; the others show as a summary line, e.g. *40m · 32kg · 0:00:45*. Tap a summary to open it, once the open set is valid; Add set opens the new set. The last set is open. Errors on a closed set show under its summary, which gets a red outline.
- Cardio: one entry, duration and the exercise's other metrics, with units. Starts empty; not removable on its own. Outdoor Run and Walk show their pace, and Treadmill and Outdoor Cycle their speed, worked out from distance and duration, as a muted line under the fields: *Pace 5:46/km*, *Speed 10.4 km/h*, or *Pace —* / *Speed —* until both are filled and more than 0. They are never typed.
- A partly filled duration fills in when focus leaves it: empty boxes become 0, minutes and seconds two digits (e.g. *1:00:00*, *0:00:22*). Format errors are checked by [`SetChecker`](https://github.com/marty-shields/workout-log-app/blob/main/src/WorkoutLog.UI/Models/Workout/SetChecker.cs), then Core's rules.
- The trash opens `ConfirmDialog` "Delete {exercise name}?" with a red **Delete** and Cancel, on an open or a done card. Delete takes the exercise out, and is a change in Edit. Cancel, or Android back, closes it. Removing one set stays instant.

## Done
While logging, mark an exercise done so it shrinks to one line and is hard to touch by accident. An open card has a tick beside its name and the trash.
- The tick runs the exercise's Save checks through `IWorkoutChecker.CheckExercise`: every set and entry (cardio too), and "Add at least one set". The workout rules, a repeated exercise and the 10-set limit, stay with Save.
- If a check fails, the card stays open and shows the errors; focus moves to the first one, as Add set does. A new strength exercise with its blank set fails "Enter reps" and stays open.
- A done card is one line: the name, a pencil (edit) and the trash. The pencil opens the full card as it was left, with all its sets and entries and, for a carry, the same open set. Any number of cards can be open.
- Done doesn't reorder: cards stay newest first. Picking an exercise that is done from Add exercise opens it, moves it to the top and adds a set.
- Done is only how the screen shows an exercise. It isn't saved, isn't a change for Edit's "Discard changes?", and records no time. A done exercise saves as it stands.

## Adding an exercise
Add exercise opens [Add Exercise](./?path=/custom/screens-add-exercise-overview), which loads the last sessions through `ILoggedExercisesService.GetAsync` each time it opens. A picked exercise goes on top, and focus moves to its first field, scrolling it into view. Picking an exercise already in the workout moves its card to the top: strength adds a set as Add set does, unless it has 10 sets, and cardio focuses its entry.

A strength exercise just added loads its last and PB weights through `IWeightHistoryService.GetAsync` and shows them under its name, e.g. *Last 60kg · PB 70kg*. Nothing shows while they load, when there are none, or when the load fails; Core logs the failure. On a bodyweight move, hold or carry, 0 kg counts as none: no line when the PB is 0, and *Last —* when the last is 0. They don't change while logging. Picking it again doesn't reload; adding it again after Remove does.

## Last time
While logging, each exercise shows what its last saved workout had, so you know what to beat. It is display only: nothing is copied into a set, and every value is still typed. The values come from `ExerciseState.LastTime`; none means the exercise's first time, and nothing shows. This sits beside the Last and PB line, which stays.

An exercise just added, strength or cardio, loads its last time through `ILastSessionService.GetAsync`: its sets or entry in the saved workout with the latest start that has it. Nothing shows while it loads, and the values don't change while logging. Picking it again doesn't reload; adding it again after Remove does. When the load fails nothing shows and logging works as usual; Core logs Warning 1510 "Loading the last session of exercise {ExerciseId} failed" with the exception, and no workout contents.
- Sets match by number: set 1 shows last time's set 1, set 2 its set 2. A set number last time didn't have, such as set 4 after 3 sets, shows *—*. Last time's extra sets show nowhere.
- Strength rows: a muted **Last** column after Set, as wide as a field, with last time's set as the Exercise page writes it: *60×8* (weight × reps), *1:30*, *20kg · 1:30*. Read out as "Last time 60×8".
- Holds: *1:30*, or *20kg · 1:30* when a weight was logged. Carries: *32kg · 40m · 0:45*. A weight of 0 isn't shown.
- Carries (set panel): a muted *Last time 32kg · 40m · 0:45* line at the top of the open panel and under each closed set's summary; *Last time —* for a set number last time didn't have.
- Cardio: a muted line above the fields with the entry's values, e.g. *Last time 5.2km · 30:00 · 5:46/km*.
- A done card is one line, so its Last column and lines are hidden with its sets and entry. The pencil brings them back.

## Save
Save runs `IWorkoutChecker.Check`: every set and entry, then the workout and each exercise through `IWorkoutService.Validate`.
- Errors show on the exercise, set or entry at fault: each faulty field is outlined, with one message per problem under its set or entry.
- Focus moves to the first error in screen order: Add exercise for the workout error, an exercise's Add set or Remove, the first field at fault, or a closed set's summary line.
- Once a set or entry fails a check, every change re-checks it until it passes Add set or opening another set, or the whole workout passes Save. Adding a set clears "Add at least one set"; adding an exercise clears the workout error. Cardio entries are first checked on Save.

A valid Save calls `IWorkoutService.SaveNewAsync` with `ISetChecker.Read`'s records, oldest first, then raises `OnSaved`; the App goes to the Dashboard. If nothing was saved, the workout error shows "Couldn't save the workout. Try again", the workout stays and focus moves to Save. Taps on Save while a save runs are ignored.

## Back
Back (arrow or Android) opens `ConfirmDialog` "Discard this workout?", even with no exercises. Cancel, or Android back, closes it and the workout stays. Discard logs Information 2701 "New workout discarded with {ExerciseCount} exercises" and raises `OnDiscarded`; the App goes to the Dashboard. Back is ignored while a save runs, after one succeeds and after a discard. Discard runs once; while a save runs or after one succeeds it closes the prompt and does nothing else.

With the soft keyboard open, Android back only hides the keyboard.

States: [Empty](./?path=/story/screens-new-workout--empty) · [Strength](./?path=/story/screens-new-workout--strength) · [Done](./?path=/story/screens-new-workout--done) · [Carry](./?path=/story/screens-new-workout--carry) · [Cardio](./?path=/story/screens-new-workout--cardio) · [Records](./?path=/story/screens-new-workout--records) · [Last time](./?path=/story/screens-new-workout--last-time) · [Last time done](./?path=/story/screens-new-workout--last-time-done) · [Exercises full](./?path=/story/screens-new-workout--exercises-full) · [Save errors](./?path=/story/screens-new-workout--save-errors) · [No exercises error](./?path=/story/screens-new-workout--no-exercises-error) · [Save failed](./?path=/story/screens-new-workout--save-failed) · [Delete exercise prompt](./?path=/story/screens-new-workout--delete-exercise-prompt) · [Discard prompt](./?path=/story/screens-new-workout--discard-prompt)

`data-testid`: `back`, `screen-title`, `screen-subtitle`, `workout-error`, `no-exercises`, `exercises-full`, `exercise-{n}` (and its parts, see [Workout](./?path=/custom/components-workout-overview)), `add-exercise`, `save-workout`, `confirm-dialog`.
