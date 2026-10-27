---
$attribute: CustomPage("Screens/Startup/Overview")
---

# Startup

Shown while the app opens the database, before the Dashboard (D108). View: `StartupView`, shown by `StartupGate`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". |
| Content | Centred. `Spinner` "Loading…" while loading (D75); "Couldn't open the workout database" when `Failed` (D75). |

States: [Loading](./?path=/story/screens-startup--loading) · [Load failed](./?path=/story/screens-startup--load-failed)

`data-testid`: `startup-loading`, `startup-error`.
