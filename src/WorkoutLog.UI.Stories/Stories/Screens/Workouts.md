---
$attribute: CustomPage("Screens/Workouts/Overview")
---

# Workouts

Saved workouts in a span of dates, newest first. View: `WorkoutsView`; page: `WorkoutsPage`.

| Part | Content |
|---|---|
| Top bar | Title "Workouts". No back arrow: Workouts is a top-level screen. |
| Dates | Fixed under the top bar. A `DateRangeField` with the dates shown and their workout count, e.g. *6 Aug – 6 Nov 2026* and *38*, over a full-width `SegmentedControl` with **3M**, **6M**, **1Y** and **All**. Screen readers read the segments as *Last 3 months*, *Last 6 months*, *Last year* and *All*. Hidden when there are no workouts. |
| Totals | Under the date field, above the first month header, scrolling with the list: a `WorkoutTotalsCard` of **Workouts**, **Sets**, **Time trained** (e.g. *12 h 30 min*, or *45 min* under an hour) and **Per week** (one decimal, e.g. *2.5*) for the dates shown. Per week is workouts divided by the dates' days over 7, counting a span under 7 days as 7. Hidden when the dates hold no workouts, while loading, and when the totals couldn't be read: the list shows as usual and the failure is only logged. |
| Content | The workouts in the dates, under sticky month headers such as *November 2026* with *4 workouts*. One `ListRow` per workout: day, date and start time without the year, e.g. *Thu 5 Nov · 18:05*. The list ends at the last row, with no note about workouts outside the dates. |
| Nothing in the dates | `EmptyState` *No workouts between these dates*, or *No workout on this day* for one day, with the hint "Choose different dates or a quick range above". |
| No workouts | `EmptyState` "No workouts yet", hint "Tap New Workout on the Dashboard to log one". |
| Calendar | Tapping the date field opens a `RangeCalendar` under it, in place of the segments and the list. Cancel or Android back closes it and keeps the dates; Show workouts shows the dates picked. |
| Loading | `Spinner` "Loading…" centred until the workouts load. When the dates change, the date field and segments stay and the spinner replaces the list. |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reads the counts and dates again when the counts read failed, on opening or on a reload, else only the dates shown. Focus moves to Try again. |
| Navigation bar | `BottomNav` with Workouts current. |

The screen always opens on 3M: from today less 3 calendar months to today, in the phone's time zone, the same span as the Exercise chart's 3M. A segment shows as chosen only while the dates match it exactly; dates picked in the calendar choose none. All runs from the first workout. On opening, the page reads how many workouts started on each local day: the date field's count, the calendar dots and month counts, and the first workout's day come from it. Each change of dates reads only the workouts in them.

The totals are read with the workouts for the dates shown, so they follow each change of dates, and a reply to earlier dates is dropped like the rows'.

The page reads the counts and then the workouts again when the app comes back on screen, and on [pull down to refresh](./?path=/custom/components-layout-overview). Each shows the spinner in place of the list, as on opening, then what was read, or the load error. After a pull the spinner stays up for at least half a second, so a fast reload doesn't blink. A pull while the spinner shows does nothing. Both take today afresh, so after midnight dates that match a segment move to that segment's span ending on the new today, and the segment stays chosen. Dates picked in the calendar stay as picked. Dates changed during a reload win over it.

Tapping a row opens the workout in Edit Workout. Android back: closes the calendar when it is open, else goes to the Dashboard. Dashboard or Exercises in the navigation bar: that screen.

States: [Empty](./?path=/story/screens-workouts--empty) · [With workouts](./?path=/story/screens-workouts--with-workouts) · [With totals](./?path=/story/screens-workouts--with-totals) · [Totals unavailable](./?path=/story/screens-workouts--totals-unavailable) · [1Y](./?path=/story/screens-workouts--1y) · [All](./?path=/story/screens-workouts--all) · [Dates picked](./?path=/story/screens-workouts--dates-picked) · [One day](./?path=/story/screens-workouts--one-day) · [Nothing in range](./?path=/story/screens-workouts--nothing-in-range) · [No workout on this day](./?path=/story/screens-workouts--no-workout-on-this-day) · [Picking dates](./?path=/story/screens-workouts--picking-dates) · [Picking month](./?path=/story/screens-workouts--picking-month) · [Loading](./?path=/story/screens-workouts--loading) · [Load failed](./?path=/story/screens-workouts--load-failed)

`data-testid`: `workouts-dates` (with `-dates` and `-count`), `workouts-range` and `workouts-range-3m`, `-6m`, `-1y`, `-all`, `workout-totals` (with `-workouts`, `-sets`, `-time`, `-per-week`), `workouts-month-{yyyy-MM}`, `workouts-row-{id}`, `workouts-none` (with `-message` and `-hint`), `workouts-empty`, `workouts-loading`, `workouts-error`, `workouts-retry`, the calendar's `range-*` ids, and the navigation bar's `bottom-nav`, `dashboard-link`, `workouts-link`, `exercises-link`.
