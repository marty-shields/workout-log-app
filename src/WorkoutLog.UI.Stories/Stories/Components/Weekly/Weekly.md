---
$attribute: CustomPage("Components/Weekly/Overview")
---

# Weekly

`WeekBars` in the Last 12 weeks card of [Trends](./?path=/custom/screens-trends-overview) (week view).

## `WeekBars`
One bar per week, oldest on the left, height in proportion to the busiest week, on a baseline. This week is the last bar, in `--color-accent`; the others are `--color-bar-past`. A week with no workouts has no bar. Muted labels under the baseline name the oldest week and This week. The figure is named "Workouts per week, last 12 weeks" and lists each week for screen readers, e.g. *11 weeks ago: 2 workouts* … *This week: 1 workout*. With no workouts in any week, a muted "No workouts in the last 12 weeks" replaces the chart.
- `data-testid`: `TestId`; `{TestId}-values` on the list; `{TestId}-none` on the message.

Stories: [WeekBars](./?path=/docs/components-weekly-weekbars--docs)
