---
$attribute: CustomPage("Components/Overlays/Overview")
---

# Overlays

## `ConfirmDialog`
A modal question at the bottom of the screen, over a scrim, in thumb reach (D119). Cancel (secondary) and the confirm button share the row, full width. Focus moves to Cancel when it opens (D109). Android back does what Cancel does (D144). `Danger` makes the confirm button red.

| Use | Title | Confirm |
|---|---|---|
| Back on a new workout | Discard this workout? | Discard |
| Back on an edited workout with changes | Discard changes? | Discard |
| Delete on an edited workout | Delete this workout? | Delete |

- `data-testid`: `TestId` (default `confirm-dialog`), `{TestId}-cancel`, `{TestId}-confirm`.

Stories: [ConfirmDialog](./?path=/docs/components-overlays-confirmdialog--docs)
