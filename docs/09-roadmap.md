# 09 — Roadmap

| Milestone | Scope | Exit criteria | Epics |
|---|---|---|---|
| **M0 Setup** | Tooling installed; App, UI and Core projects plus test projects created; local git; empty app runs on the phone | Blank Blazor page shows on the phone over USB | [EP-01](13-backlog.md#ep-01-project-setup-m0) |
| **M1 Core + data** | Domain, metrics, exercise definitions, validation, sqlite-net schema, seeding, repositories, services, `IClock` | Core unit and repository tests pass | [EP-02](13-backlog.md#ep-02-exercises--data-m1) |
| **M1.5 UX & design system** | Stories project, wireframes, design tokens (light + dark), components, final screen designs. Can run alongside M1. | Every screen and state in [06](06-screens-ux.md) shows as a story in light and dark; the Design System pages are complete ([14](14-design-system.md)) | [EP-09](13-backlog.md#ep-09-ux--design-system-m15) |
| **M2 Workout logging** | New Workout screen, live timer, exercise picker, metric-driven strength sets and cardio entry, remove, Save, Discard/Cancel | Can log and save a workout on the phone; component tests pass | [EP-03](13-backlog.md#ep-03-workout-logging-m2) |
| **M3 History** | History list, newest first, empty state. Rows are not tappable yet. | Saved workouts show in History on the phone; tests pass | [EP-04](13-backlog.md#ep-04-history-m3) |
| **M4 Edit workouts** | Tap a row to edit, Discard prompt for unsaved edits, delete with confirm | Can edit and delete saved workouts; tests pass | [EP-05](13-backlog.md#ep-05-edit-workouts-m4) |
| **M5 Dashboard** | Home screen: New Workout, last workout, week/month counts, History link | Counts correct; tests pass | [EP-06](13-backlog.md#ep-06-dashboard-m5) |
| **M6 Tests + release** | Appium flows, manual checklist, signed release APK | All automated tests green, checklist done, APK installed on the phone | [EP-07](13-backlog.md#ep-07-automated-ui-tests-m6), [EP-08](13-backlog.md#ep-08-release-m6) |

After v1: [11 Future ideas](11-future-ideas.md).
