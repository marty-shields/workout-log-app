---
$attribute: CustomPage("Screens/Error/Overview")
---

# Error

Shown instead of any screen that hits an unhandled error. View: `ErrorView`, shown by `AppErrorBoundary`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". |
| Content | Centred: "Something went wrong", with no error details. |
| Bottom bar | **Restart**: goes to the Dashboard. An unsaved workout is lost. |

States: [Error](./?path=/story/screens-error--error)

`data-testid`: `error-message`, `error-restart`.
