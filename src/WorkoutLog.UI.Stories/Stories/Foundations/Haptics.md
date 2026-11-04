---
$attribute: CustomPage("Foundations/Haptics")
---

# Haptics

The workout is logged with the phone on a bench, glanced at between sets. A short buzz says the tap worked, or that it didn't, without reading the screen. Haptics confirm a few meaningful moments and signal a rejection. Ordinary taps don't buzz.

A screen plays a cue through `IHaptics.Play(HapticCue)` where the action succeeded or was blocked, never on the raw tap. The App plays it as Android's semantic touch feedback on the current window (`View.PerformHapticFeedback`), so the phone's **Touch feedback** setting turns it off, and it needs no `VIBRATE` permission. A failure is ignored: haptics are never worth an error. The stories site and the tests play nothing.

| Cue | When | Android feedback |
|---|---|---|
| `SetLogged` | Add set closed the open set as valid and opened a new one. Not when no set was open, and not when picking an exercise again adds a set. | `CONTEXT_CLICK`, a light tick: it is the most frequent cue. |
| `ExerciseDone` | The tick marked an exercise done. | `CONFIRM` |
| `WorkoutSaved` | Save saved the workout, New or Edit. Once, however often Save is tapped. | `CONFIRM` |
| `Rejected` | Add set, the tick or Save was blocked by errors. Not when a save fails to write: its message says why. | `REJECT`, a distinct, stronger pattern. |

No cue for navigation, opening another set (even when it is blocked), typing, adding an exercise, deleting, Undo, Discard or Delete. The snackbar and the screen change already confirm those.

The cues are listed on [New Workout](./?path=/custom/screens-new-workout-overview#haptics) and [Edit Workout](./?path=/custom/screens-edit-workout-overview).
