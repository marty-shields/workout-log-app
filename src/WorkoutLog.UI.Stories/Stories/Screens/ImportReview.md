---
$attribute: CustomPage("Screens/Import Review/Overview")
---

# Import review

Opened from Settings after a file is picked and read (FR-52). View: `ImportReviewView`; page: `ImportReviewPage`. Nothing is written until Import or Replace all.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Import workouts". |
| File | The file name, then "44 workouts in the file" ("1 workout in the file"), grouped from 1,000. |
| Counts | Four `StatTile`s: New, Already on this phone, Different on this phone, Can't be imported. |
| How to import | `RadioGroup`: **Add to my workouts** (chosen first; adds New, never deletes) and **Replace all my workouts**, with the hint "Deletes the 41 on this phone" ("Deletes the 1 on this phone"; with none, "No workouts on this phone to delete"). |
| Different workouts | Add only, when there are any: **Keep this phone's** (chosen first) or **Use the file's**, which replaces the phone workout's finish time, exercises, sets and entries. |
| Can't be imported | "4 workouts can't be imported and will be skipped" ("1 workout…"), when there are any. |
| Nothing new | Add with no New, and Different absent or kept: "Nothing new to import"; Import is disabled. |
| Bottom bar | Secondary **Cancel** and Primary **Import**. With Replace all chosen, a Danger **Replace all**. When no workout can be imported, both are disabled. |
| Replace confirm | `ConfirmDialog` "Delete your 41 workouts and import 44?" with **Cancel** and Danger **Replace**, also with 0 on the phone. |
| Importing | Import is busy: a spinner beside "Importing…". Cancel and the choices are disabled; taps and back are ignored. |
| Import failed | "Couldn't import your workouts. Try again" above the bottom bar. Nothing is written. Focus moves to Import. |
| File errors | Instead of the count and the choices: "This file isn't a Workout Log export"; "This file is from a newer version of Workout Log. Update the app to import it"; "This file has no workouts"; "This file is too large to import". The bottom bar has **Cancel** and Primary **Choose another file**. |
| Reading another file | Choose another file is busy: "Reading…". Cancel and back still work. Cancelling the picker keeps the error. If the picker or the read fails, "Couldn't import your workouts. Try again" replaces the error. |

Import writes in one transaction, then goes back to Settings with its message. Back and Cancel go back to Settings with no prompt. An open dialog closes on back (D78).

States: [New only](./?path=/story/screens-import-review--new-only) · [With all groups](./?path=/story/screens-import-review--with-all-groups) · [Different](./?path=/story/screens-import-review--different) · [Can't be imported](./?path=/story/screens-import-review--can-t-be-imported) · [Nothing new](./?path=/story/screens-import-review--nothing-new) · [Replace all](./?path=/story/screens-import-review--replace-all) · [Replace confirm](./?path=/story/screens-import-review--replace-confirm) · [Importing](./?path=/story/screens-import-review--importing) · [Import failed](./?path=/story/screens-import-review--import-failed) · [Not an export](./?path=/story/screens-import-review--not-an-export) · [Newer version](./?path=/story/screens-import-review--newer-version) · [No workouts](./?path=/story/screens-import-review--no-workouts) · [Too large](./?path=/story/screens-import-review--too-large) · [Reading another file](./?path=/story/screens-import-review--reading-another-file)

`data-testid`: `back`, `review-file-name`, `review-total`, `review-new`, `review-already`, `review-different`, `review-skipped`, `review-mode`, `review-add`, `review-replace`, `review-different-choice`, `review-keep`, `review-use-file`, `review-skipped-text`, `review-nothing-new`, `review-failed`, `review-error`, `review-cancel`, `review-import`, `review-choose`, `replace-dialog`, `replace-dialog-cancel`, `replace-dialog-confirm`.
