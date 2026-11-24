---
$attribute: CustomPage(StoryTitles.Prefix + "/Screens/This week/Overview")
---

# This week

Home screen: how the week is going. Which days you trained, how it compares with last week, what you did, which records fell and which muscles you worked; start a workout with + and switch to Trends from the bottom nav, or open Settings. View: `ThisWeekView`; page: `ThisWeekPage`. Design: [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html).

| Part | Content |
|---|---|
| Top bar | Title "Workout Log"; Settings gear (`IconButton`) on the right in every state. No back arrow; back leaves the app. |
| Week line | The week's dates (e.g. *16 – 22 Nov*) and the streak at the right (e.g. *23-week streak*, hidden at 0). |
| Day strip | The kit `DayStrip` (Charts) for Monday to Sunday: a tick on each day with a workout, a ring on today, the days to come dimmed. Read out as e.g. *Monday 16 November, trained*. Day test ids are `day-strip-{yyyy-MM-dd}`. |
| Tiles | `WeekTiles`: Workouts, Time and Sets against Monday to today's weekday last week. |
| This week's workouts | A `WeekWorkoutsCard`. |
| Records this week | A `WeekRecordsCard`. |
| Sets per muscle group | A `MuscleSetsCard`: Monday to today. |
| Last week | A `LastWeekCard`, only while nothing is logged this week and last week is known. |
| Records failed | The same week with its Records card left out, because the records could not be read. |
| Loading | `Spinner` "Loading…" instead of the week. The top bar and bottom nav work. |
| Load failed | Error "Couldn't load This week" and the kit's `LoadFailed` **Try again** button instead of the week. Focus moves to Try again. The top bar and bottom nav work. |
| Bottom navigation | The kit's `BottomNav` with This week current, through `AppScreen`'s `NavItems` (from `MainTabs`): Trends opens [Trends](./?path=/custom/workout-log-screens-trends-overview), + opens [New Workout](./?path=/custom/workout-log-screens-new-workout-overview) (`/workout/new?from=this-week`, so discarding it returns here). |

The parts run top to bottom in that order. With no workouts this week the Records and muscle group cards are left out: the workouts card says "Nothing yet this week. Tap + to start one.", the tiles show zeros against last week, and a Last week row shows last week's workouts, time and sets. The row shows only when last week had a workout, so before the first workout ever there is none. Tapping it opens last week in [Trends](./?path=/custom/workout-log-screens-trends-overview) (`OnOpenLastWeek` raises that week).

`ThisWeekView` takes every value as a parameter. `ThisWeekPage` fills them from `IPeriodTotalsService` and `IPeriodTrainingService`, reading together: the week and last week (`GetComparisonAsync`), the days with a workout (`GetSeriesAsync`), the streak (`GetStreaksAsync`), the week's workouts, the sets per muscle group and the records (`GetWorkoutsAsync`, `GetMuscleSetsAsync`, `GetRecordsAsync`, all for Monday to Sunday, up to now). Weeks run from Monday 00:00 to Sunday 23:59:59 in the phone's time zone, by start time, and today is taken afresh on each load. The tiles compare this week so far with last week up to the end of the same weekday, so on a Wednesday morning it is compared with all of last Monday to Wednesday. A workout shows its start on the phone's clock, its first two exercises and `+n` for the rest, and how long it took. The streak counts consecutive weeks with a workout ending last week, plus this week once it has one.

A record is a session whose best beats the exercise's best in every earlier session, the best being the one the Add Exercise list shows: lift heaviest, tie → most reps; bodyweight heaviest, unweighted being 0 kg, tie → most reps; assisted least assist, tie → most reps; hold longest; carry heaviest, tie → farthest; cardio distance, else duration. A tie isn't a record, and an exercise's first session isn't one. The card lists the week's records newest first, one row per exercise (its newest record that week), up to five and then "n more". If the records fail to load, their card is left out and the rest still shows; if any other read fails, This week shows Load failed.

A workout opens [Edit Workout](./?path=/custom/workout-log-screens-edit-workout-overview) (`/workout/{id}?from=this-week`) and a record its [Exercise](./?path=/custom/workout-log-screens-exercise-overview) page (`/exercises/{id}?from=this-week`); back, or on a workout Save or Delete, returns to This week. Android back leaves the app.

Loads each time it shows, and again when the app comes back on screen. Pull down to refresh (the kit's `AppScreen`, whose stories are under Components on the [combined stories site](https://marty-shields.github.io/app-ui-kit/)) loads it again the same way. Each load shows the spinner in place of the week, as on opening, then what was read, or the load error. After a pull the spinner stays up for at least half a second, so a fast reload doesn't blink. A failed load is logged without any workout contents.

States: [Mid-week](./?path=/story/workout-log-screens-this-week--mid-week) · [New week](./?path=/story/workout-log-screens-this-week--new-week) · [No workouts](./?path=/story/workout-log-screens-this-week--no-workouts) · [Records failed](./?path=/story/workout-log-screens-this-week--records-failed) · [Loading](./?path=/story/workout-log-screens-this-week--loading) · [Load failed](./?path=/story/workout-log-screens-this-week--load-failed)

The parts are described on the [This week components](./?path=/custom/workout-log-components-thisweek-overview) page.

`data-testid`: `this-week-dates`, `this-week-streak`, `day-strip`, `week-tiles`, `week-workouts`, `week-records`, `week-muscles`, `last-week` (with the parts' own as prefixes), `bottom-nav` with `this-week-link`, `add-button` and `trends-link`, `settings`, `this-week-loading`, `this-week-error`, `this-week-retry`.
