---
$attribute: CustomPage("Components/Charts/Overview")
---

# Charts

## `Chart`
An SVG line chart that fills its width. Each `ChartPoint` has a date, a value and an optional label; points are evenly spaced in order.

- The y-axis has round ticks (steps of 1, 2 or 5 × 10ⁿ) spanning the values, as bare numbers. `TimeScale` steps on whole times (seconds, minutes, hours) and shows m:ss.
- Each point shows its label (e.g. ×6), else its value through `Format` (e.g. 82.3kg, D49). Above 8 points, only the best and the latest are labelled.
- The best point is larger and filled: the highest value, or the lowest when `Flipped`; the latest of a tie. `Flipped` puts the lowest value at the top.
- A point with no value is skipped; the line joins across. One point draws a single dot.
- The x-axis shows the first and last dates.
- `Name` is the accessible name; a visually hidden list gives each date and value.
- Line and points `--color-accent`; grid `--color-border`; text `--color-text-muted`.
- `data-testid`: `TestId`; `{TestId}-values` on the list.

Stories: [Chart](./?path=/docs/components-charts-chart--docs)
