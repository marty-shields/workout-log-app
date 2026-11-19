---
$attribute: CustomPage("Screens/Trends/Overview")
---

# Trends

How a week, a month or a year went: what you did, how it compares with the period before, and how the weeks and months added up. It is the second tab of the tab bar, at `/trends`. View: `TrendsView`; page: `TrendsPage`. Design: [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html).

| Part | Content |
|---|---|
| Top bar | Title "Trends"; Settings gear (`IconButton`) on the right in every state. No back arrow. |
| Kind | A Week / Month / Year `SegmentedControl`. The page offers all three (`TrendsView`'s `Kinds` lists what is offered; one kind hides the control). |
| Stepper | A `PeriodStepper`: previous arrow, the period's label, next arrow. Previous is disabled at the period of the first workout, next at the period that holds today. |
| Tiles | `TrendTiles`: Workouts, Time, Sets and Volume (strength) against the period before, up to the same point in it while the period is still running. |
| Tab bar | `MainTabBar` with Trends current, through `AppScreen`'s `Tab`: This week opens [This week](./?path=/custom/screens-this-week-overview), + opens [New Workout](./?path=/custom/screens-new-workout-overview) with `from` the period shown, so discarding returns to it. |

Under the tiles, each kind has its own cards, top to bottom:

| Kind | Cards |
|---|---|
| Week | **Time trained by day**: `TrendBars` paired, this week beside the same day last week, with a legend. **Sets per muscle group**: `MuscleSetsCard`. **Records**: `WeekRecordsCard`, "n this week". **Last 12 weeks**: `WeekBars` with the streak at the right (e.g. *23-week streak*, hidden at 0). |
| Month | **Each day**: `MonthCalendar`. **Weeks**: `MonthWeeks`. **Records**: `RecordRows`, "n this month", each dated (e.g. *Wed 18 Nov*). **Most trained**: `MeterRows` of the top five exercises by sessions. |
| Year | **Workouts by month**: single `TrendBars`, "3.2 a week" at the right (hidden at 0). **Longest streak**: `LongestStreakCard`. **Records by month**: `MeterRows`, "n this year". |

Every chart has an accessible name saying what it shows and a hidden list of its values; none tells a state by colour alone (see [Trends components](./?path=/custom/components-trends-overview)).

## States

- **Week, Month, Year**: the current period part-way through, as it would look today (Wed 18 Nov 2026). The next arrow is disabled; the day bars after today are empty and today's day is bold; the calendar rings today and dims the days to come.
- **Week only**: the current week as the page shows it today, with only Week offered, so the kind control is hidden.
- **Past week, Past month, Past year**: a finished period. Both arrows work, the cards say "that week", "that month" or "that year", and the paired bars are labelled *That week* and *The week before*.
- **No workouts**: a period with none. The tiles show zeros and "—", each chart says what is missing instead (e.g. *No workouts that week*), the records cards say "No new records that week" and the muscle groups all read "Not trained". The previous arrow is disabled when it is the first period.
- **No workouts ever**: the current week with nothing logged yet, so both arrows are disabled and the cards read as an empty week.
- **Loading**: `Spinner` "Loading…" in place of the cards. The kind control, stepper and tab bar work.
- **Load failed**: error "Couldn't load Trends" and a Secondary full-width **Try again** in place of the cards. Focus moves to Try again. The kind control, stepper and tab bar work.

`TrendsView` takes every value as a parameter, with those of the kinds not shown ignored. A change of kind or period is raised (`OnKindChanged`, `OnPrevious`, `OnNext`) and the page answers by loading the period and passing it back.

## Trends page

`TrendsPage` fills the Week, Month and Year views from `IPeriodTotalsService` and `IPeriodTrainingService`. Whatever the kind it reads together the period and the one before (`GetComparisonAsync`), its volume (`GetVolumeAsync`), its series (`GetSeriesAsync`), its records (`GetRecordsAsync`) and the stepper's limits for that kind (`GetLimitsAsync(kind)`). On top of that:

| Kind | Also reads | Shows from the series |
|---|---|---|
| Week | the sets per muscle group (`GetMuscleSetsAsync`), the streak and the last 12 weeks (`GetStreaksAsync` for this year) | the days, Monday to Sunday, beside the same day the week before |
| Month | the most trained exercises (`GetMostTrainedAsync`) | the days with a workout for the calendar, and a row per week cut to the month; in the month holding today the weeks yet to begin are left out and the last one ends today |
| Year | the longest streak among the weeks starting in that year (`GetStreaksAsync(year)`) | the workouts in each month, the average a week, and the records in each month that has begun (all twelve for a past year) |

Weeks run from Monday 00:00 to Sunday 23:59:59 in the phone's time zone, and today is taken afresh on each load.

- **Route**: `/trends?period=month&date=2026-11-01` opens the week, month or year holding that date, so a link or back can open a given period. A missing or unknown `period` shows the week; a missing or invalid `date`, or one after the period holding today, shows the period holding today.
- **Switching kind**: picking Week, Month or Year shows that kind's period holding the day being looked at (the week of 9 Nov gives November 2026, then 2026), or today's period when the one shown holds today (from 2026 down to November 2026, then to this week). The period is put in the URL. If the new kind began later than that period (its first workout is in a later month than the week's Monday), it moves to the first period of the kind.
- **Stepping**: the previous and next arrows show the period before or after, put its first day in the URL as `date`, and load it. Previous is disabled on the period of the first workout of that kind (the same as next with no workouts at all) and next on the period holding today. A step or a switch made while a load runs drops that load's result.
- **Compared with**: the period holding today is compared with the one before up to the same point (*Against Monday to Wednesday last week*, *Against October to the 18th*, *Against 2025 to 18 Nov*); a finished period with the one before it (*Against the week before*, *Against September*, *Against 2024*). A day the earlier month lacks becomes its last day.
- **Last 12 weeks** (Week) always ends at the current week, whichever week is shown.
- **Load failed**: if the period, the one before, the volume, the series, the limits, or what the kind also reads (the muscle groups and streak for a week, the most trained for a month, the streak for a year) fail to read, the page shows the error and Try again. If only the records fail, their card reads "No new records" (the year's "No records") and the rest shows; the failure is logged either way, with no workout contents.
- **Reloads** when the app comes back on screen and on pull to refresh (the spinner shows for at least half a second), whichever kind is shown. A reload that finds a new week, month or year has begun while the current one was shown moves to the new one.
- **Records**, and in the month the Most trained rows, open their exercise; back from the exercise returns to the same kind and period (the link carries `from=trends:month:2026-11-01`; the older `trends:2026-11-09` still means the week holding it). Muscle group rows, and in the year the Records by month rows, do nothing. Settings opened from the gear returns to the same period too (`/settings?from=trends:…`).
- **Workouts**: a day with a workout in the month calendar, a Weeks row with workouts, a day's columns with a workout in the week view and a month's column with workouts in the year view are buttons (when `OnOpenWorkouts` is set) that open [Workouts](./?path=/custom/screens-workouts-overview) with those dates (`first` and `last`; a month's end is cut to today), and `from` the period shown, so back returns to it. A day, row or column with no workouts isn't tappable.
- **Back**: Android back goes to This week (`AppScreen`'s `Tab`), whichever kind is shown.

States: [Week](./?path=/story/screens-trends--week) · [Week only](./?path=/story/screens-trends--week-only) · [Month](./?path=/story/screens-trends--month) · [Year](./?path=/story/screens-trends--year) · [Past week](./?path=/story/screens-trends--past-week) · [Past month](./?path=/story/screens-trends--past-month) · [Past year](./?path=/story/screens-trends--past-year) · [No workouts ever](./?path=/story/screens-trends--no-workouts-ever) · [No workouts, week](./?path=/story/screens-trends--no-workouts-week) · [No workouts, month](./?path=/story/screens-trends--no-workouts-month) · [No workouts, year](./?path=/story/screens-trends--no-workouts-year) · [Loading](./?path=/story/screens-trends--loading) · [Load failed](./?path=/story/screens-trends--load-failed)

`data-testid`: `trends-kind` (`trends-week`, `trends-month`, `trends-year`), `trends-stepper` (`-previous`, `-label`, `-next`), `trends-tiles`, `trends-days`, `trends-muscles`, `trends-records`, `trends-recent-weeks`, `trends-calendar`, `trends-weeks`, `trends-most-trained`, `trends-months`, `trends-streak`, `trends-records-by-month` (with the parts' own as prefixes), `settings`, `tab-bar`, `this-week-link`, `new-link`, `trends-link`, `trends-loading`, `trends-error`, `trends-retry`.
