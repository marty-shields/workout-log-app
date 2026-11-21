---
$attribute: CustomPage("Components/Trends/Overview")
---

# Trends

Parts of the [Trends](./?path=/custom/screens-trends-overview) screen. Each takes display-ready values; the page works them out. The design they started from is [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html). The muscle group card is the This week one, with a `Period` that renames it: see [This week components](./?path=/custom/components-thisweek-overview).

## `TrendCard`
A card with a bold title, an optional muted aside at the right (e.g. *1 this week*, *23-week streak*) and whatever it is given underneath. The Trends cards that are not This week's are built from it.
- `data-testid`: `TestId`, and `{TestId}-aside`.

## `TrendTiles`
Workouts, Time, Sets and Volume in a two-by-two grid, kit `StatTile`s, each with its change on the period before, drawn as in `WeekTiles`: Up `Icon` and the difference in `--color-success`, Down `Icon` in `--color-error`, or a muted "—"; screen readers hear "up 35m", "down 1" or "no change". Time is compared in whole minutes. Volume is in kg for a week or month and in tonnes for a year (`DisplayFormat.Volume`), has the caption "strength", and its change is a percentage of the earlier volume (`DisplayFormat.PercentChange`); with nothing earlier, a percentage is undefined, so it shows the volume gained. Under the tiles: the `Against` line, e.g. *Against Monday to Wednesday last week*.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-workouts`, `{TestId}-time`, `{TestId}-sets`, `{TestId}-volume` (each tile with `-value`, `-label`, `-caption` and `-change` parts), `{TestId}-against`.

## `TrendBars`
A hand-drawn SVG bar chart that fills its width, one column per `TrendBar`. Its `AriaLabel` says what it shows and a visually hidden list gives each column's name and value in words (e.g. *Monday: 1h 02m (last week 1h 05m)*), so no figure depends on colour or on seeing the bars.
- **Paired** (`EarlierLabel` set): each column has the period before's bar to the left, in `--color-bar-past`, and this period's to the right, in `--color-accent`. A kit `ChartLegend` above names both (*This week*, *Last week*), so the colours are also told by order and words.
- **Single**: one bar per column; the current column's bar is `--color-accent`, the rest `--color-bar-past`. The current column's axis label is bold, and its value, and the highest column's, print above the bar, so "now" is not told by colour alone.
- Bars use the biggest value as the top of the plot unless `Max` is set; a column at 0 has no bar. When every column is 0 the chart is replaced by `EmptyText`. The baseline is `--color-border-strong`; axis text `--color-text-muted`.
- With `OnOpen` set, a column that has a value and a `TrendBar.Span` (the days it counts) is a button laid over the column, a touch target tall, with the hidden text *Open workouts, Monday*; it raises the `DateSpan`. Other columns do nothing.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-values` (the list), `{TestId}-legend`, `{TestId}-none`, and `{TestId}-{column, from 1}-open` on a tappable column.

## `MonthCalendar`
The days of a month in a Monday-first grid under the weekday initials, as circles like `DayStrip`'s: trained days are solid `--color-primary` with the date in `--color-on-primary`, other days a dashed circle, today a ring, and days after today dimmed to `--opacity-disabled`. A kit `ChartLegend` names the three shapes. Each day has hidden text, e.g. *Wednesday 18 November, trained, today*. The list is named "Days in November 2026".
- With `OnOpenDay` set, a day with a workout is a button (a touch target drawn over its cell, so the grid keeps its size) with the hidden text, e.g. *Wednesday 4 November, trained, open workouts*; it raises the date. Days with no workout stay as they are.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-1` to `{TestId}-31`, `{TestId}-{day}-open` on a tappable day, and `{TestId}-legend`.

## `MonthWeeks`
A row per week of the month: its dates cut to the month at the left (e.g. *16 – 18 Nov*) and its workouts and time at the right (e.g. *3 workouts · 2h 35m*). None shows "No weeks yet".
- With `OnOpen` set, a row that has workouts and a `MonthWeek.Span` (its dates) is a button with a chevron, a touch target tall; it raises the `DateSpan`.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-1` for the first row, `{TestId}-1-open` on its button, `{TestId}-none`.

## `MeterRows`
Rows of a name, a bar as long as its number against the biggest and the number, as `MuscleSetsCard` draws its groups. `Wide` makes room for exercise names (*Overhead press*); without it the name column fits a month's three letters. `Unit` (e.g. *sessions*) is read by screen readers after each number, and `AriaLabel` names the list. No rows, or all at 0, shows `EmptyText`.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-{label, lower case, dashes for spaces}`, `{TestId}-none`.

## `RecordRows`
The records of a period as `ListRow`s inside a `TrendCard` titled "Records" with "n this month" as its aside: exercise and best (e.g. *Bench press · 82.5kg × 5*) with the date under it (a `WeekRecord`'s `When`, e.g. *Wed 18 Nov*, else its weekday). Up to five rows, then a muted "n more". With `OnOpen` the rows are tappable and raise it with the exercise id. None shows `EmptyText`.
- `data-testid`: as a prefix: `{TestId}-row-{exercise id}`, `{TestId}-more`, `{TestId}-none`.

## `LongestStreakCard`
One row, *Longest streak*, with the weeks at the right (*23 weeks*, *1 week*, or *None* at 0) and, under it, *Still going* when the run reaches this week.
- `data-testid`: `TestId`, and `{TestId}-row`.

Stories: [TrendCard](./?path=/docs/components-trends-trendcard--docs) · [TrendTiles](./?path=/docs/components-trends-trendtiles--docs) · [TrendBars](./?path=/docs/components-trends-trendbars--docs) · [MonthCalendar](./?path=/docs/components-trends-monthcalendar--docs) · [MonthWeeks](./?path=/docs/components-trends-monthweeks--docs) · [MeterRows](./?path=/docs/components-trends-meterrows--docs) · [RecordRows](./?path=/docs/components-trends-recordrows--docs) · [LongestStreakCard](./?path=/docs/components-trends-longeststreakcard--docs)
