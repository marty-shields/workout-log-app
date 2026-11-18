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
| Bottom bar | `BottomBar` actions, in thumb reach. Optional. |
| Navigation bar | `BottomNav` under the bottom bar, when `Destination` is set; else whatever `Navigation` holds, such as a `MainTabBar`. |
| Status | `Status`, a `Snackbar` just above the bottom bar (or the navigation bar), over the content. Its slot is a polite live region (`role="status"`), there whenever `Status` is set, so a snackbar shown later is read out. While a snackbar shows, the content gets room at its foot (`2 × --touch-target` more) so its last controls scroll clear of it. Needs a bottom bar or a navigation bar. Optional. |

- `Destination` makes the screen top-level: it shows the `BottomNav` with that item current and raises `OnNavigate` with the item tapped. A top-level screen sets no `OnBack`, so it has no back arrow. Android back raises `OnNavigate` with the Dashboard (`NavDestinations.Start`), except on the Dashboard item's screen, This week, where it leaves the app.
- Top padding includes the safe-area inset (`--inset-top`). The bottom inset (`--inset-bottom`) goes under the last thing on screen: the navigation bar, else the bottom bar, else the content.
- `OnRefresh` makes the content pull-to-refresh. Pulled down from the top, a ring like the `Spinner`'s follows the finger at half its travel, coming out from under the top bar. Let go, it goes back up over `--pull-refresh-return-duration`, and if it was past `--pull-refresh-threshold`, `OnRefresh` runs. The ring shows only while pulling, so the screen shows its own loading state, as on first open: each top-level screen shows its `Spinner` "Loading…" in place of the content. A pull during a refresh does nothing. The [Pulling](./?path=/story/components-layout-appscreen--pulling) story shows the ring mid-pull. With reduced motion the ring doesn't slide back. The content doesn't overscroll, so Android's stretch doesn't fight the pull. No gesture without `OnRefresh`.
- `data-testid`: `back` (or `BackTestId`, for a screen layered over another), `screen-title`, `screen-subtitle`, `bottom-bar`, `pull-refresh` (with `OnRefresh`), plus `BottomNav`'s.

Stories: [AppScreen](./?path=/docs/components-layout-appscreen--docs)

## `BottomNav`
The Material navigation bar for the top-level screens: Dashboard, Workouts and Exercises, in that order, from the `NavDestinations` table (label, icon, test id and route of each `NavDestination`).

| Part | Content |
|---|---|
| Item | Icon over its label, a third of the width each, at least 48px tall. Dashboard: house; Workouts: history clock; Exercises: dumbbell. |
| Current item | A `--color-selected` pill behind the icon, label and icon in the text colour and bold; the others are muted. `aria-current="page"`. |

- Tapping an item raises `OnNavigate` with its destination. Tapping the current item does nothing.
- A `<nav>` labelled "Main"; each item is a button. A pressed item shows the shared state layer on its indicator pill, not across the whole item and the inset.
- Its bottom padding is the safe-area inset (`--inset-bottom`).
- `data-testid`: `bottom-nav`, `dashboard-link`, `workouts-link`, `exercises-link`.

Stories: [BottomNav](./?path=/docs/components-layout-bottomnav--docs)

## `MainTabBar`
The three-slot bar that will replace `BottomNav` when the app moves to This week and Trends. Not in use by the app yet: only [This week](./?path=/custom/screens-this-week-overview) shows it, through `AppScreen`'s `Navigation` slot. `BottomNav` and `Destination` are unchanged.

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

## `StartupGate`
Runs `Initialize` once at startup. Shows the [Startup](./?path=/custom/screens-startup-overview) screen while it runs or if it throws, then `ChildContent`.

Stories: [StartupGate](./?path=/docs/components-layout-startupgate--docs)

## `AppErrorBoundary`
Shows `ChildContent` until it throws an unhandled error, then logs the error and shows the [Error](./?path=/custom/screens-error-overview) screen. **Restart** raises `OnRestart`, then shows `ChildContent` again.

Stories: [AppErrorBoundary](./?path=/docs/components-layout-apperrorboundary--docs)
