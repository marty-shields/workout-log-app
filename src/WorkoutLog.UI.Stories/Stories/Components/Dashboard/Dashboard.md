---
$attribute: CustomPage("Components/Dashboard/Overview")
---

# Dashboard

Weekly figures shared by two screens: `WeekChange` in the [This week](./?path=/custom/screens-this-week-overview) tiles, and `WeekBars` in the Last 12 weeks card of [Trends](./?path=/custom/screens-trends-overview) (week view). There is no Dashboard screen; the group keeps the name.

## `WeekChange`
How a total moved between two weeks, shown by the This week tiles: an increase is the Up `Icon` and the difference in `--color-success`, a decrease the Down `Icon` and the difference in `--color-error`, no change a muted "—". `Format` shows the size of the difference. Screen readers hear "up 25m", "down 1" or "no change".

## `WeekBars`
One bar per week, oldest on the left, height in proportion to the busiest week, on a baseline. This week is the last bar, in `--color-accent`; the others are `--color-bar-past`. A week with no workouts has no bar. Muted labels under the baseline name the oldest week and This week. The figure is named "Workouts per week, last 12 weeks" and lists each week for screen readers, e.g. *11 weeks ago: 2 workouts* … *This week: 1 workout*. With no workouts in any week, a muted "No workouts in the last 12 weeks" replaces the chart.
- `data-testid`: `TestId`; `{TestId}-values` on the list; `{TestId}-none` on the message.

Stories: [WeekChange](./?path=/docs/components-dashboard-weekchange--docs) · [WeekBars](./?path=/docs/components-dashboard-weekbars--docs)
