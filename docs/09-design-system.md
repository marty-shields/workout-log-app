# 09 — Design System

The design system is the stories site (D100, D239): tokens, components, screens, their behaviour and navigation. `main` is published at https://marty-shields.github.io/workout-log-app/ (D246).

| Section | What | Files |
|---|---|---|
| Foundations | Introduction, Tokens, Navigation, App Icon | `src/WorkoutLog.UI.Stories/Stories/Foundations/` |
| Components | One folder per group: an Overview page, and one stories file per component with one story per state (D244, D245) | `src/WorkoutLog.UI.Stories/Stories/Components/<Group>/` |
| Screens | One folder per screen: an Overview page with its parts, behaviour and `data-testid`s (D243), and one story per state, in the phone frame | `src/WorkoutLog.UI.Stories/Stories/Screens/` |

| Item | Location |
|---|---|
| Tokens | `src/WorkoutLog.UI/wwwroot/css/tokens.css` |
| Components | `src/WorkoutLog.UI/Components/` |
| Screens | `src/WorkoutLog.UI/Screens/` |
| App icon and splash | SVGs in `src/WorkoutLog.App/Resources/` (D209–D214) |
| Component tests, design-system and story coverage checks (bUnit) | `tests/WorkoutLog.UI.Tests/` (D117, D244) |

Component and screen stories have a System / Light / Dark switch (D116). Stories are named by the state they show (D242).

Run the stories: see [06](06-dev-setup-build.md#stories-design-system).

## Process
Every new or changed screen or component goes through these steps (D131):
1. Build it in `WorkoutLog.UI` with its stories and its Overview page.
2. Run the stories and take Playwright screenshots of each state, light and dark, at phone width.
3. Stop for review: leave the stories running and open each new or changed story and page in the reviewer's browser (D193). Change it until the reviewer approves, then stop the stories.
4. Only then use it in the App or in other screens.
5. If the screen is in the README, retake its [README screenshots](#readme-screenshots) (D222).

## README screenshots
`README.md` shows five screen stories, light and dark, from `docs/images/readme/` (D221). To retake one:
1. Run the stories ([06](06-dev-setup-build.md#stories-design-system)).
2. With Playwright, set the colour scheme (light, then dark) and open `http://localhost:5056/iframe.html?id=<story id>&viewMode=story`.
3. Screenshot the `.phone` element with a transparent background, to `<file>-light.png` and `<file>-dark.png`.

| File | Story id |
|---|---|
| `dashboard` | `screens-dashboard--with-workouts` |
| `new-workout` | `screens-new-workout--strength` |
| `cardio` | `screens-new-workout--cardio` |
| `exercise-picker` | `screens-exercise-picker--results` |
| `history` | `screens-history--with-workouts` |

`icon.svg` is the app icon's foreground on its background with rounded corners; rebuild it when the icon changes.
