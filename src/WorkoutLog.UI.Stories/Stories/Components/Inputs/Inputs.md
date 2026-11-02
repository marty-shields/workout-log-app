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

## `NoteField`
A note on a workout or an exercise. The parent has the note button (an `IconButton`) that opens it, and sets `Editing`.
- No note and `Editing` off: nothing shows.
- Editing: the label and a counter such as *120/500* on one line, over a three-row text box that has no length limit. At the limit the counter reads *500/500*; above it the counter is red. Setting `Editing` from the parent, or tapping the saved note, moves focus into the text box.
- Saved note: one plain line, cut with … when long, in the muted text colour, with no border or fill. Tap to edit. Leaving the text box collapses it to this line.
- The text is trimmed when the field collapses. Blank or only spaces is no note: `ValueChanged` gets null.
- With an `Error` the text box or the saved line gets a red 2px border and the message shows under it in red. It is not set by the field: the screen sets it, e.g. *Note must be 500 characters or less*.
- `FocusAsync` opens the text box and focuses it.
- `data-testid`: `TestId`, `{TestId}-input`, `{TestId}-count`, `{TestId}-open` (saved line), `{TestId}-error`.
## `RadioGroup`
A labelled group of choices; one is chosen. Each choice is a full-width row with a radio, its label and an optional hint under it. The chosen row is highlighted. Disabled dims every row and ignores taps.
- `data-testid`: `TestId` on the group; each option's `TestId` on its radio.

## `SegmentedControl`
A full-width row of 2–4 segments; one is chosen and filled with `--color-selected`. For an Exercise screen's tabs and range. Each segment is a radio with a 48px touch target; `Label` names the group for screen readers only.
- `data-testid`: `TestId` on the control; each option's `TestId` on its radio.

## `FilterChips`
A row of rounded chips; one is chosen and filled with `--color-selected`. For the Exercises list's All, Strength, Cardio. Each chip is a radio with a 48px touch target; `Label` names the group for screen readers only.
- `data-testid`: `TestId` on the group; each option's `TestId` on its radio.

Stories: [FieldInput](./?path=/docs/components-inputs-fieldinput--docs) · [NumberField](./?path=/docs/components-inputs-numberfield--docs) · [NoteField](./?path=/docs/components-inputs-notefield--docs) · [DurationField](./?path=/docs/components-inputs-durationfield--docs) · [RadioGroup](./?path=/docs/components-inputs-radiogroup--docs) · [SegmentedControl](./?path=/docs/components-inputs-segmentedcontrol--docs) · [FilterChips](./?path=/docs/components-inputs-filterchips--docs)
