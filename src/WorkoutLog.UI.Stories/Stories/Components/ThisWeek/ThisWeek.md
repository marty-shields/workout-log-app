---
$attribute: CustomPage("Components/ThisWeek/Overview")
---

# This week

Parts of the [This week](./?path=/custom/screens-this-week-overview) screen. Each takes display-ready values; the page works them out. The design they started from is [docs/design/this-week-and-trends.html](https://github.com/marty-shields/workout-log-app/blob/main/docs/design/this-week-and-trends.html).

## `DayStrip`
A card with the seven days of the week, Monday to Sunday: the weekday abbreviation over a circle.

| Day | Shows |
|---|---|
| Trained | A tick on a solid `--color-primary` circle. |
| Not trained, up to today | The date number in a dashed circle. |
| Today | A ring around the circle and a bold label, whether trained or not. `aria-current="date"`. |
| Still to come | The circle dimmed to `--opacity-disabled`. |

Colour is never the only sign: the tick, the dashed or solid outline, the ring and the dimming differ in shape. Each day also has hidden text for screen readers, e.g. *Monday 16 November, trained*, *Wednesday 18 November, no workout, today*, *Thursday 19 November, coming up*.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-monday` to `{TestId}-sunday`.

## `WeekTiles`
Three kit `StatTile`s in a row: Workouts, Time (e.g. *2h 35m*) and Sets, each with its change on last week from Monday to the same weekday: an increase is the Up `Icon` and the difference in `--color-success`, a decrease the Down `Icon` and the difference in `--color-error`, no change a muted "—". Screen readers hear "up 35m", "down 1" or "no change". Time is compared in whole minutes. Under the tiles: "Against Monday to Wednesday last week" (on a Monday, "Against Monday last week").
- `data-testid`: `TestId`, and as a prefix: `{TestId}-workouts`, `{TestId}-time`, `{TestId}-sets` (each with a `-change` part), `{TestId}-against`.

## `WeekWorkoutsCard`
A card titled "This week's workouts" with a `ListRow` per workout, newest first: day and time (e.g. *Wed 18 Nov · 07:10*), the first two exercises and `+n` for the rest (e.g. *Squat, Leg press +2*) under it, and the duration as h:mm (e.g. *0:58*) at the right. With `OnOpen` the rows are tappable and raise it with the workout id. None shows "Nothing yet this week. Tap + to start one."
- `data-testid`: `TestId`, and as a prefix: `{TestId}-row-{workout id}`, `{TestId}-none`.

## `WeekRecordsCard`
A card titled "Records this week" with a `ListRow` per record: exercise and best (e.g. *Bench press · 82.5kg × 5*) with the weekday under it (e.g. *Monday*). Up to five rows, then a muted "n more" (e.g. *2 more*). With `OnOpen` the rows are tappable and raise it with the exercise id. None shows "No new records this week".
- `data-testid`: `TestId`, and as a prefix: `{TestId}-row-{exercise id}`, `{TestId}-more`, `{TestId}-none`.

## `MuscleSetsCard`
A card titled "Sets per muscle group", "this week" at its right (`Period` renames it, e.g. "that week" in Trends), with a bar for each group, most sets first: the name, a bar as long as its sets against the busiest group's in `--color-accent` on `--color-border`, and the sets (e.g. *14 sets*, *1 set*). A group at zero has no bar and a muted "Not trained"; the groups at zero come last.
- `data-testid`: `TestId`, and as a prefix: `{TestId}-{group, lower case}`.

## `LastWeekCard`
A card with one `ListRow` "Last week" and, under it, last week's whole-week totals (e.g. *4 workouts · 4h 10m · 67 sets*). With `OnOpen` the row is tappable, with a chevron. This week shows it while nothing is logged yet.
- `data-testid`: `TestId`, and `{TestId}-row`.

Stories: [DayStrip](./?path=/docs/components-thisweek-daystrip--docs) · [WeekTiles](./?path=/docs/components-thisweek-weektiles--docs) · [WeekWorkoutsCard](./?path=/docs/components-thisweek-weekworkoutscard--docs) · [WeekRecordsCard](./?path=/docs/components-thisweek-weekrecordscard--docs) · [MuscleSetsCard](./?path=/docs/components-thisweek-musclesetscard--docs) · [LastWeekCard](./?path=/docs/components-thisweek-lastweekcard--docs)
