---
$attribute: CustomPage("Screens/Dashboard/Overview")
---

# Dashboard

Home screen: what you did last, how often you train, what you've beaten recently; start a workout, open Workouts, Exercises and Settings. View: `DashboardView`; page: `DashboardPage`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log"; Settings gear (`IconButton`) on the right in every state, opens Settings. No back arrow; back leaves the app. |
| Cards | Three slots, top to bottom: `LastWorkout`, `Consistency`, `RecentRecords`, filled with a `LastWorkoutCard`, `ConsistencyCard` and `RecentRecordsCard`. The page fills each slot from its own service; an empty slot shows nothing. |
| Rows | Workouts and Exercises as `ListRow`s; Exercises opens [Exercises](./?path=/custom/screens-exercises-overview). |
| Loading | `Spinner` "Loading…" instead of the cards. Workouts, Exercises and New Workout work. |
| Load failed | Error "Couldn't load the Dashboard" and a Secondary full-width **Try again** that reloads, instead of the cards. Focus moves to Try again. Workouts, Exercises and New Workout work. |
| Bottom bar | New Workout, `AppButton` primary, full width. |

Last workout taps through to the workout, a record to its exercise, and the Sets per muscle group row to [Sets per muscle group](./?path=/custom/screens-sets-per-muscle-group-overview). Each card's content and rules are on the [Dashboard components](./?path=/custom/components-dashboard-overview) page.

`DashboardPage` fills `LastWorkout` from `ILastWorkoutService`: the workout with the latest start, its days since in calendar days in the phone's time zone (a start after today shows Today), finish − start as the duration, and its exercises in the workout's order. Back, or Delete, on the [Edit Workout](./?path=/custom/screens-edit-workout-overview) it opens (`/workout/{id}?from=dashboard`) returns to the Dashboard. If the last workout, the counts or the Consistency card fail to load, the Dashboard shows Load failed.

`DashboardPage` fills `Consistency` with a `ConsistencyCard`. Weeks run from Monday 00:00 to Sunday 23:59:59 in the phone's time zone, by start time. This week counts from Monday 00:00 to now, last week from last Monday 00:00 to the same day and time last week. The 12 bars are this week and the 11 before it. The streak counts consecutive weeks with a workout ending last week, as far back as they go, plus this week once it has one.

Recent records: `DashboardPage` fills `RecentRecords` from `IRecentRecordsService`, loaded with the rest of the Dashboard. A record is a session whose best beats the exercise's best in every earlier session, the best being the one the Exercises list shows: lift heaviest, tie → most reps; bodyweight heaviest, unweighted being 0 kg, tie → most reps; assisted least assist, tie → most reps; hold longest; carry heaviest, tie → farthest; cardio distance, else duration. A tie isn't a record, and an exercise's first session isn't one. The card shows records from sessions started today or in the 29 days before, in the phone's time zone and not after now: the latest per exercise, newest first, exercises from the same workout A–Z, up to 3. Tapping one opens its [Exercise](./?path=/custom/screens-exercise-overview) page, and back returns to the Dashboard. If the records fail to load, the slot is empty.

Loads each time it shows, and again when the app comes back on screen.

States: [With workouts](./?path=/story/screens-dashboard--with-workouts) · [First workout](./?path=/story/screens-dashboard--first-workout) · [No workouts](./?path=/story/screens-dashboard--no-workouts) · [Loading](./?path=/story/screens-dashboard--loading) · [Load failed](./?path=/story/screens-dashboard--load-failed)

`data-testid`: `last-workout`, `consistency`, `recent-records` (with the cards' own as prefixes), `workouts-link`, `exercises-link`, `new-workout`, `dashboard-loading`, `dashboard-error`, `dashboard-retry`, `settings`. `DashboardPage` today: `recent-records`.
