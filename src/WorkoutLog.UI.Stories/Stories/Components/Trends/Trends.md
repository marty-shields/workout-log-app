---
$attribute: CustomPage("Components/Trends/Overview")
---

# Trends

Parts of the [Trends](./?path=/custom/screens-trends-overview) screen. Each takes display-ready values; the page will work them out. The design they follow is [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html). The records and muscle group cards are the This week ones, with a `Period` that renames them: see [This week components](./?path=/custom/components-thisweek-overview).

## `PeriodStepper`
A previous arrow, the period's label and a next arrow in one row: *16 – 22 Nov*, *November 2026* or *2026*. The arrows are round buttons at `--touch-target` named by `Kind` (*Previous week*, *Next month*); the previous one is the chevron mirrored. An arrow is disabled (at `--opacity-disabled`) when its `CanGo` flag is false: previous at the period of the first workout, next at the period that holds today. The label is a polite live region, so a screen reader hears the new period.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-previous`, `{TestId}-label`, `{TestId}-next`.

## `TrendCard`
A card with a bold title, an optional muted aside at the right (e.g. *1 this week*, *23-week streak*) and whatever it is given underneath. The Trends cards that are not This week's are built from it.
- `data-testid`: `TestId`, and `{TestId}-aside`.

## `TrendTiles`
Workouts, Time, Sets and Volume in a two-by-two grid, each with its change on the period before, drawn as `WeekTiles` does: Up `Icon` and the difference in `--color-success`, Down `Icon` in `--color-error`, or a muted "—"; screen readers hear "up 35m", "down 1" or "no change". Time is compared in whole minutes. Volume is in kg for a week or month and in tonnes for a year (`DisplayFormat.Volume`), carries the word "strength", and its change is a percentage of the earlier volume (`DisplayFormat.PercentChange`); with nothing earlier, a percentage is undefined, so it shows the volume gained. Under the tiles: the `Against` line, e.g. *Against Monday to Wednesday last week*.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-workouts`, `{TestId}-time`, `{TestId}-sets`, `{TestId}-volume` (each with a `-change` part), `{TestId}-against`.

## `TrendBars`
A hand-drawn SVG bar chart that fills its width, one column per `TrendBar`. Its `AriaLabel` says what it shows and a visually hidden list gives each column's name and value in words (e.g. *Monday: 1h 02m (last week 1h 05m)*), so no figure depends on colour or on seeing the bars.
- **Paired** (`EarlierLabel` set): each column has the period before's bar to the left, in `--color-bar-past`, and this period's to the right, in `--color-accent`. A legend above names both (*This week*, *Last week*), so the colours are also told by order and words.
- **Single**: one bar per column; the current column's bar is `--color-accent`, the rest `--color-bar-past`. The current column's axis label is bold, and its value, and the highest column's, print above the bar, so "now" is not told by colour alone.
- Bars use the biggest value as the top of the plot unless `Max` is set; a column at 0 has no bar. When every column is 0 the chart is replaced by `EmptyText`. The baseline is `--color-border-strong`; axis text `--color-text-muted`.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-values` (the list), `{TestId}-legend`, `{TestId}-none`.

## `MonthCalendar`
The days of a month in a Monday-first grid under the weekday initials, as circles like `DayStrip`'s: trained days are solid `--color-primary` with the date in `--color-on-primary`, other days a dashed circle, today a ring, and days after today dimmed to `--opacity-disabled`. A legend names the three shapes. Each day has hidden text, e.g. *Wednesday 18 November, trained, today*. The list is named "Days in November 2026".
- `data-testid`: `TestId`, and as a prefix: `{TestId}-1` to `{TestId}-31`.

## `MonthWeeks`
A row per week of the month: its dates cut to the month at the left (e.g. *16 – 18 Nov*) and its workouts and time at the right (e.g. *3 workouts · 2h 35m*). None shows "No weeks yet".
- `data-testid`: `TestId`, and as a prefix: `{TestId}-1` for the first row, `{TestId}-none`.

## `MeterRows`
Rows of a name, a bar as long as its number against the biggest and the number, as `MuscleSetsCard` draws its groups. `Wide` makes room for exercise names (*Overhead press*); without it the name column fits a month's three letters. `Unit` (e.g. *sessions*) is read by screen readers after each number. No rows, or all at 0, shows `EmptyText`.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-{label, lower case, dashes for spaces}`, `{TestId}-none`.

## `LongestStreakCard`
One row, *Longest streak*, with the weeks at the right (*23 weeks*, *1 week*, or *None yet* at 0) and, under it, *Still going* when the run reaches this week.
- `data-testid`: `TestId`, and `{TestId}-row`.

Stories: [PeriodStepper](./?path=/docs/components-trends-periodstepper--docs) · [TrendCard](./?path=/docs/components-trends-trendcard--docs) · [TrendTiles](./?path=/docs/components-trends-trendtiles--docs) · [TrendBars](./?path=/docs/components-trends-trendbars--docs) · [MonthCalendar](./?path=/docs/components-trends-monthcalendar--docs) · [MonthWeeks](./?path=/docs/components-trends-monthweeks--docs) · [MeterRows](./?path=/docs/components-trends-meterrows--docs) · [LongestStreakCard](./?path=/docs/components-trends-longeststreakcard--docs)
