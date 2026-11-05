---
$attribute: CustomPage("Foundations/App Icon/Overview")
---

# App icon and splash

A pixel-art notebook: a white dumbbell on a teal header band, ruled lines, gold spiral rings and a coral ribbon, with a near-black teal outline and a drop shadow.

| Item | Rule |
|---|---|
| Style | 32 × 32 pixel sprite, 1px outline `#0a2a2b`, shadow one pixel down and right |
| Colours | Only the colours in `AppIcon/palette.json`; a test checks the SVGs, the project's `Color` values and the dark splash colour against it |
| Canvas | 108-unit adaptive-icon canvas, 2 units per sprite pixel from 22, so each pixel is a whole 2–8 device pixels from mdpi to xxxhdpi. Only two shadow pixels under the ribbon reach past Android's 66dp safe circle, by about 1 unit |
| Background | A dithered deep-teal gradient (4 × 4 Bayer, six steps from `#1f4645` to `#0e2726`) over the 72 units a launcher shows; its average is about `#173535`, the project's `MauiIcon` colour |
| Themed icon | Android 13+ tints the monochrome layer as one shape, so the header band, its highlight and the ruled lines are cut out and the shadow is left off |
| Splash | The icon art, no wordmark. Light theme `#f4f8f8` (the app's light page) with a `#c9d9d6` shadow; dark theme `#173535` with the icon's own shadow |
| Builds | Debug and Release use the same art |

## Files
In `src/WorkoutLog.App/`; the stories project embeds the SVGs for these stories:

| File | Use |
|---|---|
| `Resources/AppIcon/appicon.svg` | Icon background |
| `Resources/AppIcon/appiconfg.svg` | Icon foreground, and the dark splash |
| `Resources/AppIcon/appicon_monochrome.svg` | Themed icon layer (a `MauiImage`) |
| `Resources/AppIcon/palette.json` | Every colour the art may use |
| `Resources/Splash/splash.svg` | Light splash |
| `Platforms/Android/Resources/mipmap-anydpi-v33/` | Adaptive icon with the monochrome layer; MAUI's own uses the foreground |
| `Platforms/Android/Resources/values-night/colors.xml` | Dark splash colour (`maui_splash_color`) |
| `Platforms/Android/Resources/drawable-night-v31/` | Dark splash image, the icon's foreground |

## Changing the art
The SVGs, and the README's `docs/images/readme/icon.svg`, are generated. Edit the sprite or the colour roles in `.github/scripts/app-icon.mjs` and colours in `palette.json`, then run `node .github/scripts/app-icon.mjs`. It overwrites every file, and rerunning it changes nothing.

Story: [Icon and splash](./?path=/story/foundations-app-icon--icon-and-splash)
