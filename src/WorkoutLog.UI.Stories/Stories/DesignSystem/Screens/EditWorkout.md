---
$attribute: CustomPage("Design System/Screens/Edit Workout")
---

# Edit Workout

Fix or delete a saved workout (FR-21–FR-24). View: `WorkoutView` with `Mode` Edit, after `WorkoutLoadView` while it loads; page: `EditWorkoutPage`.

Same layout as [New Workout](/?path=/custom/design-system-screens-new-workout), except:

| Part | Content |
|---|---|
| Top bar | "Total h:mm:ss" instead of the live timer; Delete (text button) on the right. Delete does nothing until US-19. |
| Content | Every carry set starts closed. |
| Overlays | `ConfirmDialog` "Discard changes?" on back with changes (US-18); "Delete this workout?" on Delete (US-19). |
| Loading | `WorkoutLoadView`: title "Workout", `Spinner` "Loading…" centred (D185). |
| Load failed | Error "Couldn't load the workout." and a Secondary **Try again** that reloads. Focus moves to Try again (D185). |
| Not found | "This workout no longer exists.", no retry (D185). |

Back: while loading, after a failed load and when not found, History. On a loaded workout back does nothing until US-18 (D184); then with changes, Discard / Cancel; without, History.

States: [Saved](/?path=/story/screens-edit-workout--saved) · [Carry sets closed](/?path=/story/screens-edit-workout--carry-sets-closed) · [Cardio entries](/?path=/story/screens-edit-workout--cardio-entries) · [Save errors](/?path=/story/screens-edit-workout--save-errors) · [Save failed](/?path=/story/screens-edit-workout--save-failed) · [Discard prompt](/?path=/story/screens-edit-workout--discard-prompt) · [Delete prompt](/?path=/story/screens-edit-workout--delete-prompt) · [Loading](/?path=/story/screens-edit-workout--loading) · [Load failed](/?path=/story/screens-edit-workout--load-failed) · [Not found](/?path=/story/screens-edit-workout--not-found) · [Live page](/?path=/story/screens-edit-workout--live-page)

`data-testid`: as New Workout, plus `delete-workout`; while loading `back`, `workout-loading`, `workout-load-error`, `workout-retry`, `workout-not-found`.
