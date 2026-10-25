# 12 — Design System

The design system is documented in the stories site, under **Design System** (D100). It covers tokens, components, screens and layout rules.

| Item | Location |
|---|---|
| Design-system pages (Markdown) | `src/WorkoutLog.UI.Stories/Stories/DesignSystem/`: Introduction, one page per component group (`Components/`) and per screen (`Screens/`) (D115) |
| Component stories | `src/WorkoutLog.UI.Stories/Stories/Components/`; System / Light / Dark switch from `ThemeLayout`, or from the phone frame for components shown on a screen (D116) |
| Screen stories | `src/WorkoutLog.UI.Stories/Stories/Screens/`: one section per screen, in the phone frame (D114) |
| Tokens | `src/WorkoutLog.UI/wwwroot/css/tokens.css`; shown in the **Design System / Tokens** story |
| Components | `src/WorkoutLog.UI/Components/` |
| Screens | `src/WorkoutLog.UI/Screens/` |
| App icon and splash | SVGs in `src/WorkoutLog.App/Resources/`; the **Design System / App Icon** page and stories (D209–D214) |
| Component tests and design-system checks (bUnit) | `tests/WorkoutLog.UI.Tests/` (D117) |

Run the stories: see [08](08-dev-setup-build.md#stories-design-system-and-prototypes).

## Process
Every new or changed screen or component goes through these steps (D131):
1. Build it in `WorkoutLog.UI` with its story and its design-system page.
2. Run the stories and take Playwright screenshots of each state, light and dark, at phone width.
3. Stop for review: leave the stories running and open each new or changed story and design-system page in the reviewer's browser (D193). Change it until the reviewer approves, then stop the stories.
4. Only then use it in the App or in other screens.
5. If the screen is in the README, retake its [README screenshots](#readme-screenshots) (D222).

## README screenshots
`README.md` shows five screen stories, light and dark, from `docs/images/readme/` (D221). To retake one:
1. Run the stories ([08](08-dev-setup-build.md#stories-design-system-and-prototypes)).
2. With Playwright, set the colour scheme (light, then dark) and open `http://localhost:5056/iframe.html?id=<story id>&viewMode=story`.
3. Screenshot the `.phone` element with a transparent background, to `<file>-light.png` and `<file>-dark.png`.

| File | Story id |
|---|---|
| `dashboard` | `screens-dashboard--with-workouts` |
| `new-workout` | `screens-new-workout--strength-sets` |
| `cardio` | `screens-new-workout--cardio-entries` |
| `exercise-picker` | `screens-exercise-picker--results` |
| `history` | `screens-history--with-workouts` |

`icon.svg` is the app icon's foreground on its background with rounded corners; rebuild it when the icon changes.
