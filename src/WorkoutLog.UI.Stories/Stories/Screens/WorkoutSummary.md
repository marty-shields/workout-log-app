---
$attribute: CustomPage("Screens/Workout Summary/Overview")
---

# Workout Summary

What you did and what improved, shown after saving a new workout. View: `WorkoutSummaryView`; page: `WorkoutSummaryPage` (not built yet). Saving an edit goes to the [Dashboard](./?path=/custom/screens-dashboard-overview) as before.

| Part | Content |
|---|---|
| Top bar | Title "Workout saved". No back arrow; Android back does what Done does. |
| Totals | A card with Duration (finish − start, h:mm:ss), Exercises and Sets (strength sets), each a number in `--color-accent` over its muted label. |
| Records | Card "Records": a row per record, the Record `Icon` in `--color-success` and the exercise with its best, e.g. *Deadlift · 160kg × 3*. "No records this time" when there are none. |
| Since last time | Card "Since last time": a row per exercise in the workout's order, the exercise with its best in muted text under it, e.g. *Barbell Bench Press* and *90kg × 5*, and the change at the right. |
| First workout | When there is no earlier workout, a card "Your first workout. Records and changes show from your next one." replaces Records and Since last time. |
| Loading | `Spinner` "Loading…" centred instead of the cards. |
| Load failed | Error "Couldn't load the summary" and a Secondary full-width **Try again** that loads again, instead of the cards. Focus moves to Try again. |
| Bottom bar | Done, `AppButton` primary, full width, in every state. |

The change is the exercise's best in this workout against its best in its last session before this one:

| Change | Shows |
|---|---|
| Better | The Up `Icon` and the difference in `--color-success`, e.g. *▲ 2.5kg*. Screen readers hear "up 2.5kg". |
| Worse | The Down `Icon` and the difference in `--color-error`. |
| The same | A muted "—". Screen readers hear "no change". |
| No earlier session | A muted "First time". |

Better and worse follow the Exercises list's best: for an assisted exercise, less assist is better. Records follow the same rule as [Dashboard](./?path=/custom/screens-dashboard-overview) Recent records and the record badge on [New Workout](./?path=/custom/screens-new-workout-overview): an exercise's first session and a tie aren't records.

Done and Android back return to the Dashboard; the workout is already saved.

States: [Records and changes](./?path=/story/screens-workout-summary--records-and-changes) · [No records](./?path=/story/screens-workout-summary--no-records) · [First workout](./?path=/story/screens-workout-summary--first-workout) · [All first time](./?path=/story/screens-workout-summary--all-first-time) · [Loading](./?path=/story/screens-workout-summary--loading) · [Load failed](./?path=/story/screens-workout-summary--load-failed)

`data-testid`: `screen-title`, `summary-stats`, `summary-duration`, `summary-exercises`, `summary-sets`, `summary-records`, `summary-record-{exercise id}`, `summary-records-none`, `summary-changes`, `summary-change-{exercise id}`, `summary-first-workout`, `summary-loading`, `summary-error`, `summary-retry`, `done`, `bottom-bar`.
