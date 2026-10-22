---
$attribute: CustomPage("Design System/Introduction")
---

# Workout Log design system

Tokens, components and layout rules for the Workout Log app.

## Where it lives
| Item | Location |
|---|---|
| Tokens | `src/WorkoutLog.UI/wwwroot/css/tokens.css` (CSS custom properties) |
| Components | `src/WorkoutLog.UI/Components/` (Razor components with scoped `.razor.css`) |
| Stories and these pages | `src/WorkoutLog.UI.Stories/Stories/` |

## Rules
- Light and dark themes. The app follows the phone's setting (`prefers-color-scheme`) (D97).
- Components use tokens only; no hard-coded colours or sizes. Story-only chrome (phone frame, scenario player) is exempt.
- Components take their data as parameters. Pages call services.
- Numbers use the phone's numeric keyboard: `inputmode="numeric"` for whole numbers, `inputmode="decimal"` for decimals. Duration is three boxes: h, mm, ss (D98).
- Content stays inside the Android 16 safe-area insets.
- Layout suits one-handed use.
- Interactive elements have a `data-testid`.

## Tokens
To be defined in US-31.

## Components
Wireframe versions on greyscale tokens (US-30). US-32 finishes them on the final tokens.

| Group | Components |
|---|---|
| Actions | `AppButton` |
| Inputs | `FieldInput`, `NumberField`, `DurationField` |
| Layout | `AppScreen` |
| Lists | `ListRow`, `StatTile`, `EmptyState` |
| Overlays | `ConfirmDialog` |
| Workout | `ExerciseCard`, `SetRow`, `SetSummary`, `SetPanel`, `EntryErrors`, `ExercisePicker` |

Screens (`DashboardView`, `HistoryView`, `WorkoutView`) are built from these components.
