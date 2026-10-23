---
$attribute: CustomPage("Design System/Screens/Error")
---

# Error

Shown instead of the app when a screen throws an unhandled error (D140). View: `ErrorView`, shown by `AppErrorBoundary`.

| Part | Content |
|---|---|
| Top bar | Title "Workout Log". |
| Content | Centred: "Something went wrong." No error details. |
| Bottom bar | **Restart**: goes to the Dashboard. An unsaved workout is lost. |

States: [Rendering error](/?path=/story/screens-error--rendering-error)

Prototype: [Recover from an error](/?path=/story/prototype-workout-log--recover-from-an-error). **Simulate an error** in the phone frame's bottom bar shows this screen from any screen.

`data-testid`: `error-message`, `error-restart`.
