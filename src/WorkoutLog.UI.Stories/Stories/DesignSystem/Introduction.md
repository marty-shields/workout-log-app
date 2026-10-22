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
- Components use tokens only; no hard-coded colours or sizes. Story-only chrome (phone frame, theme switch, scenario player) is exempt.
- Components take their data as parameters. Pages call services.
- Numbers use the phone's numeric keyboard: `inputmode="numeric"` for whole numbers, `inputmode="decimal"` for decimals. Duration is three boxes: h, mm, ss (D98).
- Content stays inside the Android 16 safe-area insets.
- Layout suits one-handed use.
- Interactive elements have a `data-testid`.

## Tokens
Defined in `tokens.css`; the **Tokens** story shows each one in light and dark.

### Palette
Five colours, each with a role in light and dark (D110):

| Colour | Light | Dark |
|---|---|---|
| Dark teal `#305252` | Top bar, primary buttons, text on secondary buttons | Top bar, secondary buttons, open set, text on primary buttons |
| Aqua `#b7d5d4` | Secondary buttons, open set, card borders, top-bar subtitle | Text (also on the top bar), primary buttons, focus, stat numbers |
| Teal `#488286` | Focus, stat numbers | — |
| Charcoal `#373e40` | Text | Cards |
| Grey `#77878b` | Input borders | — |

Derived from the palette:

| Colour | Light | Dark |
|---|---|---|
| Background | `#f4f8f8` | `#1e2324` |
| Cards | `#ffffff` | Charcoal `#373e40` |
| Card borders | Aqua `#b7d5d4` | `#4a5558` |
| Muted text and placeholders | `#5e6e72` | `#9aaaad` |
| Input borders | Grey `#77878b` | `#9aaaad` |
| Errors and danger buttons | `#a8231c` | `#f2b8b5` |

Text and controls meet WCAG AA in both themes.

### Other tokens
- Most colour tokens are `light-dark()` pairs. `data-theme="light"` or `"dark"` forces a theme on an element; phone-frame stories have a System / Light / Dark switch (D111).
- Danger buttons are filled red, the same red as errors (D112).
- System font; body 16px, number and duration inputs 20px; radius 8 / 12 / 20px; spacing 4–32px (D113).
- Some components override tokens inside them: the top bar sets text, muted text and focus; the picker bar sets focus; the open set sets muted text, input border and focus to the text colour.

## Components
One page per group: [Actions](/?path=/custom/design-system-components-actions) · [Inputs](/?path=/custom/design-system-components-inputs) · [Layout](/?path=/custom/design-system-components-layout) · [Lists](/?path=/custom/design-system-components-lists) · [Overlays](/?path=/custom/design-system-components-overlays) · [Workout](/?path=/custom/design-system-components-workout)

Component stories have a System / Light / Dark switch.

## Screens
Screens are built from the components only. One page per screen, each with its states as stories in the phone frame: [Dashboard](/?path=/custom/design-system-screens-dashboard) · [New Workout](/?path=/custom/design-system-screens-new-workout) · [Edit Workout](/?path=/custom/design-system-screens-edit-workout) · [Exercise Picker](/?path=/custom/design-system-screens-exercise-picker) · [History](/?path=/custom/design-system-screens-history)

## Checks
Tests in `WorkoutLog.UI.Tests` fail when:
- a component's CSS has a literal colour or size, or uses a token that doesn't exist;
- a token pair is below WCAG AA in light or dark;
- a button or input is smaller than the touch target;
- an interactive element has no `data-testid`;
- a screen's stories differ from the screen states listed in the tests;
- a component or screen has no page here.
