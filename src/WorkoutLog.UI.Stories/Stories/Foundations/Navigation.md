---
$attribute: CustomPage("Foundations/Navigation")
---

# Navigation

The app opens on [Startup](./?path=/custom/screens-startup-overview), then the Dashboard. Add Exercise is an overlay on the workout screen, not a route.

| From | Action | To |
|---|---|---|
| Dashboard | New Workout | New Workout |
| Dashboard | Workouts | Workouts |
| Dashboard | Exercises | Exercises |
| Dashboard | Settings gear | Settings |
| Dashboard | Back | Leaves the app |
| Workouts | Tap a row | Edit Workout |
| Workouts | Back | Dashboard |
| Exercises | Tap a lift or bodyweight row | Exercise |
| Exercises | Back | Dashboard |
| Exercise | Back | Exercises |
| Settings | Back | Dashboard |
| Settings | Import, then pick a file | Import Review |
| Import Review | Import, or Replace all then Replace | Settings |
| Import Review | Cancel, or back | Settings |
| New Workout | Add exercise | Add Exercise |
| New Workout | Save | Dashboard |
| New Workout | Back, then Discard | Dashboard |
| Edit Workout | Add exercise | Add Exercise |
| Edit Workout | Save | Dashboard |
| Edit Workout | Back with no changes | Workouts |
| Edit Workout | Back with changes, then Discard | Workouts |
| Edit Workout | Delete, then Delete | Workouts |
| Add Exercise | Pick an exercise, or back | The workout it opened from |
| Error | Restart | Dashboard |

## Back
- The back arrow and Android back do the same, except with the soft keyboard open: Android back then only hides the keyboard, and the next back acts as below. The back arrow acts at once (D78).
- An open `ConfirmDialog` closes on Android back, as Cancel does (D78).
- What back does on each screen is on its page: [New Workout](./?path=/custom/screens-new-workout-overview), [Edit Workout](./?path=/custom/screens-edit-workout-overview), [Add Exercise](./?path=/custom/screens-add-exercise-overview), [Workouts](./?path=/custom/screens-workouts-overview), [Exercises](./?path=/custom/screens-exercises-overview), [Exercise](./?path=/custom/screens-exercise-overview), [Settings](./?path=/custom/screens-settings-overview), [Import Review](./?path=/custom/screens-import-review-overview).
