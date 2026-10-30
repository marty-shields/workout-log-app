---
$attribute: CustomPage("Components/Dashboard/Overview")
---

# Dashboard

Cards for the Dashboard's slots. Each takes display-ready values; the page works them out.

## `LastWorkoutCard`
A card labelled "Last workout": the date and day in bold (e.g. *Tue 27 Oct 2026*), then how long ago and how long it took (e.g. *3 days ago · 1:12:04*; *Today*, *Yesterday*), then the first two exercises and `+n` for the rest (e.g. *Barbell Bench Press, Cable Fly +3*). With `OnOpen` the card is a button ending in a muted chevron. No workout shows "No workouts yet" and is not tappable.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-date`, `{TestId}-when`, `{TestId}-exercises`, `{TestId}-empty`.

## `WeekComparison`
A table titled "This week vs last week" with This week and Last week columns and the rows Workouts, Time (e.g. *3h 05m*, *25m*) and Sets. Each row ends in its change: an increase is the Up `Icon` and the difference in `--color-success`, a decrease the Down `Icon` and the difference in `--color-error`, no change a muted "—". Screen readers hear "up 25m", "down 1" or "no change". Time is compared in whole minutes. Under it, muted: "Last week up to the same day and time".
- `data-testid`: `TestId`, and as a prefix: `{TestId}-workouts`, `{TestId}-time`, `{TestId}-sets`.

## `WeekBars`
One bar per week, oldest on the left, height in proportion to the busiest week, on a baseline. This week is the last bar, in `--color-accent`; the others are `--color-border-strong`. A week with no workouts has no bar. Muted labels under the baseline name the oldest week and This week. The figure is named "Workouts per week, last 12 weeks" and lists each week for screen readers, e.g. *11 weeks ago: 2 workouts* … *This week: 1 workout*.
- `data-testid`: `TestId`; `{TestId}-values` on the list.

## `ConsistencyCard`
A card with a `WeekComparison`, then "Last 12 weeks" with the streak at the right (e.g. *6-week streak*, hidden at 0) and `WeekBars`, then a `ListRow` "Muscles, last 7 days". With `OnMuscles` the row is tappable, with a chevron.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-comparison`, `{TestId}-streak`, `{TestId}-bars`, `{TestId}-muscles`.

## `RecentRecordsCard`
A card titled "Recent records" with a `ListRow` per record: exercise and best (e.g. *Deadlift · 160kg × 3*) with how long ago under it (e.g. *2 days ago*). With `OnOpen` the rows are tappable and raise it with the exercise id. Shows the records it is given. None shows "No new records in the last 30 days".
- `data-testid`: `TestId`, and as a prefix: `{TestId}-row-{exercise id}`, `{TestId}-none`.

Stories: [LastWorkoutCard](./?path=/docs/components-dashboard-lastworkoutcard--docs) · [WeekComparison](./?path=/docs/components-dashboard-weekcomparison--docs) · [WeekBars](./?path=/docs/components-dashboard-weekbars--docs) · [ConsistencyCard](./?path=/docs/components-dashboard-consistencycard--docs) · [RecentRecordsCard](./?path=/docs/components-dashboard-recentrecordscard--docs)
