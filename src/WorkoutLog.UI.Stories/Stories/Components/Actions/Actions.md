---
$attribute: CustomPage("Components/Actions/Overview")
---

# Actions

## `AppButton`
A button with a 48px touch target.

| Variant | Use for |
|---|---|
| Primary | The main action on a screen: New Workout, Save. One per screen. |
| Secondary | Other actions in the bottom bar or a card: Add exercise, Add set, Cancel in a confirm dialog. |
| Danger | Confirming a destructive action: Discard, Delete. Filled red (D112). |
| Text | Low-emphasis actions: Remove, Delete in the top bar. |

- `FullWidth` fills the row; bottom-bar buttons use it.
- `Disabled` dims the button with `--opacity-disabled`.
- `data-testid`: `TestId`.

Stories: [AppButton](./?path=/docs/components-actions-appbutton--docs)
