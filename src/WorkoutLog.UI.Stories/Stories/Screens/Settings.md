---
$attribute: CustomPage("Screens/Settings/Overview")
---

# Settings

Opened from the Dashboard gear (FR-36, FR-50). View: `SettingsView`; page: `SettingsPage`.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Settings". |
| Content | Heading "Your data", the export text, and a Secondary full-width **Export** (FR-51). |
| Export text | "Export all 42 workouts to a JSON file", grouped from 1,000 ("Export all 1,042 workouts…"); "Export 1 workout to a JSON file"; "No workouts to export", with Export disabled. |
| Loading | `Spinner` "Loading…", centred on the screen, until the count loads. The heading, text and Export show after it. |
| Load failed | Error "Couldn't load your workouts" and a Secondary **Try again** that reloads. Focus moves to Try again. |
| Exporting | Export is busy: a spinner beside "Exporting…". Taps are ignored. |
| Export failed | "Couldn't export your workouts. Try again" under Export, also when the share sheet can't open. Focus moves to Export. |
| Too many workouts | "Too many workouts to export" under Export: the file would be over 5 MB. |

The count loads each time Settings shows and when the app starts again while it shows. A load while one runs, or while exporting, is ignored (D67).

Export writes the file, then opens Android's share sheet, titled "Export workouts". Nothing shows afterwards. A message under Export clears on the next Export tap and on any reload.

Back: Dashboard.

States: [With workouts](./?path=/story/screens-settings--with-workouts) · [One workout](./?path=/story/screens-settings--one-workout) · [No workouts](./?path=/story/screens-settings--no-workouts) · [Loading](./?path=/story/screens-settings--loading) · [Load failed](./?path=/story/screens-settings--load-failed) · [Exporting](./?path=/story/screens-settings--exporting) · [Export failed](./?path=/story/screens-settings--export-failed) · [Too many workouts](./?path=/story/screens-settings--too-many-workouts)

`data-testid`: `back`, `export-text`, `export`, `export-error`, `settings-loading`, `settings-error`, `settings-retry`.
