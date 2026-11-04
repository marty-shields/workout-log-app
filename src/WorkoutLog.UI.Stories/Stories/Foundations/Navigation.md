---
$attribute: CustomPage("Foundations/Navigation")
---

# Navigation

The app opens on [Startup](./?path=/custom/screens-startup-overview), then the Dashboard. Add Exercise is an overlay on the workout screen, not a route.

Dashboard, Workouts and Exercises are the top-level screens: each shows the navigation bar ([`BottomNav`](./?path=/custom/components-layout-overview)) and has no back arrow. Android back from Workouts or Exercises goes to the Dashboard, and from the Dashboard leaves the app.

| From | Action | To |
|---|---|---|
| Dashboard | New Workout | New Workout |
| Dashboard | Workouts in the navigation bar | Workouts |
| Dashboard | Exercises in the navigation bar | Exercises |
| Dashboard | Tap a recent record | Exercise |
| Dashboard | Settings gear | Settings |
| Dashboard | Last workout | Edit Workout |
| Dashboard | Sets per muscle group | Sets per muscle group |
| Dashboard | Back | Leaves the app |
| Sets per muscle group | Back | Dashboard |
| Workouts | Tap a row | Edit Workout |
| Workouts | Back, or Dashboard in the navigation bar | Dashboard |
| Workouts | Exercises in the navigation bar | Exercises |
| Exercises | Tap a row | Exercise |
| Exercises | Back, or Dashboard in the navigation bar | Dashboard |
| Exercises | Workouts in the navigation bar | Workouts |
| Exercise | Back | Exercises, or the Dashboard when opened from it |
| Settings | Back | Dashboard |
| Settings | Import, then pick a file | Import Review |
| Import Review | Import, or Replace all then Replace | Settings |
| Import Review | Cancel, or back | Settings |
| New Workout | Add exercise | Add Exercise |
| New Workout | Save | Workout Summary |
| New Workout | Back, then Discard | Dashboard |
| Workout Summary | Done, or back | Dashboard |
| Edit Workout | Add exercise | Add Exercise |
| Edit Workout | Save | Dashboard |
| Edit Workout | Back with no changes | Workouts |
| Edit Workout | Back with changes, then Discard | Workouts |
| Edit Workout, opened from the Dashboard | Back with no changes, or with changes then Discard; or Delete, then Delete | Dashboard |
| Edit Workout | Delete, then Delete | Workouts |
| Add Exercise | Pick an exercise, or back | The workout it opened from |
| Error | Restart | Dashboard |

## Back
- The back arrow and Android back do the same, except with the soft keyboard open: Android back then only hides the keyboard, and the next back acts as below. The back arrow acts at once.
- An open `ConfirmDialog` closes on Android back, as Cancel does.
- What back does on each screen is on its page: [New Workout](./?path=/custom/screens-new-workout-overview), [Edit Workout](./?path=/custom/screens-edit-workout-overview), [Add Exercise](./?path=/custom/screens-add-exercise-overview), [Workouts](./?path=/custom/screens-workouts-overview), [Exercises](./?path=/custom/screens-exercises-overview), [Exercise](./?path=/custom/screens-exercise-overview), [Sets per muscle group](./?path=/custom/screens-sets-per-muscle-group-overview), [Settings](./?path=/custom/screens-settings-overview), [Import Review](./?path=/custom/screens-import-review-overview). Workout Summary has no back arrow: Android back does what Done does, [Workout Summary](./?path=/custom/screens-workout-summary-overview).
