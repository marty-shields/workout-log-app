# 14 — Design System

The design system is documented in the stories site, under **Design System** (D100). It covers tokens, components, screens and layout rules.

| Item | Location |
|---|---|
| Design-system pages (Markdown) | `src/WorkoutLog.UI.Stories/Stories/DesignSystem/`: Introduction, one page per component group (`Components/`) and per screen (`Screens/`) (D115) |
| Component stories | `src/WorkoutLog.UI.Stories/Stories/Components/`; System / Light / Dark switch from `ThemeLayout`, or from the phone frame for components shown on a screen (D116) |
| Screen stories | `src/WorkoutLog.UI.Stories/Stories/Screens/`: one section per screen, in the phone frame (D114) |
| Tokens | `src/WorkoutLog.UI/wwwroot/css/tokens.css`; shown in the **Design System / Tokens** story |
| Components | `src/WorkoutLog.UI/Components/` |
| Screens | `src/WorkoutLog.UI/Screens/` |
| Component tests and design-system checks (bUnit) | `tests/WorkoutLog.UI.Tests/` (D117) |

Run the stories: see [08](08-dev-setup-build.md#stories-design-system-and-prototypes).
