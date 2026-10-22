---
$attribute: CustomPage("Design System/Screens/Startup")
---

# Startup

Shown while the app opens the database, before the Dashboard. View: `StartupView`, shown by `StartupGate`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". |
| Content | Centred. `Spinner` "Loading…" while loading; "Couldn't open the workout database." when `Failed`. |

States: [Loading](/?path=/story/screens-startup--loading) · [Error](/?path=/story/screens-startup--error)

`data-testid`: `startup-loading`, `startup-error`.
