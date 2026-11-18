---
$attribute: CustomPage("Screens/Trends/Overview")
---

# Trends

How a week, a month or a year went: what you did, how it compares with the period before, and how the weeks and months added up. It will be the second tab of the bottom bar; until then it opens by its route, `/trends`, and nothing in the app links to it. Only Week works so far: Month and Year come later. View: `TrendsView`; page: `TrendsPage`. Design: [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html).

| Part | Content |
|---|---|
| Top bar | Title "Trends"; Settings gear (`IconButton`) on the right in every state. No back arrow. |
| Kind | A Week / Month / Year `SegmentedControl`. The page offers only Week for now, so it leaves the control out (`TrendsView`'s `Kinds` lists what is offered; one kind hides it). |
| Stepper | A `PeriodStepper`: previous arrow, the period's label, next arrow. Previous is disabled at the period of the first workout, next at the period that holds today. |
| Tiles | `TrendTiles`: Workouts, Time, Sets and Volume (strength) against the period before, up to the same point in it while the period is still running. |
| Tab bar | `MainTabBar` with Trends current, through `AppScreen`'s `Navigation` slot. |

Under the tiles, each kind has its own cards, top to bottom:

| Kind | Cards |
|---|---|
| Week | **Time trained by day**: `TrendBars` paired, this week beside the same day last week, with a legend. **Sets per muscle group**: `MuscleSetsCard`. **Records**: `WeekRecordsCard`, "n this week". **Last 12 weeks**: `WeekBars` with the streak at the right (e.g. *23-week streak*, hidden at 0). |
| Month | **Each day**: `MonthCalendar`. **Weeks**: `MonthWeeks`. **Records**: `WeekRecordsCard`, "n this month". **Most trained**: `MeterRows` of the top five exercises by sessions. |
| Year | **Workouts by month**: single `TrendBars`, "3.2 a week" at the right (hidden at 0). **Longest streak**: `LongestStreakCard`. **Records by month**: `MeterRows`, "n this year". |

Every chart has an accessible name saying what it shows and a hidden list of its values; none tells a state by colour alone (see [Trends components](./?path=/custom/components-trends-overview)).

## States

- **Week, Month, Year**: the current period part-way through, as it would look today (Wed 18 Nov 2026). The next arrow is disabled; the day bars after today are empty and today's day is bold; the calendar rings today and dims the days to come.
- **Week only**: the current week as the page shows it today, with only Week offered, so the kind control is hidden.
- **Past week, Past month, Past year**: a finished period. Both arrows work, the cards say "that week", "that month" or "that year", and the paired bars are labelled *That week* and *The week before*.
- **No workouts**: a period with none. The tiles show zeros and "—", each chart says what is missing instead (e.g. *No workouts that week*), the records cards say "No new records that week" and the muscle groups all read "Not trained". The previous arrow is disabled when it is the first period.
- **Loading**: `Spinner` "Loading…" in place of the cards. The kind control, stepper and tab bar work.
- **Load failed**: error "Couldn't load Trends" and a Secondary full-width **Try again** in place of the cards. Focus moves to Try again. The kind control, stepper and tab bar work.

`TrendsView` takes every value as a parameter, with those of the kinds not shown ignored. A change of kind or period is raised (`OnKindChanged`, `OnPrevious`, `OnNext`) and the page answers by loading the period and passing it back.

## Trends page

`TrendsPage` fills the Week view from `IPeriodTotalsService` and `IPeriodTrainingService`, reading together: the week and the week before (`GetComparisonAsync`), the week's volume (`GetVolumeAsync`), the days (`GetSeriesAsync`), the streak and the last 12 weeks (`GetStreaksAsync`), the stepper's limits (`GetLimitsAsync`), the sets per muscle group and the records (`GetMuscleSetsAsync`, `GetRecordsAsync`). Weeks run from Monday 00:00 to Sunday 23:59:59 in the phone's time zone, and today is taken afresh on each load.

- **Route**: `/trends?period=week&date=2026-11-09` opens the week holding that date, so a link or back can open a given week. A missing or invalid `period` or `date`, or a date after this week, shows this week. Any `period` other than `week` shows the week until Month and Year arrive.
- **Stepping**: the previous and next arrows show the week before or after, put its Monday in the URL as `date`, and load it. Previous is disabled on the week of the first workout (the same as next with no workouts at all) and next on this week. A step made while a load runs drops that load's result.
- **Compared with**: this week so far is compared with last week up to the same weekday (*Against Monday to Wednesday last week*); a finished week with the week before it.
- **Last 12 weeks** always ends at the current week, whichever week is shown.
- **Load failed**: if the week, the week before, the volume, the days, the streak, the limits or the muscle groups fail to read, the page shows the error and Try again. If only the records fail, their card reads "No new records" and the rest shows; the failure is logged either way, with no workout contents.
- **Reloads** when the app comes back on screen and on pull to refresh (the spinner shows for at least half a second). A reload that finds a new week has begun while this week was shown moves to the new week.
- **Records** open their exercise; back from the exercise returns to the same week (the link carries `from=trends:2026-11-09`). Muscle group rows do nothing.
- **Back**: Android back has no handler, so it leaves the app, as on This week.

States: [Week](./?path=/story/screens-trends--week) · [Week only](./?path=/story/screens-trends--week-only) · [Month](./?path=/story/screens-trends--month) · [Year](./?path=/story/screens-trends--year) · [Past week](./?path=/story/screens-trends--past-week) · [Past month](./?path=/story/screens-trends--past-month) · [Past year](./?path=/story/screens-trends--past-year) · [No workouts, week](./?path=/story/screens-trends--no-workouts-week) · [No workouts, month](./?path=/story/screens-trends--no-workouts-month) · [No workouts, year](./?path=/story/screens-trends--no-workouts-year) · [Loading](./?path=/story/screens-trends--loading) · [Load failed](./?path=/story/screens-trends--load-failed)

`data-testid`: `trends-kind` (`trends-week`, `trends-month`, `trends-year`), `trends-stepper` (`-previous`, `-label`, `-next`), `trends-tiles`, `trends-days`, `trends-muscles`, `trends-records`, `trends-recent-weeks`, `trends-calendar`, `trends-weeks`, `trends-most-trained`, `trends-months`, `trends-streak`, `trends-records-by-month` (with the parts' own as prefixes), `settings`, `tab-bar`, `this-week-link`, `new-link`, `trends-link`, `trends-loading`, `trends-error`, `trends-retry`.
