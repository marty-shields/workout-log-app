---
$attribute: CustomPage("Screens/Startup/Overview")
---

# Startup

Shown while the app opens the database, before the Dashboard (T36). View: `StartupView`, shown by `StartupGate`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". |
| Content | Centred. `Spinner` "Loading…" while loading (D134); "Couldn't open the workout database." when `Failed` (D133). |

States: [Loading](./?path=/story/screens-startup--loading) · [Load failed](./?path=/story/screens-startup--load-failed)

`data-testid`: `startup-loading`, `startup-error`.
