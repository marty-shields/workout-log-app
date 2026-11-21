---
$attribute: CustomPage("Components/Layout/Overview")
---

# Layout

## `AppScreen`
The frame of every screen:

| Part | Content |
|---|---|
| Top bar | Back arrow `IconButton` (when `OnBack` is set; Android back raises `OnBack` too), title, subtitle, `Actions` on the right. Dark teal bar; sets text, muted text and focus to the on-bar colours. |
| Content | Scrolls. 16px padding. |
| Bottom bar | `BottomBar` actions, in thumb reach. Optional. Buttons are full width: the one main action (Save) is a primary `AppButton`, one per screen; others (Add exercise, Add set) are secondary. |
| Tab bar | `MainTabBar` under the bottom bar, when `Tab` is set. |
| Status | `Status`, a `Snackbar` just above the bottom bar (or the tab bar, clear of the raised New button), over the content. Its slot is a polite live region (`role="status"`), there whenever `Status` is set, so a snackbar shown later is read out. While a snackbar shows, the content gets room at its foot (`2 × --touch-target` more) so its last controls scroll clear of it. Needs a bottom bar or a tab bar. Optional. |

- `Tab` makes the screen top-level: it shows the `MainTabBar` with that tab current, raises `OnNavigate` with the tab tapped and `OnNew` for the raised New button. A top-level screen sets no `OnBack`, so it has no back arrow. Android back raises `OnNavigate` with This week (`MainTabs.Start`), except on This week itself, where it leaves the app.
- Top padding includes the safe-area inset (`--inset-top`). The bottom inset (`--inset-bottom`) goes under the last thing on screen: the tab bar, else the bottom bar, else the content.
- `OnRefresh` makes the content pull-to-refresh. Pulled down from the top, a ring like the `Spinner`'s follows the finger at half its travel, coming out from under the top bar. Let go, it goes back up over `--pull-refresh-return-duration`, and if it was past `--pull-refresh-threshold`, `OnRefresh` runs. The ring shows only while pulling, so the screen shows its own loading state, as on first open: each top-level screen shows its `Spinner` "Loading…" in place of the content. A pull during a refresh does nothing. The [Pulling](./?path=/story/components-layout-appscreen--pulling) story shows the ring mid-pull. With reduced motion the ring doesn't slide back. The content doesn't overscroll, so Android's stretch doesn't fight the pull. No gesture without `OnRefresh`.
- `data-testid`: `back` (or `BackTestId`, for a screen layered over another), `screen-title`, `screen-subtitle`, `bottom-bar`, `pull-refresh` (with `OnRefresh`), plus `MainTabBar`'s.

Stories: [AppScreen](./?path=/docs/components-layout-appscreen--docs)

## `MainTabBar`
The bar of the app's two top-level screens, [This week](./?path=/custom/screens-this-week-overview) and [Trends](./?path=/custom/screens-trends-overview), shown by `AppScreen` when its `Tab` is set. The `MainTab` enum lists its tabs.

| Part | Content |
|---|---|
| This week | Calendar icon over "This week", a third of the width, at least 48px tall. |
| New | A raised square button in `--color-primary` with the Add (+) icon, `--nav-new-size` across and lifted `--nav-new-raise` above the bar's top edge, with "New" under it level with the other labels. Never the current page. |
| Trends | Bar-chart icon over "Trends". |

- Current tab (`Current`): a `--color-selected` pill behind the icon, the label and icon in the text colour and bold, `aria-current="page"`. The pill and bold mean it is not shown by colour alone.
- Tapping a tab raises `OnNavigate` with it; tapping the current tab does nothing. New raises `OnNew`.
- A `<nav>` labelled "Main"; each item is a button. A pressed tab shows the shared state layer on its pill, New on its button. Its bottom padding is the safe-area inset.
- `data-testid`: `tab-bar`, `this-week-link`, `new-link`, `trends-link`.

Stories: [MainTabBar](./?path=/docs/components-layout-maintabbar--docs)

The start-up gate, the error boundary and the confirm dialog come from the kit (`StartupGate`, `AppErrorBoundary`, `ConfirmDialog`). Their stories are in the Shields.UI kit's story book, under Components/Layout (`StartupGate`) and Components/Overlays (`ConfirmDialog`).

## Startup and error
- `Routes.razor` wraps the router in `StartupGate` and `AppErrorBoundary`, both titled "Workout Log". `ActivityBridge` comes first, before the gate.
- Start-up deletes leftover export files and opens the database, before This week shows. While it runs the screen shows "Loading…".
- If that fails, the screen shows "Couldn't open the workout database" and the kit's Try again button, which runs start-up again (`Retryable` is left at its default of true).
- An unhandled error in any screen shows the kit's error screen with "Something went wrong" and no details. **Restart** goes to This week; an unsaved workout is lost.
- The E2E tests find the kit's parts by `data-testid`: `startup-loading`, `startup-error`, `startup-retry`, `error-message`, `error-restart`. `ConfirmDialog` uses `confirm-dialog` for the dialog and `{TestId}-cancel` and `{TestId}-confirm` for its buttons, where `TestId` is `confirm-dialog` unless a screen sets another.
