---
$attribute: CustomPage(StoryTitles.Prefix + "/Foundations/Introduction")
---

# Workout Log design system

Screens and their behaviour for the Workout Log app.

**This repo holds no custom UI elements.** Every general component, style and token comes from the shared kit, [app-ui-kit](https://github.com/marty-shields/app-ui-kit). The app holds screens and the few app-only components on the allow-list (`AppComponentAllowListTests`). A missing one is requested in the kit, not built here. To try a kit change first, build with `-p:UseKitSource=true -p:KitPath=<path of your app-ui-kit clone>`.

## Where it lives
| Item | Location |
|---|---|
| Tokens | The Shields.UI kit's `css/tokens.css` (with its base styles and theme script). The app declares none |
| Components | The general ones come from the Shields.UI kit and are documented in the kit's stories. `src/WorkoutLog.UI/Components/` holds the app-only ones (Razor components whose scoped `.razor.css` only lays out) |
| Screens | `src/WorkoutLog.UI/Screens/` (`*View` components, and `*Page` components that call services) |
| Stories and these pages | `src/WorkoutLog.UI.Stories/Stories/`: `Foundations/`, `Components/<Group>/`, `Screens/` |

## Sections
- **Foundations**: this page, [Navigation](./?path=/custom/workout-log-foundations-navigation), [Haptics](./?path=/custom/workout-log-foundations-haptics) and the [App Icon](./?path=/custom/workout-log-foundations-app-icon-overview).
- **Components**: one folder per group, with an Overview page and one stories file per component. Each story is one visual state.
- **Screens**: one folder per screen, with an Overview page (parts, behaviour, `data-testid`s) and one story per state, in the phone frame.

## Rules
- Light and dark themes. The app follows the phone's setting (`prefers-color-scheme`) until Settings, Theme sets Light or Dark.
- Components use kit tokens only; no hard-coded colours or sizes, no custom properties of our own, and app CSS only lays out (`LayoutOnlyCssTests`). Story-only chrome (phone frame, theme switch) is exempt.
- Components take their data as parameters. Pages call services.
- Numbers use the phone's numeric keyboard: `inputmode="numeric"` for whole numbers, `inputmode="decimal"` for decimals. Duration is three boxes: h, mm, ss.
- Content stays inside the Android 16 safe-area insets: `--inset-top` and `--inset-bottom`. The app sets them to the space the status bar, camera cutout and navigation bar cover; elsewhere they are `env(safe-area-inset-*)`, and the phone frame sets 32px.
- Layout suits one-handed use.
- Interactive elements have a `data-testid`.

## Tokens
Defined in the kit's `tokens.css`; this app has none.

### Palette
Five colours. Light uses them directly; dark keeps aqua for primary buttons and uses neutral, low-tint colours for everything else so text, accents and surfaces read apart:

| Colour | Light | Dark |
|---|---|---|
| Dark teal `#305252` | Top bar, primary buttons, text on secondary buttons | — |
| Aqua `#b7d5d4` | Secondary buttons, card borders, top-bar subtitle | Primary buttons |
| Teal `#488286` | Focus | — |
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
| Secondary buttons and selected | Aqua `#b7d5d4` | `#2b4444` |
| Text on secondary buttons | Dark teal `#305252` | `#d4e8e7` |
| Focus | Teal `#488286` | `#8cc4c1` |
| Accent: stat numbers, current-week bars, meter fills | `#29494c` | `#acd5d2` |
| Errors | `#841c16` | `#f4c1be` |
| Danger buttons | `#a8231c` | `#f2b8b5` |
| Increases and records | `#2e7d32` | `#a5d6a7` |
| Past weeks in the weekly chart (`--color-chart-past`) | `#8a9092` | `#6c7274` |

Text and controls meet WCAG AA in both themes.

### Other tokens
- Most colour tokens are `light-dark()` pairs. `data-theme="light"` or `"dark"` forces a theme on an element; component and screen stories have a System / Light / Dark switch.
- Danger buttons are filled red (`#a8231c` light, `#f2b8b5` dark), a slightly different red from the error text (`#841c16`, `#f4c1be`): lighter in light theme, darker in dark.
- System font; body 16px, number and duration inputs 20px; radius 8 / 12 / 20px, plus round; spacing 4–32px.
- Kit components override tokens inside them where they need to, such as the top bar's text, muted text and focus. The app overrides none.

## Checks
Tests in `WorkoutLog.UI.Tests` fail when:
- a component's CSS has a literal colour or size, uses a token that doesn't exist, declares a custom property, or sets anything but layout;
- a `.razor` file or `ComponentBase` class is not on the allow-list;
- a token pair is below WCAG AA in light or dark;
- a button or input is smaller than the touch target;
- an interactive element on a screen state has no `data-testid`;
- a component has no stories file or more than one, a `*View` is in no screen's stories, or a stories file shows anything else;
- a screen's stories differ from the screen states listed in the tests;
- a component has no section on its group's Overview, or a `*View` isn't named on a screen's Overview;
- the app icon or splash uses a colour outside the palette and white.
