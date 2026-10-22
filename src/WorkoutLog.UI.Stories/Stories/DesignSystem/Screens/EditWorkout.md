---
$attribute: CustomPage("Design System/Screens/Edit Workout")
---

# Edit Workout

Fix or delete a saved workout (FR-21–FR-24). View: `WorkoutView` with `Mode` Edit.

Same layout as [New Workout](/?path=/custom/design-system-screens-new-workout), except:

| Part | Content |
|---|---|
| Top bar | "Total h:mm:ss" instead of the live timer; Delete (text button) on the right. |
| Content | Every carry set starts closed. |
| Overlays | `ConfirmDialog` "Discard changes?" on back with changes; "Delete this workout?" on Delete. |

Back: with changes, Discard / Cancel; without, History.

States: [Saved](/?path=/story/screens-edit-workout--saved) · [Carry sets closed](/?path=/story/screens-edit-workout--carry-sets-closed) · [Save errors](/?path=/story/screens-edit-workout--save-errors) · [Discard prompt](/?path=/story/screens-edit-workout--discard-prompt) · [Delete prompt](/?path=/story/screens-edit-workout--delete-prompt)

`data-testid`: as New Workout, plus `delete-workout`.
