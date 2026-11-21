---
$attribute: CustomPage(StoryTitles.Prefix + "/Components/Trends/Overview")
---

# Trends

Parts of the [Trends](./?path=/custom/workout-log-screens-trends-overview) screen. Each takes display-ready values; the page works them out. The charts and bar lists are the kit's `TrendBarChart` and `BarList`, drawn by the screen. The design they started from is [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html). The muscle group card is the This week one, with a `Period` that renames it: see [This week components](./?path=/custom/workout-log-components-thisweek-overview).

## `TrendTiles`
Workouts, Time, Sets and Volume in a two-by-two grid, kit `StatTile`s, each with its change on the period before, drawn as in `WeekTiles`: Up or Down `Icon` and the difference, both muted (the arrow gives the direction), or a muted "—"; screen readers hear "up 35m", "down 1" or "no change". Time is compared in whole minutes. Volume is in kg for a week or month and in tonnes for a year (`DisplayFormat.Volume`), has the caption "strength", and its change is a percentage of the earlier volume (`DisplayFormat.PercentChange`); with nothing earlier, a percentage is undefined, so it shows the volume gained. Under the tiles: the `Against` line, e.g. *Against Monday to Wednesday last week*.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-workouts`, `{TestId}-time`, `{TestId}-sets`, `{TestId}-volume` (each tile with `-value`, `-label`, `-caption` and `-change` parts), `{TestId}-against`.

## `MonthWeeks`
A row per week of the month: its dates cut to the month at the left (e.g. *16 – 18 Nov*) and its workouts and time at the right (e.g. *3 workouts · 2h 35m*). None shows "No weeks yet".
- With `OnOpen` set, a row that has workouts and a `MonthWeek.Span` (its dates) is a button with a chevron, a touch target tall; it raises the `DateSpan`.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-1` for the first row, `{TestId}-1-open` on its button, `{TestId}-none`.

## `RecordRows`
The records of a period as `ListRow`s inside a kit `Card` titled "Records" with "n this month" as its aside: exercise and best (e.g. *Bench press · 82.5kg × 5*) with the date under it (a `WeekRecord`'s `When`, e.g. *Wed 18 Nov*, else its weekday). Up to five rows, then a muted "n more". With `OnOpen` the rows are tappable and raise it with the exercise id. None shows `EmptyText`.
- `data-testid`: as a prefix: `{TestId}-row-{exercise id}`, `{TestId}-more`, `{TestId}-none`.

## `LongestStreakCard`
One row, *Longest streak*, with the weeks at the right (*23 weeks*, *1 week*, or *None* at 0) and, under it, *Still going* when the run reaches this week.
- `data-testid`: `TestId`, and `{TestId}-row`.

Stories: [TrendTiles](./?path=/docs/workout-log-components-trends-trendtiles--docs) · [MonthWeeks](./?path=/docs/workout-log-components-trends-monthweeks--docs) · [RecordRows](./?path=/docs/workout-log-components-trends-recordrows--docs) · [LongestStreakCard](./?path=/docs/workout-log-components-trends-longeststreakcard--docs)
