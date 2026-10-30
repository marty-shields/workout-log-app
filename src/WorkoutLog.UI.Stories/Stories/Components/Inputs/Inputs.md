---
$attribute: CustomPage("Components/Inputs/Overview")
---

# Inputs

Number and duration inputs use the phone's numeric keyboard and 20px text. An invalid input has a red 2px border.

## `FieldInput`
Picks the input for a field from its kind: `NumberField` for whole and decimal numbers, `DurationField` for durations. Shows the field's label and unit.
- `data-testid`: `TestId`, passed to the input it picks.

## `NumberField`
One text box with `inputmode="numeric"` (whole) or `"decimal"`.
- `data-testid`: `TestId`.

## `DurationField`
Three boxes, h : mm : ss. When focus leaves, empty boxes become 0 and minutes and seconds are padded to two digits.
- `data-testid`: `{TestId}-h`, `{TestId}-mm`, `{TestId}-ss`.

## `RadioGroup`
A labelled group of choices; one is chosen. Each choice is a full-width row with a radio, its label and an optional hint under it. The chosen row is highlighted. Disabled dims every row and ignores taps.
- `data-testid`: `TestId` on the group; each option's `TestId` on its radio.

## `SegmentedControl`
A full-width row of 2–4 segments; one is chosen and filled with `--color-selected`. For an Exercise screen's tabs and range. Each segment is a radio with a 48px touch target; `Label` names the group for screen readers only.
- `data-testid`: `TestId` on the control; each option's `TestId` on its radio.

## `FilterChips`
A row of rounded chips; one is chosen and filled with `--color-selected`. For the Exercises list's All, Strength, Cardio. Each chip is a radio with a 48px touch target; `Label` names the group for screen readers only.
- `data-testid`: `TestId` on the group; each option's `TestId` on its radio.

Stories: [FieldInput](./?path=/docs/components-inputs-fieldinput--docs) · [NumberField](./?path=/docs/components-inputs-numberfield--docs) · [DurationField](./?path=/docs/components-inputs-durationfield--docs) · [RadioGroup](./?path=/docs/components-inputs-radiogroup--docs) · [SegmentedControl](./?path=/docs/components-inputs-segmentedcontrol--docs) · [FilterChips](./?path=/docs/components-inputs-filterchips--docs)
