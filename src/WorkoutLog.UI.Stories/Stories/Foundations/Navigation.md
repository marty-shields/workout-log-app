---
$attribute: CustomPage("Foundations/Navigation")
---

# Navigation

The app opens on [Startup](./?path=/custom/screens-startup-overview), then [This week](./?path=/custom/screens-this-week-overview). Add Exercise is an overlay on the workout screen, not a route.

This week, Workouts and Exercises are the top-level screens: each shows the navigation bar ([`BottomNav`](./?path=/custom/components-layout-overview), its first item still labelled Dashboard) and has no back arrow. Android back from Workouts or Exercises goes to This week, and from This week leaves the app. The three-slot bar of This week, New and Trends replaces the navigation bar later. [Trends](./?path=/custom/screens-trends-overview) is reached by its route, `/trends`, until the bar gives it a tab.

| From | Action | To |
|---|---|---|
| This week | New Workout | New Workout |
| This week | Workouts in the navigation bar | Workouts |
| This week | Exercises in the navigation bar | Exercises |
| This week | Tap a record | Exercise |
| This week | Settings gear | Settings |
| This week | Tap a workout | Edit Workout |
| This week | Last week row | Nothing yet |
| This week | Back | Leaves the app |
| Trends | Previous or next week | Trends, that week |
| Trends | Tap a record | Exercise |
| Trends | Settings gear | Settings |
| Trends | New in the tab bar | New Workout |
| Trends | This week in the tab bar | This week |
| Trends | Back | Leaves the app |
| Workouts | Tap a row | Edit Workout |
| Workouts | Back, or Dashboard in the navigation bar | This week |
| Workouts | Exercises in the navigation bar | Exercises |
| Exercises | Tap a row | Exercise |
| Exercises | Back, or Dashboard in the navigation bar | This week |
| Exercises | Workouts in the navigation bar | Workouts |
| Exercise | Back | Exercises, This week when opened from it, or the Trends week when opened from there |
| Settings | Back | This week |
| Settings | Import, then pick a file | Import Review |
| Import Review | Import, or Replace all then Replace | Settings |
| Import Review | Cancel, or back | Settings |
| New Workout | Add exercise | Add Exercise |
| New Workout | Save | Workout Summary |
| New Workout | Back, then Discard | This week |
| Workout Summary | Done, or back | This week |
| Edit Workout | Add exercise | Add Exercise |
| Edit Workout | Save | This week |
| Edit Workout | Back with no changes | Workouts |
| Edit Workout | Back with changes, then Discard | Workouts |
| Edit Workout, opened from This week | Back with no changes, or with changes then Discard; or Delete, then Delete | This week |
| Edit Workout | Delete, then Delete | Workouts |
| Add Exercise | Pick an exercise, or back | The workout it opened from |
| Error | Restart | This week |

## Back
- The back arrow and Android back do the same, except with the soft keyboard open: Android back then only hides the keyboard, and the next back acts as below. The back arrow acts at once.
- An open `ConfirmDialog` closes on Android back, as Cancel does.
- A `Snackbar` is not a dialog: Android back does what it does on the screen and leaves the snackbar to time out.
- What back does on each screen is on its page: [New Workout](./?path=/custom/screens-new-workout-overview), [Edit Workout](./?path=/custom/screens-edit-workout-overview), [Add Exercise](./?path=/custom/screens-add-exercise-overview), [Workouts](./?path=/custom/screens-workouts-overview), [Exercises](./?path=/custom/screens-exercises-overview), [Exercise](./?path=/custom/screens-exercise-overview), [Settings](./?path=/custom/screens-settings-overview), [Import Review](./?path=/custom/screens-import-review-overview). Workout Summary has no back arrow: Android back does what Done does, [Workout Summary](./?path=/custom/screens-workout-summary-overview).
