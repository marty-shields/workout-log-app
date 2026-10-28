# 09 — Design System

The design system is the stories site (D94, D95): tokens, components, screens, their behaviour and navigation. `main` is published at https://marty-shields.github.io/workout-log-app/ (D177).

| Section | What | Files |
|---|---|---|
| Foundations | Introduction, Tokens, Navigation, App Icon | `src/WorkoutLog.UI.Stories/Stories/Foundations/` |
| Components | One folder per group: an Overview page, and one stories file per component with one story per state (D97, D96) | `src/WorkoutLog.UI.Stories/Stories/Components/<Group>/` |
| Screens | One folder per screen: an Overview page with its parts, behaviour and `data-testid`s (D95), and one story per state, in the phone frame | `src/WorkoutLog.UI.Stories/Stories/Screens/` |

| Item | Location |
|---|---|
| Tokens | `src/WorkoutLog.UI/wwwroot/css/tokens.css` |
| Components | `src/WorkoutLog.UI/Components/` |
| Screens | `src/WorkoutLog.UI/Screens/` |
| App icon and splash | SVGs in `src/WorkoutLog.App/Resources/` (D88–D93) |
| Component tests, design-system and story coverage checks (bUnit) | `tests/WorkoutLog.UI.Tests/` (D97) |

Component and screen stories have a System / Light / Dark switch (D96). Stories are named by the state they show (D96).

Run the stories: see [06](06-dev-setup-build.md#stories-design-system).

## Messages
Messages (errors, empty states, prompts' text) have no final full stop; a two-sentence message keeps its inner one: "Couldn't export your workouts. Try again" (D189). Titles, labels and button text have none either.

## Process
Every new or changed screen or component goes through these steps (D98):
1. Build it in `WorkoutLog.UI` with its stories and its Overview page.
2. Run the stories and take Playwright screenshots of each state, light and dark, at phone width. Open and check every screenshot: the look and position of what changed, in every story that shows it, including screens that use a changed component (D98).
3. Stop for review: leave the stories running and open each new or changed story and page in the reviewer's browser (D98). Change it until the reviewer approves, then stop the stories.
4. Only then use it in the App or in other screens.
5. If the screen is in the README, retake its [README screenshots](#readme-screenshots) (D181).

## README screenshots
`README.md` shows eight screen stories, light and dark, from `docs/images/readme/` (D181). To retake one:
1. Run the stories ([06](06-dev-setup-build.md#stories-design-system)).
2. With Playwright, set the colour scheme (light, then dark) and open `http://localhost:5056/iframe.html?id=<story id>&viewMode=story`.
3. Screenshot the `.phone` element with a transparent background, to `<file>-light.png` and `<file>-dark.png`.

| File | Story id |
|---|---|
| `dashboard` | `screens-dashboard--with-workouts` |
| `new-workout` | `screens-new-workout--strength` |
| `cardio` | `screens-new-workout--cardio` |
| `add-exercise` | `screens-add-exercise--results` |
| `workouts` | `screens-workouts--with-workouts` |
| `settings` | `screens-settings--with-workouts` |
| `exercise-lift` | `screens-exercise--lift-top-set` |
| `exercise-cardio` | `screens-exercise--cardio-distance` |

`icon.svg` is the app icon's foreground on its background with rounded corners; rebuild it when the icon changes.
