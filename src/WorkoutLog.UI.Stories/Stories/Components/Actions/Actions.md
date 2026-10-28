---
$attribute: CustomPage("Components/Actions/Overview")
---

# Actions

## `AppButton`
A button with a 48px touch target.

| Variant | Use for |
|---|---|
| Primary | The main action on a screen: New Workout, Save. One per screen. |
| Secondary | Other actions in the bottom bar or a card: Add exercise, Add set, Cancel in a confirm dialog. |
| Danger | Confirming a destructive action: Discard, Delete. Filled red (D85). |

- `FullWidth` fills the row; bottom-bar buttons use it.
- `Disabled` dims the button with `--opacity-disabled`.
- `Busy` shows a turning ring, `--spinner-size-small` across in the text colour, beside the text, sets `aria-busy` and ignores taps. The caller sets the text, e.g. "Exporting…". The ring stands still when the phone asks for reduced motion.
- `data-testid`: `TestId`.

## `Icon`
A Material Symbols Outlined icon (weight 400), drawn as an SVG and hidden from screen readers. `IconButton` uses it.

- `Name`: an `IconName`.
- The icon takes the text colour; `Muted` uses `--color-text-muted`.
- `Size`: `Default` is `--font-size-xl`, `Small` is `--font-size-lg`.
- Chevron: the end of a tappable `ListRow` and carry `SetSummary` row.
- Search: the Exercises search box, `Muted`.

## `IconButton`
A button that shows an `Icon`, with a 48px touch target.

| Icon | Use for |
|---|---|
| Settings | Opening Settings from the Dashboard top bar. |
| Back | The back arrow in the top bar and the exercise picker. |
| Close | Removing a set: `Muted`, `Small`. |
| Delete | Deleting the workout in the Edit Workout top bar; removing an exercise from its card (`Muted`). |

- `AriaLabel` names the button.
- The icon is `--color-text`; on the top bar and the Exercise Picker bar that is `--color-on-bar`. `Muted` uses `--color-text-muted`.
- `Size`: `Default` is `--font-size-xl`, `Small` is `--font-size-lg`.
- `data-testid`: `TestId`.

Stories: [AppButton](./?path=/docs/components-actions-appbutton--docs), [Icon](./?path=/docs/components-actions-icon--docs), [IconButton](./?path=/docs/components-actions-iconbutton--docs)
