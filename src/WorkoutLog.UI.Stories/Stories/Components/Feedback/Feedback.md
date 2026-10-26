---
$attribute: CustomPage("Components/Feedback/Overview")
---

# Feedback

## `Spinner`
A turning ring with a label under it, for work in progress. The ring is `--color-border` with a `--color-accent` arc, `--spinner-size` across, one turn every `--spinner-duration`; it stands still when the phone asks for reduced motion (D138). The label is muted text. `role="status"` (a live region).
- `data-testid`: `TestId`.

## `LoadFailed`
A load error with **Try again**, in place of the content that didn't load. The message is bold `--color-error` text in a `--color-error` box (`role="alert"`); Try again is a Secondary full-width `AppButton` under it. Focus moves to Try again when it shows (D180). A failed focus logs Warning 3301 and the error stays.
- `data-testid`: `{TestId}-error` on the message, `{TestId}-retry` on Try again.

Stories: [Spinner](./?path=/docs/components-feedback-spinner--docs) · [LoadFailed](./?path=/docs/components-feedback-loadfailed--docs)
