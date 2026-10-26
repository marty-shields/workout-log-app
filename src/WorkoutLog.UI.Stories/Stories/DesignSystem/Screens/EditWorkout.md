---
$attribute: CustomPage("Design System/Screens/Edit Workout")
---

# Edit Workout

Fix or delete a saved workout (FR-21–FR-24). View: `WorkoutView` with `Mode` Edit, after `WorkoutLoadView` while it loads; page: `EditWorkoutPage`.

Same layout as [New Workout](/?path=/custom/design-system-screens-new-workout), except:

| Part | Content |
|---|---|
| Top bar | "Total h:mm:ss" instead of the live timer; Delete (text button) on the right. |
| Content | Every carry set starts closed. The saved workout opens newest first. |
| Overlays | `ConfirmDialog` "Discard changes?" on back with changes; "Delete this workout?" on Delete (FR-24). |
| Loading | `WorkoutLoadView`: title "Workout", `Spinner` "Loading…" centred (D185). |
| Load failed | Error "Couldn't load the workout." and a Secondary **Try again** that reloads. Focus moves to Try again (D185). |
| Not found | "This workout no longer exists.", no retry (D185). |

Back: while loading, after a failed load and when not found, History (D185). On a loaded workout with no changes, History. After a change (a value typed, a set added or removed, an exercise added, picked again or removed), back (arrow or Android) opens `ConfirmDialog` "Discard changes?" (FR-23). Cancel, or Android back, closes it and the edits stay. Discard logs Information 2902 "Edits to workout {WorkoutId} discarded" and raises `OnBack`; the App goes to History. Back is ignored while a save runs, after one succeeds and after a discard. Discard runs once; while a save runs or after one succeeds it closes the prompt and does nothing else (D189).

Delete: opens `ConfirmDialog` "Delete this workout?", the same with or without changes. Cancel, or Android back, closes it. Delete removes the workout and raises `OnDeleted`; the App goes to History. If the delete fails, the prompt closes, "Couldn't delete the workout. Try again." shows at the top, the workout and its edits stay, and focus moves to Delete. Delete is ignored while a save runs, after one succeeds and after a discard; while a delete runs and after one succeeds, Back, Save and Delete are ignored, the prompt's Cancel and Android back do nothing, and Discard closes its prompt. Confirm runs once; while a save runs or after one succeeds it closes the prompt and does nothing else (D191).

States: [Saved](/?path=/story/screens-edit-workout--saved) · [Newest first](/?path=/story/screens-edit-workout--newest-first) · [Carry sets closed](/?path=/story/screens-edit-workout--carry-sets-closed) · [Cardio entries](/?path=/story/screens-edit-workout--cardio-entries) · [Save errors](/?path=/story/screens-edit-workout--save-errors) · [Save failed](/?path=/story/screens-edit-workout--save-failed) · [Discard prompt](/?path=/story/screens-edit-workout--discard-prompt) · [Delete prompt](/?path=/story/screens-edit-workout--delete-prompt) · [Delete failed](/?path=/story/screens-edit-workout--delete-failed) · [Loading](/?path=/story/screens-edit-workout--loading) · [Load failed](/?path=/story/screens-edit-workout--load-failed) · [Not found](/?path=/story/screens-edit-workout--not-found) · [Live page](/?path=/story/screens-edit-workout--live-page) · [Back, discard and delete](/?path=/story/screens-edit-workout--back-discard-and-delete)

`data-testid`: as New Workout, plus `delete-workout`; while loading `back`, `workout-loading`, `workout-load-error`, `workout-load-retry`, `workout-not-found`.
