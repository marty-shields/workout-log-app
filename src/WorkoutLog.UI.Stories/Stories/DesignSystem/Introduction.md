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
Defined in `tokens.css`; the **Tokens** story shows each one in light and dark.

- Palette: [coolors.co/palette/77878b-305252-373e40-488286-b7d5d4](https://coolors.co/palette/77878b-305252-373e40-488286-b7d5d4), plus a derived background, card border, muted text and red (D110). Placeholders use muted text.
- Most colour tokens are `light-dark()` pairs. `data-theme="light"` or `"dark"` forces a theme on an element; phone-frame stories have a System / Light / Dark switch (D111).
- Danger buttons are filled red, the same red as errors (D112).
- System font; body 16px, number and duration inputs 20px; radius 8 / 12 / 20px; spacing 4–32px (D113).
- Some components override tokens inside them: the top bar sets text, muted text and focus; the picker bar sets focus; the open set sets muted text, input border and focus to the text colour.

## Components
Built on the final tokens (US-31, US-32).

| Group | Components |
|---|---|
| Actions | `AppButton` |
| Inputs | `FieldInput`, `NumberField`, `DurationField` |
| Layout | `AppScreen` |
| Lists | `ListRow`, `StatTile`, `EmptyState` |
| Overlays | `ConfirmDialog` |
| Workout | `ExerciseCard`, `SetRow`, `SetSummary`, `SetPanel`, `EntryErrors`, `ExercisePicker` |

Screens (`DashboardView`, `HistoryView`, `WorkoutView`) are built from these components.
