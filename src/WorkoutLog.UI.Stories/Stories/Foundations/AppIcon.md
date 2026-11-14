---
$attribute: CustomPage("Foundations/App Icon/Overview")
---

# App icon and splash

The Volt dumbbell: one flat dumbbell with slots cut in its big plates, tilted, on the design system's deep teal.

| Item | Rule |
|---|---|
| Style | A dumbbell (bar, two big plates, two end caps) tilted -40° and scaled to 0.78, with a slot cut in each big plate (one `nonzero` path, slots wound the other way), and an offset drop shadow 2.2 units down |
| Colours | Only the colours in `AppIcon/palette.json`; a test checks the SVGs and the project's `Color` and `DarkColor` values against it. The dumbbell is a vertical gradient from `#d9ff4d` through `#6fe38a` to `#1fb7c9` |
| Canvas | 108-unit adaptive-icon canvas. The dumbbell and its shadow stay inside Android's 66-unit safe circle (33 units from the centre), so a circular launcher mask does not touch it |
| Background | Flat `#173535`, the project's `MauiIcon` colour. No glow behind the dumbbell |
| Themed icon | Android 13+ tints the monochrome layer as one shape: the dumbbell in white with the plate slots as holes, and no shadow |
| Splash | The dumbbell alone, no wordmark. Light theme `#f4f8f8` (the app's light page) with a deeper gradient (`#8fcf10`, `#27b878`, `#14899f`, because the bright top would fade into the page) and a `#0e3a3a` shadow at 0.32; dark theme `#173535` with the icon's own gradient and shadow |
| Builds | Debug and Release use the same art |
| SVG features | The build rasterises the SVGs, so they use only plain paths (lines and circular arcs), solid fills, `linearGradient`, `fill-opacity` and `clipPath`. No `mask`, `radialGradient`, `filter` or `transform` |

## Files
In `src/WorkoutLog.App/`; the stories project embeds the SVGs for these stories:

| File | Use |
|---|---|
| `Resources/AppIcon/appicon.svg` | Icon background |
| `Resources/AppIcon/appiconfg.svg` | Icon foreground, and the dark splash (the splash's `DarkFile`) |
| `Resources/AppIcon/appicon_monochrome.svg` | Themed icon layer (the icon's `MonochromeFile`) |
| `Resources/AppIcon/palette.json` | Every colour the art may use |
| `Resources/Splash/splash.svg` | Light splash |

## Changing the art
The SVGs, and the README's `docs/images/readme/icon.svg`, are generated. Edit the dumbbell parts, the tilt, the scale or the shadow in `.github/scripts/app-icon.mjs` and colours in `palette.json`, then run `node .github/scripts/app-icon.mjs`. It overwrites every file, and rerunning it changes nothing.

Story: [Icon and splash](./?path=/story/foundations-app-icon--icon-and-splash)
