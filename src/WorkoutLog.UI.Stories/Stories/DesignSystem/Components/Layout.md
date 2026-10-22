---
$attribute: CustomPage("Design System/Components/Layout")
---

# Layout

## `AppScreen`
The frame of every screen:

| Part | Content |
|---|---|
| Top bar | Back arrow (when `OnBack` is set), title, subtitle, `Actions` on the right. Dark teal bar; sets text, muted text and focus to the on-bar colours. |
| Content | Scrolls. 16px padding. |
| Bottom bar | `BottomBar` actions, in thumb reach. Optional. |

- Top and bottom padding include the safe-area insets (`--inset-top`, `--inset-bottom`).
- `data-testid`: `back`, `screen-title`, `screen-subtitle`, `bottom-bar`.

Stories: [AppScreen](/?path=/docs/components-layout-appscreen--docs)
