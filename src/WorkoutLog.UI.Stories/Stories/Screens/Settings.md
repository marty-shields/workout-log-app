---
$attribute: CustomPage("Screens/Settings/Overview")
---

# Settings

Opened from the Dashboard gear. View: `SettingsView`; page: `SettingsPage`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Settings". |
| Content | Heading "Appearance" and a **Theme** segmented control, System, Light and Dark (System selected by default); then heading "Your data", the export text, and a Secondary full-width **Export**; then "Import workouts from a JSON file exported by Workout Log" and a Secondary full-width **Import**, enabled with or without workouts. |
| Export text | "Export all 42 workouts to a JSON file", grouped from 1,000 ("Export all 1,042 workouts…"); "Export 1 workout to a JSON file"; "No workouts to export", with Export disabled. |
| Version | "Version 1.4.2": muted, centred and pinned to the bottom of the screen; also in Loading and Load failed. Nothing shows when there is no version. CI and local E2E builds show "Version 1.0.0-e2e.<run id>" or "1.0.0-e2e.<epoch>". |
| Theme | Tapping a segment applies it at once and saves it on the phone. **System** follows the phone's light or dark setting and is the default. **Light** and **Dark** override it, and are kept across app starts. The saved choice is applied before the first screen shows, so the app never flashes the wrong theme. The control shows while the count loads and when it fails to load. If the saved choice can't be read, System shows; if the phone fails to apply a tapped choice, the previous one stays selected; if it applies but can't be saved, the new one stays selected until the app closes, then System or the earlier saved choice returns. |
| Loading | `Spinner` "Loading…", centred under Appearance, until the count loads. Your data shows after it. |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reloads, under Appearance. Focus moves to Try again. |
| Exporting | Export is busy: a spinner beside "Exporting…". Taps are ignored. |
| Export failed | "Couldn't export your workouts. Try again" under Export, also when the share sheet can't open. Focus moves to Export. |
| Too many workouts | "Too many workouts to export" under Export: the file would be over 5 MB. |
| Reading | Import is busy: a spinner beside "Reading…" while the picked file is read and checked. Export is disabled; back still works. Exporting disables Import. |
| Import failed | "Couldn't import your workouts. Try again" under Import, when the picker fails or the phone's workouts can't be read. Focus moves to Import. |
| Imported | After an import, under Import: "Added 3 workouts, replaced 2, skipped 1". Replaced and skipped are left out at 0 ("Added 1 workout"); Replace all never says replaced. |

The count loads each time Settings shows and when the app starts again while it shows. A load while one runs, or while exporting, is ignored.

Export writes the file, then opens Android's share sheet, titled "Export workouts". Nothing shows afterwards.

Import opens Android's file picker (JSON files). Cancelling it leaves Settings as it was. A file that's read opens [Import Review](./?path=/custom/screens-import-review-overview), also when it can't be imported.

The messages under Export and Import clear on the next Export or Import tap and on any reload.

Back: Dashboard. Import: Import Review.

States: [With workouts](./?path=/story/screens-settings--with-workouts) · [Dark chosen](./?path=/story/screens-settings--dark-chosen) · [One workout](./?path=/story/screens-settings--one-workout) · [No workouts](./?path=/story/screens-settings--no-workouts) · [Loading](./?path=/story/screens-settings--loading) · [Load failed](./?path=/story/screens-settings--load-failed) · [Exporting](./?path=/story/screens-settings--exporting) · [Export failed](./?path=/story/screens-settings--export-failed) · [Too many workouts](./?path=/story/screens-settings--too-many-workouts) · [Reading](./?path=/story/screens-settings--reading) · [Import failed](./?path=/story/screens-settings--import-failed) · [Imported](./?path=/story/screens-settings--imported)

`data-testid`: `back`, `theme`, `settings-theme-system`, `settings-theme-light`, `settings-theme-dark`, `export-text`, `export`, `export-error`, `import-text`, `import`, `import-error`, `import-done`, `settings-loading`, `settings-error`, `settings-retry`, `app-version`.
