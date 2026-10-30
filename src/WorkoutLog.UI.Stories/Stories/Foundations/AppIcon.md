---
$attribute: CustomPage("Foundations/App Icon/Overview")
---

# App icon and splash

A pixel-art notebook: a dumbbell header and ruled lines cut out of the page, with a spiral binding.

| Item | Rule |
|---|---|
| Style | 32 × 32 pixel sprite, 1px charcoal outline, light from the top left |
| Colours | The five palette colours and white only; a test checks the SVGs and the project's `Color` values against `tokens.css` |
| Canvas | 108-unit adaptive-icon canvas, 2 units per sprite pixel from 22, so each pixel is a whole 2–8 device pixels from mdpi to xxxhdpi. The art stays inside Android's 66dp safe circle |
| Background | Dark teal `#305252` (`--color-bar`) |
| Themed icon | Android tints the foreground's shape, so details are cut-outs, not shades |
| Splash | The same art on `#305252`, on the same 108-unit canvas |
| Builds | Debug and Release use the same art |

## Files
In `src/WorkoutLog.App/Resources/`; the stories project embeds them for these stories:

| File | Use |
|---|---|
| `AppIcon/appicon.svg` | Icon background |
| `AppIcon/appiconfg.svg` | Icon foreground |
| `Splash/splash.svg` | Splash |

Story: [Icon and splash](./?path=/story/foundations-app-icon--icon-and-splash)
