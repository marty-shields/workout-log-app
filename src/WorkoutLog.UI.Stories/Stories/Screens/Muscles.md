---
$attribute: CustomPage("Screens/Sets per muscle group/Overview")
---

# Sets per muscle group

Strength sets per muscle group in the last 7 days, to pick what to train next. View: `MusclesView`. Not in the app yet.

| Part | Content |
|---|---|
| Top bar | Back arrow; title "Sets per muscle group", subtitle "Last 7 days". |
| Content | One row per group: Chest, Back, Shoulders, Arms, Legs, Core, each with its sets at the right, e.g. *12 sets*, *1 set*. A group with none shows "Not trained", muted. |
| Loading | `Spinner` "Loading…" centred until the groups load. |
| Load failed | Error "Couldn't load your muscles" and a Secondary **Try again** that reloads. Focus moves to Try again. |

Opens from the Sets per muscle group row on the [Dashboard](./?path=/custom/screens-dashboard-overview). Back: Dashboard.

States: [Some trained](./?path=/story/screens-sets-per-muscle-group--some-trained) · [All trained](./?path=/story/screens-sets-per-muscle-group--all-trained) · [None trained](./?path=/story/screens-sets-per-muscle-group--none-trained) · [Loading](./?path=/story/screens-sets-per-muscle-group--loading) · [Load failed](./?path=/story/screens-sets-per-muscle-group--load-failed)

`data-testid`: `back`, `muscles-{group}` (e.g. `muscles-chest`), `muscles-loading`, `muscles-error`, `muscles-retry`.
