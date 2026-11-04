---
$attribute: CustomPage("Screens/New Workout/Overview")
---

# New Workout

Log a workout as you go. View: `WorkoutView` with `Mode` New; page: `NewWorkoutPage`.

`NewWorkoutPage` starts the workout through `IWorkoutService.StartNew()`, ticks every second from `TimeProvider`, shows `IWorkoutService.Elapsed` as the timer, and renders `WorkoutView`. The date and day come from the start time in the phone's time zone. The timer counts while the phone sleeps and doesn't move when the phone's clock changes.

| Part | Content |
|---|---|
| Top bar | Back arrow; date and day; "Elapsed h:mm:ss"; the note icon button. |
| Content | Workout error, if any; the workout note (`NoteField`), once there is one; `EmptyState` "No exercises yet", hint "Tap Add exercise to start logging", or one `ExerciseCard` per exercise, newest first. Sets in a card are in the order added. At 10 exercises, a muted centred note "At most 10 exercises" under the cards. |
| Bottom bar | Add exercise (secondary) and Save (primary), side by side. Add exercise is off at 10 exercises. |
| Snackbar | After a closed set or an exercise is deleted: `Snackbar` "{exercise name} set {n} deleted" or "{exercise name} deleted" with **Undo**, just above the bottom bar (see [Deleting](#deleting)). |
| Overlays | `AddExerciseView` ([Add Exercise](./?path=/custom/screens-add-exercise-overview)); `ConfirmDialog` "Discard this workout?" on back. |

## Exercises
Each card shows the fields its definition declares ([exercise list](https://github.com/marty-shields/workout-log-app/blob/main/docs/exercise-list.md)), with the exercise's label where it has one (e.g. Assist (kg), Resistance, Damper). Card parts: [Workout components](./?path=/custom/components-workout-overview).
- Strength: rows of sets (e.g. weight and reps, duration for Plank), Add set, remove set. Starts with one empty set. One set is open at a time and the others show as a summary line (see below). Add set does nothing while the open set is empty or has errors; the errors show on that set and focus moves to its first field at fault. Otherwise it closes the open set and opens the new one. At 10 sets Add set is off, with a muted note "At most 10 sets" under it.
- Weight is required (`*`). On a bodyweight move, assisted machine, hold or carry, a blank weight becomes 0 before every check: Add set, opening another set and Save. A lift's blank weight shows "Enter weight".
- A closed set is a summary line, e.g. *1  8 reps · 60kg* or *1  40m · 32kg · 0:00:45*, with a pencil and a trash. The pencil opens it, once the open set is valid; an open set with errors stays open and shows them. The trash deletes it at once, with Undo (see [Deleting](#deleting)). The close button on the open set removes it at once, with no Undo, and leaves every set closed. Carries (more than two fields, one a duration) open as a panel with every field labelled; other exercises open as one row. A set with errors is never closed, so a closed set shows none.
- Cardio: one entry, duration and the exercise's other metrics, with units. Starts empty; not removable on its own. Outdoor Run and Walk show their pace, and Treadmill and Outdoor Cycle their speed, worked out from distance and duration, as a muted line under the fields: *Pace 5:46/km*, *Speed 10.4 km/h*, or *Pace —* / *Speed —* until both are filled and more than 0. They are never typed.
- A partly filled duration fills in when focus leaves it: empty boxes become 0, minutes and seconds two digits (e.g. *1:00:00*, *0:00:22*). Format errors are checked by [`SetChecker`](https://github.com/marty-shields/workout-log-app/blob/main/src/WorkoutLog.UI/Models/Workout/SetChecker.cs), then Core's rules.
- The trash, on an open or a done card, deletes the exercise at once, with Undo (see [Deleting](#deleting)).

## Deleting
Deleting a closed set or an exercise doesn't ask first: a dialog on every delete gets tapped through by habit, and Undo protects better. Discarding the whole workout still asks.
- The set or exercise goes at once, and `Snackbar` shows "Barbell Back Squat set 2 deleted" or "Barbell Back Squat deleted" with **Undo**, just above the bottom bar, over the cards. It doesn't cover Add exercise or Save and doesn't block the screen. Focus moves to **Undo**, since the trash that had it is gone. The 10 seconds keep running after a tap and stop while Undo has keyboard focus.
- **Undo** puts it back where it was, with its values and notes; a set comes back closed and an exercise done or open as it was (an open exercise opens its sets as when it was added). A set goes back under the sets that were above it, even if one of them was removed meanwhile, and an exercise under the exercises that were above it, even if one was added at the top meanwhile. Focus moves to what came back: a set's pencil, or an exercise's first button (Mark done, or Edit when done); a set of an exercise marked done since focuses its Edit. An error the delete caused goes with it: an exercise put back clears "Add at least one exercise", a set put back clears its exercise's "Add at least one set". The delete is still a change in Edit.
- One at a time: the next delete replaces the snackbar, and the earlier one stays deleted. The snackbar goes, and the delete stays, after 10 seconds, when Save saves the workout, when the screen closes, and when Undo would no longer fit: a set added to fill the exercise to 10 sets, an exercise added to make 10, or the deleted exercise picked again. Opening Add exercise and closing it without a pick keeps it. A Save blocked by errors or one that fails keeps it, so an exercise deleted by mistake can still come back. The snackbar hides while a save runs, which has read the workout without the delete, and a failed save shows it again with a fresh 10 seconds, leaving focus on Save.
- Android back keeps its meaning while the snackbar shows (see [Back](#back)); it doesn't just hide the snackbar.
- Removing the open set with its close button has no Undo: it is the set being typed, usually empty or added by mistake.
- The model is `WorkoutEditor`: `Remove` and `RemoveSet` keep the delete in `PendingUndo`, `Undo` puts it back, and `DismissUndo` ends it when its snackbar times out.

## Done
While logging, mark an exercise done so it shrinks to one line and is hard to touch by accident. An open card has a tick beside its name and the trash.
- The tick runs the exercise's Save checks through `IWorkoutChecker.CheckExercise`: every set and entry (cardio too), its note, and "Add at least one set". The workout rules, a repeated exercise and the 10-set limit, stay with Save.
- If a check fails, the card stays open and shows the errors; focus moves to the first one, as Add set does. A new strength exercise with its blank set fails "Enter reps" and stays open.
- A done card is one line: the name, a pencil (edit) and the trash. The pencil opens the full card with all its sets and entries, every set closed (a set with errors opens, as does an empty last set). Any number of cards can be open.
- Done doesn't reorder: cards stay newest first. Picking an exercise that is done from Add exercise opens it, moves it to the top and adds a set.
- Done is only how the screen shows an exercise. It isn't saved, isn't a change for Edit's "Discard changes?", and records no time. A done exercise saves as it stands.

## Notes
Written like the margin notes of a paper notebook: one on the workout and one on each exercise. Parts: [Inputs](./?path=/custom/components-inputs-overview) and [Workout components](./?path=/custom/components-workout-overview).
- Workout note: a note icon button in the top bar, named "Add note" or, with a note, "Edit note". It opens a text box under the top bar, above the cards, with a counter, e.g. *120/500*. Leaving the text box shows the note as one plain line, cut with …; tap to edit. Cleared or blank means no note, and nothing shows. With no note the screen has no empty note row.
- Exercise note: one per exercise, on its card. The note icon button sits at the right of the Add set row (cardio: on the footer, right-aligned, beside the Speed or Pace line). It opens a text box at the bottom of the card, with the same counter and the same one plain line when you leave it. The button stays once there is a note, named "Edit note" and filled.
- Last time's note: an exercise just added shows the note it had in the last saved workout with it, as one muted line with the note icon under its last and PB line, or under its name when there is no weights line. Cut with … when long; tap to show the whole note, tap again to cut it. It cannot be edited and does not change while logging.
- A done card hides its notes and last time's note. They show again when the card is opened.
- Limit: 500 characters each, trimmed; blank is no note. Save checks each note through `IWorkoutService.Validate`. Over 500 shows "Note must be 500 characters or less" under the field, which gets a red border, and the counter turns red. The message goes away when that note changes. The tick on an exercise checks its note too.
- A note is saved with the workout or its exercise and is never logged.

## Adding an exercise
Add exercise opens [Add Exercise](./?path=/custom/screens-add-exercise-overview), which loads the last sessions through `ILoggedExercisesService.GetAsync` each time it opens. A picked exercise goes on top, and focus moves to its first field, scrolling it into view. Picking an exercise already in the workout moves its card to the top: strength adds a set as Add set does, unless it has 10 sets, and cardio focuses its entry.

An exercise just added, strength or cardio, loads its history through `IWeightHistoryService.GetAsync`. A strength exercise shows its last and PB weights under its name, e.g. *Last 60kg · PB 70kg*. Either kind shows last time's note when it has one. Nothing shows while they load, when there are none, or when the load fails; Core logs the failure. On a bodyweight move, hold or carry, 0 kg counts as none: no line when the PB is 0, and *Last —* when the last is 0. They don't change while logging. Picking it again doesn't reload; adding it again after Remove does.

## Last time
While logging, each exercise shows what its last saved workout had, so you know what to beat. It is display only: nothing is copied into a set, and every value is still typed. The values come from `ExerciseState.LastTime`; none means the exercise's first time, and nothing shows. This sits beside the Last and PB line, which stays.

An exercise just added, strength or cardio, loads its last time through `ILastSessionService.GetAsync`: its sets or entry in the saved workout with the latest start that has it. Nothing shows while it loads, and the values don't change while logging. Picking it again doesn't reload; adding it again after Remove does. When the load fails nothing shows and logging works as usual; Core logs Warning 1510 "Loading the last session of exercise {ExerciseId} failed" with the exception, and no workout contents.
- Sets match by number: set 1 shows last time's set 1, set 2 its set 2. A set number last time didn't have, such as set 4 after 3 sets, shows *—* in the Last column and no line anywhere else. Last time's extra sets show nowhere.
- Lift, assisted and bodyweight rows: a muted **Last** column after Set, as wide as a field, with last time's set as the Exercise page writes it: *60×8* (weight × reps), *1:30*, *20kg · 1:30*. Read out as "Last time 60×8".
- Holds (duration with an optional weight): no Last column, so the duration boxes keep their width. A muted full-width line under each row, e.g. *Last time 1:30*, or *Last time 20kg · 1:30* when a weight was logged. No line for a set number last time didn't have.
- Carries (set panel): a muted *Last time 32kg · 40m · 0:45* line at the top of the open panel; closed sets show none, and no line for a set number last time didn't have. A weight of 0 isn't shown in holds or carries.
- Cardio: a muted line above the fields with the entry's values, e.g. *Last time 5.2km · 30:00 · 5:46/km*; none when last time had no entry.
- A done card is one line, so its Last column and lines are hidden with its sets and entry. The pencil shows the card again, but closed sets show no last time; it shows on a set once its own pencil opens it.

## Save
Save runs `IWorkoutChecker.Check`: every set and entry, then the workout, its note and each exercise and its note through `IWorkoutService.Validate`.
- Errors show on the exercise, set or entry at fault: each faulty field is outlined, with one message per problem under its set or entry. A note over the limit shows its message in its field.
- Focus moves to the first error in screen order: Add exercise for the workout error, otherwise the workout note when it has an error, otherwise the first exercise with an error, which takes it to its Add set or Remove, the first field at fault, or its note.
- Once a set or entry fails a check, every change re-checks it until it passes Add set or opening another set, or the whole workout passes Save. Adding a set clears "Add at least one set"; adding an exercise clears the workout error. Cardio entries are first checked on Save.

A valid Save calls `IWorkoutService.SaveNewAsync` with the workout note and `ISetChecker.Read`'s records, oldest first, each with its note, then raises `OnSaved`; the App goes to the Dashboard. If nothing was saved, the workout error shows "Couldn't save the workout. Try again", the workout and its notes stay and focus moves to Save. Taps on Save while a save runs are ignored.

## Haptics
With the phone on a bench, a short buzz confirms the moments that matter (see [Haptics](./?path=/custom/foundations-haptics)):
- Add set that closes a valid open set: a light tick (`SetLogged`).
- The tick marking an exercise done (`ExerciseDone`), and Save saving the workout, once (`WorkoutSaved`).
- Add set, the tick or Save blocked by errors: the reject pattern (`Rejected`).

Nothing else buzzes: not typing, opening another set, adding or deleting an exercise or set, Undo, a failed write or Discard. Nothing buzzes when the phone's touch feedback is off.

## Back
Back (arrow or Android) opens `ConfirmDialog` "Discard this workout?", even with no exercises. Cancel, or Android back, closes it and the workout stays. Discard logs Information 2701 "New workout discarded with {ExerciseCount} exercises" and raises `OnDiscarded`; the App goes to the Dashboard. Back is ignored while a save runs, after one succeeds and after a discard. Discard runs once; while a save runs or after one succeeds it closes the prompt and does nothing else.

With the soft keyboard open, Android back only hides the keyboard.

States: [Empty](./?path=/story/screens-new-workout--empty) · [Strength](./?path=/story/screens-new-workout--strength) · [Done](./?path=/story/screens-new-workout--done) · [Carry](./?path=/story/screens-new-workout--carry) · [Cardio](./?path=/story/screens-new-workout--cardio) · [Records](./?path=/story/screens-new-workout--records) · [Last time](./?path=/story/screens-new-workout--last-time) · [Last time done](./?path=/story/screens-new-workout--last-time-done) · [Notes](./?path=/story/screens-new-workout--notes) · [Note over the limit](./?path=/story/screens-new-workout--note-over-the-limit) · [Exercises full](./?path=/story/screens-new-workout--exercises-full) · [Save errors](./?path=/story/screens-new-workout--save-errors) · [No exercises error](./?path=/story/screens-new-workout--no-exercises-error) · [Save failed](./?path=/story/screens-new-workout--save-failed) · [Set deleted](./?path=/story/screens-new-workout--set-deleted) · [Exercise deleted](./?path=/story/screens-new-workout--exercise-deleted) · [Discard prompt](./?path=/story/screens-new-workout--discard-prompt)

`data-testid`: `back`, `screen-title`, `screen-subtitle`, `workout-error`, `workout-note` (and `workout-note-add`, `workout-note-input`, `workout-note-count`, `workout-note-open`, `workout-note-error`), `no-exercises`, `exercises-full`, `exercise-{n}` (and its parts, see [Workout](./?path=/custom/components-workout-overview)), `add-exercise`, `save-workout`, `snackbar` (and `snackbar-message`, `snackbar-undo`), `confirm-dialog`.
