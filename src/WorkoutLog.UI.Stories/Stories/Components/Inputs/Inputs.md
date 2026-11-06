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
A full-width row of 2–4 segments; one is chosen and filled with `--color-selected`. For an Exercise screen's tabs and range, and the Workouts screen's quick dates. Each segment is a radio with a 48px touch target; `Label` names the group for screen readers only. An option's `AriaLabel`, when set, is what screen readers read instead of its label, e.g. *Last 3 months* for *3M*. When `Value` matches no option, no segment is chosen.
- `data-testid`: `TestId` on the control; each option's `TestId` on its radio.

## `DateRangeField`
A 48px field that shows the dates on screen and how many workouts they hold, e.g. *6 Aug – 6 Nov 2026* and *38*: one year as *6 Aug – 6 Nov 2026*, across years as *6 Nov 2025 – 6 Nov 2026*, one day as *Wed 12 Mar 2025*. Screen readers read *Dates shown: 6 Aug – 6 Nov 2026. 38 workouts*. Tapping it raises `ExpandedChanged`; when `Expanded` the field gets a 2px focus-colour border, a `RangeCalendar` opens under it, and Android back closes it. Show workouts closes it and raises `DatesChanged`.
- `data-testid`: `TestId` on the field, `{TestId}-dates`, `{TestId}-count`.

## `RangeCalendar`
Picks one day or a span of days, from `Dates` when it opens. From and To boxes sit over a month of days; the pressed box is the end the next tap sets, and tapping a box shows its month.
- The first tap sets From, clears To and moves on to To. The second sets To, swapping the two if it is earlier; tapping the same day again picks one day.
- Weeks start on Monday. Days with a workout have a dot and read as *Wed 4 Nov, workout logged*. The span is filled with `--color-selected`, both ends with `--color-primary`, and today has an outline. Days after today or before the first workout are disabled, and so are the month arrows past them.
- The month name opens a grid of the year's months with their workout counts; months outside the history are disabled. Choosing one shows its days.
- Cancel raises `OnCancel`; Show workouts raises `OnApply` with the days picked.
- `data-testid`: `range-calendar`, `range-from`, `range-to`, `range-prev-month`, `range-month-name`, `range-next-month`, `range-day-{yyyy-MM-dd}`, `range-prev-year`, `range-year`, `range-next-year`, `range-month-{yyyy-MM}`, `range-hint`, `range-cancel`, `range-apply`.

## `FilterChips`
A row of rounded chips; one is chosen and filled with `--color-selected`. For the Exercises list's All, Strength, Cardio. Each chip is a radio with a 48px touch target; `Label` names the group for screen readers only.
- `data-testid`: `TestId` on the group; each option's `TestId` on its radio.

Stories: [FieldInput](./?path=/docs/components-inputs-fieldinput--docs) · [NumberField](./?path=/docs/components-inputs-numberfield--docs) · [NoteField](./?path=/docs/components-inputs-notefield--docs) · [DurationField](./?path=/docs/components-inputs-durationfield--docs) · [RadioGroup](./?path=/docs/components-inputs-radiogroup--docs) · [SegmentedControl](./?path=/docs/components-inputs-segmentedcontrol--docs) · [DateRangeField](./?path=/docs/components-inputs-daterangefield--docs) · [RangeCalendar](./?path=/docs/components-inputs-rangecalendar--docs) · [FilterChips](./?path=/docs/components-inputs-filterchips--docs)
