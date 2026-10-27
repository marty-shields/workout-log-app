---
$attribute: CustomPage("Components/Layout/Overview")
---

# Layout

## `AppScreen`
The frame of every screen:

| Part | Content |
|---|---|
| Top bar | Back arrow `IconButton` (when `OnBack` is set; Android back raises `OnBack` too), title, subtitle, `Actions` on the right. Dark teal bar; sets text, muted text and focus to the on-bar colours. |
| Content | Scrolls. 16px padding. |
| Bottom bar | `BottomBar` actions, in thumb reach. Optional. |

- Top and bottom padding include the safe-area insets (`--inset-top`, `--inset-bottom`).
- `data-testid`: `back`, `screen-title`, `screen-subtitle`, `bottom-bar`.

Stories: [AppScreen](./?path=/docs/components-layout-appscreen--docs)

## `StartupGate`
Runs `Initialize` once at startup. Shows the [Startup](./?path=/custom/screens-startup-overview) screen while it runs or if it throws, then `ChildContent`.

Stories: [StartupGate](./?path=/docs/components-layout-startupgate--docs)

## `AppErrorBoundary`
Shows `ChildContent` until it throws an unhandled error, then logs the error and shows the [Error](./?path=/custom/screens-error-overview) screen. **Restart** raises `OnRestart`, then shows `ChildContent` again.

Stories: [AppErrorBoundary](./?path=/docs/components-layout-apperrorboundary--docs)
