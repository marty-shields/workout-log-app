---
$attribute: CustomPage("Screens/Workouts/Overview")
---

# Workouts

Saved workouts in a span of dates, newest first. View: `WorkoutsView`; page: `WorkoutsPage`.

| Part | Content |
|---|---|
| Top bar | Title "Workouts". No back arrow: Workouts is a top-level screen. |
| Dates | Fixed under the top bar. A `DateRangeField` with the dates shown and their workout count, e.g. *6 Aug – 6 Nov 2026* and *38*, over a full-width `SegmentedControl` with **3M**, **6M**, **1Y** and **All**. Screen readers read the segments as *Last 3 months*, *Last 6 months*, *Last year* and *All workouts*. Hidden when there are no workouts. |
| Content | The workouts in the dates, under sticky month headers such as *November 2026* with *4 workouts*. One `ListRow` per workout: day, date and start time without the year, e.g. *Thu 5 Nov · 18:05*. When the dates leave workouts out, the list ends with *Workouts outside these dates are hidden* and a Secondary **Show all workouts** that picks All. |
| Nothing in the dates | `EmptyState` *No workouts between these dates*, or *No workout on this day* for one day, with a Secondary **Change dates** that opens the calendar. |
| No workouts | `EmptyState` "No workouts yet", hint "Tap New Workout on the Dashboard to log one". |
| Calendar | Tapping the date field opens a `RangeCalendar` under it, in place of the segments and the list. Cancel or Android back closes it and keeps the dates; Show workouts shows the dates picked. |
| Loading | `Spinner` "Loading…" centred until the workouts load. When the dates change, the date field and segments stay and the spinner replaces the list. |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reads again what failed: the counts, or the dates shown. Focus moves to Try again. |
| Navigation bar | `BottomNav` with Workouts current. |

The screen always opens on 3M: from today less 3 calendar months to today, in the phone's time zone, the same span as the Exercise chart's 3M. A segment shows as chosen only while the dates match it exactly; dates picked in the calendar choose none. All runs from the first workout. On opening, the page reads how many workouts started on each local day: the date field's count, the calendar dots and month counts, and the first workout's day come from it. Each change of dates reads only the workouts in them.

Tapping a row opens the workout in Edit Workout. Android back: closes the calendar when it is open, else goes to the Dashboard. Dashboard or Exercises in the navigation bar: that screen.

States: [Empty](./?path=/story/screens-workouts--empty) · [With workouts](./?path=/story/screens-workouts--with-workouts) · [1Y](./?path=/story/screens-workouts--1y) · [All](./?path=/story/screens-workouts--all) · [Dates picked](./?path=/story/screens-workouts--dates-picked) · [One day](./?path=/story/screens-workouts--one-day) · [Nothing in range](./?path=/story/screens-workouts--nothing-in-range) · [Picking dates](./?path=/story/screens-workouts--picking-dates) · [Picking month](./?path=/story/screens-workouts--picking-month) · [Loading](./?path=/story/screens-workouts--loading) · [Load failed](./?path=/story/screens-workouts--load-failed)

`data-testid`: `workouts-dates` (with `-dates` and `-count`), `workouts-range` and `workouts-range-3m`, `-6m`, `-1y`, `-all`, `workouts-month-{yyyy-MM}`, `workouts-row-{id}`, `workouts-hidden`, `workouts-show-all`, `workouts-none` (with `-message`), `workouts-change-dates`, `workouts-empty`, `workouts-loading`, `workouts-error`, `workouts-retry`, the calendar's `range-*` ids, and the navigation bar's `bottom-nav`, `dashboard-link`, `workouts-link`, `exercises-link`.
