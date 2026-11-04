---
$attribute: CustomPage("Components/Charts/Overview")
---

# Charts

## `Chart`
An SVG line chart that fills its width. Each `ChartPoint` has a date, a value and an optional label; points are evenly spaced in order.

- The y-axis has round ticks (steps of 1, 2 or 5 × 10ⁿ, at least 0.1) spanning the values, as bare numbers. Equal values step by 1; ticks don't go below 0 when no value does. `TimeScale` steps on whole times (seconds, minutes, hours) and shows m:ss (h:mm:ss from an hour).
- Only the best and the latest points are labelled, so labels never crowd: each shows its label (e.g. ×6), else its value through `Format` (e.g. 82.3kg). When the latest is the best it has one label. The best is the highest value, or the lowest when `Flipped`; the most recent of a tie. `Flipped` puts the lowest value at the top.
- The latest point is larger and filled; the best has a thin ring round it, so the two differ by shape, not colour.
- A label sits over its point, centred. Near the left edge it starts at the point, clear of the y-axis ticks; near the right edge it ends at the point. When the best and latest labels would overlap, the latest goes under its point; when that would reach the dates, both stack over the best point, the latest's nearest it.
- A point with no value is skipped; the line joins across. One point draws a single dot.
- The x-axis shows the first and last dates.
- `Name` is the accessible name; a visually hidden list gives each date and value.
- Line and points `--color-accent` at `--chart-stroke-width`, the best ring at `--border-width`; grid `--color-border`; text `--color-text-muted`.
- `data-testid`: `TestId`; `{TestId}-values` on the list.

Stories: [Chart](./?path=/docs/components-charts-chart--docs) · [Best beside latest](./?path=/story/components-charts-chart--best-beside-latest) · [Best beside latest at the bottom](./?path=/story/components-charts-chart--best-beside-latest-at-the-bottom)
