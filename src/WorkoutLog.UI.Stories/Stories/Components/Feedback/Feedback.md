---
$attribute: CustomPage("Components/Feedback/Overview")
---

# Feedback

## `Spinner`
A turning ring with a label under it, for work in progress. The ring is `--color-border` with a `--color-accent` arc, `--spinner-size` across, one turn every `--spinner-duration`; it stands still when the phone asks for reduced motion. The label is muted text. `role="status"` (a live region).
- `data-testid`: `TestId`.

## `LoadFailed`
A load error with **Try again**, in place of the content that didn't load. The message is bold `--color-error` text in a `--color-error` box (`role="alert"`); Try again is a Secondary full-width `AppButton` under it. Focus moves to Try again when it shows. A failed focus logs Warning 3301 and the error stays.
- `data-testid`: `{TestId}-error` on the message, `{TestId}-retry` on Try again.

## `Snackbar`
A short message about something just done, with one **Undo** text button, e.g. *Barbell Back Squat set 2 deleted*. A long message wraps; Undo keeps its place on the right. It doesn't block the screen: it sits in `AppScreen`'s `Status` slot, just above the bottom bar, and covers neither the bar nor the safe-area inset. Android back keeps its meaning on the screen; the snackbar is not a dialog. One shows at a time; the screen replaces it for the next message.
- Colours: `--color-inverse-surface` with `--color-on-inverse-surface` text (dark on a light screen, light on a dark one), Undo in bold `--color-inverse-primary`, a full `--touch-target` tall. `--radius-sm` corners, 16px from the screen edges.
- Rises and fades in over `--snackbar-enter-duration`; just appears when the phone asks for reduced motion.
- `OnTimeout` is raised once, 10 seconds after it first shows (`Snackbar.Duration`; Material asks for 4 to 10, and the top of that range gives a TalkBack user, who reaches Undo by swiping, time to get there), timed with the injected `TimeProvider`; removing it first cancels the timer. While Undo has keyboard focus (it matches `:focus-visible`, read by `Snackbar.razor.js`) the timer stops, and when focus leaves it a full 10 seconds start again (WCAG 2.2.1, Timing Adjustable). Focus moved to Undo by `TakeFocus` after a tap doesn't stop it. A failed check logs Warning 3902 and the timer runs on. A failure in `OnTimeout` goes to the nearest error boundary. A new message is a new snackbar (`@key`), so its time starts again. `OnUndo` is raised by Undo; the snackbar doesn't hide itself.
- `TakeFocus` moves focus to Undo when it first shows. The workout screens set it, because the trash that had focus has just gone with what it deleted; TalkBack then reads Undo and a keyboard user can undo at once. A failed focus logs Warning 3901 and the snackbar stays.
- `data-testid`: `TestId` (default `snackbar`), `{TestId}-message`, `{TestId}-undo`.

Stories: [Spinner](./?path=/docs/components-feedback-spinner--docs) · [LoadFailed](./?path=/docs/components-feedback-loadfailed--docs) · [Snackbar](./?path=/docs/components-feedback-snackbar--docs)
