---
$attribute: CustomPage("Components/Charts/Overview")
---

# Charts

## `Chart`
An SVG line chart that fills its width. Each `ChartPoint` has a date, a value and an optional label; points are evenly spaced in order.

- The y-axis has round ticks (steps of 1, 2 or 5 × 10ⁿ, at least 0.1) spanning the values, as bare numbers. Equal values step by 1; ticks don't go below 0 when no value does. `TimeScale` steps on whole times (seconds, minutes, hours) and shows m:ss (h:mm:ss from an hour).
- A point with a label (e.g. ×6) shows it; above 8 points, only the best and the latest are labelled. A point without a label shows its value through `Format` (e.g. 82.3kg, D49) only when it is the best or the latest.
- The best point is larger and filled: the highest value, or the lowest when `Flipped`; the earliest of a tie. `Flipped` puts the lowest value at the top.
- A point with no value is skipped; the line joins across. One point draws a single dot.
- The x-axis shows the first and last dates.
- `Name` is the accessible name; a visually hidden list gives each date and value.
- Line and points `--color-accent` at `--chart-stroke-width`; grid `--color-border`; text `--color-text-muted`.
- `data-testid`: `TestId`; `{TestId}-values` on the list.

Stories: [Chart](./?path=/docs/components-charts-chart--docs)
