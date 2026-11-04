---
$attribute: CustomPage("Foundations/Introduction")
---

# Workout Log design system

Tokens, components, screens and their behaviour for the Workout Log app.

## Where it lives
| Item | Location |
|---|---|
| Tokens | `src/WorkoutLog.UI/wwwroot/css/tokens.css` (CSS custom properties) |
| Components | `src/WorkoutLog.UI/Components/` (Razor components with scoped `.razor.css`) |
| Screens | `src/WorkoutLog.UI/Screens/` (`*View` components, and `*Page` components that call services) |
| Stories and these pages | `src/WorkoutLog.UI.Stories/Stories/`: `Foundations/`, `Components/<Group>/`, `Screens/` |

## Sections
- **Foundations**: this page, [Tokens](./?path=/story/foundations-tokens--all-tokens), [Navigation](./?path=/custom/foundations-navigation), [Haptics](./?path=/custom/foundations-haptics) and the [App Icon](./?path=/custom/foundations-app-icon-overview).
- **Components**: one folder per group, with an Overview page and one stories file per component. Each story is one visual state.
- **Screens**: one folder per screen, with an Overview page (parts, behaviour, `data-testid`s) and one story per state, in the phone frame.

## Rules
- Light and dark themes. The app follows the phone's setting (`prefers-color-scheme`) until Settings, Theme sets Light or Dark.
- Components use tokens only; no hard-coded colours or sizes. Story-only chrome (phone frame, theme switch) is exempt.
- Components take their data as parameters. Pages call services.
- Numbers use the phone's numeric keyboard: `inputmode="numeric"` for whole numbers, `inputmode="decimal"` for decimals. Duration is three boxes: h, mm, ss.
- Content stays inside the Android 16 safe-area insets.
- Layout suits one-handed use.
- Interactive elements have a `data-testid`.

## Tokens
Defined in `tokens.css`; the [Tokens](./?path=/story/foundations-tokens--all-tokens) story shows each one in light and dark.

### Palette
Five colours. Light uses them directly; dark keeps aqua for primary buttons and uses neutral, low-tint colours for everything else so text, accents and surfaces read apart:

| Colour | Light | Dark |
|---|---|---|
| Dark teal `#305252` | Top bar, primary buttons, text on secondary buttons | — |
| Aqua `#b7d5d4` | Secondary buttons, open set, card borders, top-bar subtitle | Primary buttons |
| Teal `#488286` | Focus, stat numbers | — |
| Charcoal `#373e40` | Text | — |
| Grey `#77878b` | Input borders | — |

Derived from the palette:

| Colour | Light | Dark |
|---|---|---|
| Background | `#f4f8f8` | `#141819` |
| Cards | `#ffffff` | `#212829` |
| Card borders | Aqua `#b7d5d4` | `#364243` |
| Text | Charcoal `#373e40` | `#e4ecec` |
| Muted text and placeholders | `#5e6e72` | `#a3b1b3` |
| Input borders | Grey `#77878b` | `#7f9093` |
| Top bar | Dark teal `#305252` | `#1b2526` |
| Top-bar text and subtitle | `#ffffff`, aqua `#b7d5d4` | `#e4ecec`, `#a3b1b3` |
| Text on primary buttons | `#ffffff` | `#173535` |
| Secondary buttons and open set | Aqua `#b7d5d4` | `#2b4444` |
| Text on secondary buttons | Dark teal `#305252` | `#d4e8e7` |
| Focus and stat numbers | Teal `#488286` | `#8cc4c1` |
| Errors and danger buttons | `#a8231c` | `#f2b8b5` |
| Increases and records | `#2e7d32` | `#a5d6a7` |
| Past weeks in the weekly chart | Grey `#77878b` | `#7d8a8c` |

Text and controls meet WCAG AA in both themes.

### Other tokens
- Most colour tokens are `light-dark()` pairs. `data-theme="light"` or `"dark"` forces a theme on an element; component and screen stories have a System / Light / Dark switch.
- Danger buttons are filled red, the same red as errors.
- System font; body 16px, number and duration inputs 20px; radius 8 / 12 / 20px, plus round; spacing 4–32px.
- Some components override tokens inside them: the top bar sets text, muted text and focus; the Add Exercise bar sets text, muted text and focus; the open set sets muted text, input border and focus to the text colour.

## Checks
Tests in `WorkoutLog.UI.Tests` fail when:
- a component's CSS has a literal colour or size, or uses a token that doesn't exist;
- a token pair is below WCAG AA in light or dark;
- a button or input is smaller than the touch target;
- an interactive element on a screen state has no `data-testid`;
- a component has no stories file or more than one, a `*View` is in no screen's stories, or a stories file shows anything else;
- a screen's stories differ from the screen states listed in the tests;
- a component has no section on its group's Overview, or a `*View` isn't named on a screen's Overview;
- the app icon or splash uses a colour outside the palette and white.
