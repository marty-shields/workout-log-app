---
$attribute: CustomPage("Screens/This week/Overview")
---

# This week

How the week is going: which days you trained, how it compares with last week, what you did, which records fell and which muscles you worked. It will replace the Dashboard as the home screen. Not wired up yet: there is no page, route or service, and the app still opens on the [Dashboard](./?path=/custom/screens-dashboard-overview). View: `ThisWeekView`. Design: [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html).

| Part | Content |
|---|---|
| Top bar | Title "Workout Log"; Settings gear (`IconButton`) on the right in every state. No back arrow; back leaves the app. |
| Week line | The week's dates (e.g. *16 – 22 Nov*) and the streak at the right (e.g. *23-week streak*, hidden at 0). |
| Day strip | A `DayStrip` for Monday to Sunday. |
| Tiles | `WeekTiles`: Workouts, Time and Sets against Monday to today's weekday last week. |
| This week's workouts | A `WeekWorkoutsCard`. |
| Records this week | A `WeekRecordsCard`. |
| Sets per muscle group | A `MuscleSetsCard`: Monday to today. |
| Last week | A `LastWeekCard`, only while nothing is logged this week and last week is known. |
| Loading | `Spinner` "Loading…" instead of the week. The top bar and tab bar work. |
| Load failed | Error "Couldn't load This week" and a Secondary full-width **Try again** instead of the week. Focus moves to Try again. The top bar and tab bar work. |
| Tab bar | `MainTabBar` with This week current, shown through `AppScreen`'s `Navigation` slot. |

The parts run top to bottom in that order. With no workouts this week the Records and muscle group cards are left out: the workouts card says "Nothing yet this week. Tap + to start one.", the tiles show zeros against last week, and a Last week row shows last week's workouts, time and sets. Before the first workout ever there is no Last week row. `ThisWeekView` takes every value as a parameter; the page that fills them from services comes later.

States: [Mid-week](./?path=/story/screens-this-week--mid-week) · [New week](./?path=/story/screens-this-week--new-week) · [No workouts](./?path=/story/screens-this-week--no-workouts) · [Loading](./?path=/story/screens-this-week--loading) · [Load failed](./?path=/story/screens-this-week--load-failed)

The parts are described on the [This week components](./?path=/custom/components-thisweek-overview) page.

`data-testid`: `this-week-dates`, `this-week-streak`, `day-strip`, `week-tiles`, `week-workouts`, `week-records`, `week-muscles`, `last-week` (with the parts' own as prefixes), `settings`, `tab-bar`, `this-week-link`, `new-link`, `trends-link`, `this-week-loading`, `this-week-error`, `this-week-retry`.
