---
$attribute: CustomPage("Foundations/Navigation")
---

# Navigation

The app opens on [Startup](./?path=/custom/screens-startup-overview), then the Dashboard. The Exercise Picker is an overlay on the workout screen, not a route.

| From | Action | To |
|---|---|---|
| Dashboard | New Workout | New Workout |
| Dashboard | History | History |
| Dashboard | Back | Leaves the app |
| History | Tap a row | Edit Workout |
| History | Back | Dashboard |
| New Workout | Add exercise | Exercise Picker |
| New Workout | Save | Dashboard |
| New Workout | Back, then Discard | Dashboard |
| Edit Workout | Add exercise | Exercise Picker |
| Edit Workout | Save | Dashboard |
| Edit Workout | Back with no changes | History |
| Edit Workout | Back with changes, then Discard | History |
| Edit Workout | Delete, then Delete | History |
| Exercise Picker | Pick an exercise, or back | The workout it opened from |
| Error | Restart | Dashboard |

## Back
- The back arrow and Android back do the same, except with the soft keyboard open: Android back then only hides the keyboard, and the next back acts as below. The back arrow acts at once (D206).
- An open `ConfirmDialog` closes on Android back, as Cancel does (D144).
- What back does on each screen is on its page: [New Workout](./?path=/custom/screens-new-workout-overview), [Edit Workout](./?path=/custom/screens-edit-workout-overview), [Exercise Picker](./?path=/custom/screens-exercise-picker-overview), [History](./?path=/custom/screens-history-overview).
