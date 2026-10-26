---
$attribute: CustomPage("Components/Inputs/Overview")
---

# Inputs

Number and duration inputs use the phone's numeric keyboard and 20px text (D79, D86). An invalid input has a red 2px border.

## `FieldInput`
Picks the input for a field from its kind: `NumberField` for whole and decimal numbers, `DurationField` for duration and pace. Shows the field's label and unit.
- `data-testid`: `TestId`, passed to the input it picks.

## `NumberField`
One text box with `inputmode="numeric"` (whole) or `"decimal"`.
- `data-testid`: `TestId`.

## `DurationField`
Three boxes, h : mm : ss, or two for pace, m : ss. When focus leaves, empty boxes become 0 and minutes and seconds are padded to two digits (D31).
- `data-testid`: `{TestId}-h`, `{TestId}-mm`, `{TestId}-ss` (pace: `{TestId}-m`, `{TestId}-ss`).

Stories: [FieldInput](./?path=/docs/components-inputs-fieldinput--docs) · [NumberField](./?path=/docs/components-inputs-numberfield--docs) · [DurationField](./?path=/docs/components-inputs-durationfield--docs)
