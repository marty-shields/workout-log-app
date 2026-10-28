---
$attribute: CustomPage("Screens/Exercise/Overview")
---

# Exercise

One exercise's progress: a chart, its records and its sessions. View: `ExerciseView`; page: `ExercisePage`, for lifts only (FR-61). The other exercise types are a design only: the App doesn't show them yet (D182).

`ExercisePage` opens from a lift row on [Exercises](./?path=/custom/screens-exercises-overview) (`/exercises/{id}`) on Top set and 3M. It loads every session through `IExerciseProgressService.LoadAsync` when it opens; changing the tab or range works the chart, records and sessions out again in memory, with no spinner (D192). Back, or Android back, returns to Exercises. An id not in the exercise list, or not a lift, shows Load failed.

| Part | Content |
|---|---|
| Top bar | Back arrow; the exercise name, and under it primary muscle · equipment (the muscle only when the equipment is None), or "Cardio". Two-word names are spelled out as in the exercise names, e.g. Lower Back, EZ-Bar, Smith Machine. |
| Tabs | `SegmentedControl` of the exercise's charts. Hidden when it has one. |
| Chart card | Optional title, the `Chart`, a range `SegmentedControl` (3M, 6M, 1Y, All) and an optional caption. |
| Records | `RecordsCard` titled with the range, e.g. "Records · last 3 months". |
| Sessions | Heading "Sessions" and a `SessionRow` per session in the range, newest first. Hidden when there are none. |
| Loading | `Spinner` "Loading…" centred; the top bar shows the name. |
| Load failed | Error "Couldn't load this exercise" and a Secondary **Try again**. Focus moves to Try again. |

| Exercise type | Tabs | Caption |
|---|---|---|
| Lift | Top set (weight, ×reps over each point), e1RM (to up to 1 decimal place, e.g. 82.3kg or 80kg) | e1RM: "Estimated one-rep max" |
| Bodyweight | Best set, Total, Top set; e1RM too once done with weight | Dumbbell moves: "Weight per dumbbell" |
| Assisted | None; chart titled "Least assist per session", lower is better | "Up = less assist" |
| Hold | Duration, Weight | |
| Carry | Weight, Distance | |
| Cardio | Distance, Duration, Pace (lower is better), Calories | Pace: "Up = faster" |

Range text: last 3 months, last 6 months, last year, all time. A tab with no values shows its own message in place of the chart, e.g. "No weighted sets in the last 3 months"; with no sessions in the range it is "No sessions in the last 3 months", or "No sessions yet" for All. Records with no value show "—".

Lift text: records "Heaviest weight" (e.g. *70kg × 3*) and "Best e1RM" (e.g. *82.3kg*), each with its date, e.g. *20 Oct*. Each session shows its date (e.g. *Tue 20 Oct*), its top set as the headline, or its most reps (e.g. *8 reps*) when no set has a weight, and every set, e.g. *60×8 · 62.5×8 · 8 reps*. Dates have the year when it isn't this year. Tab messages: Top set "No weighted sets in the last 3 months"; e1RM "No sets of 12 reps or fewer in the last 3 months"; "… yet" for All.

States: [Lift top set](./?path=/story/screens-exercise--lift-top-set) · [Lift e1RM](./?path=/story/screens-exercise--lift-e1rm) · [Lift e1RM no data](./?path=/story/screens-exercise--lift-e1rm-no-data) · [Bodyweight best set](./?path=/story/screens-exercise--bodyweight-best-set) · [Bodyweight total](./?path=/story/screens-exercise--bodyweight-total) · [Bodyweight top set never weighted](./?path=/story/screens-exercise--bodyweight-top-set-never-weighted) · [Loaded bodyweight](./?path=/story/screens-exercise--loaded-bodyweight) · [Assisted](./?path=/story/screens-exercise--assisted) · [Hold duration](./?path=/story/screens-exercise--hold-duration) · [Hold weight](./?path=/story/screens-exercise--hold-weight) · [Carry weight](./?path=/story/screens-exercise--carry-weight) · [Carry distance](./?path=/story/screens-exercise--carry-distance) · [Cardio distance](./?path=/story/screens-exercise--cardio-distance) · [Cardio duration](./?path=/story/screens-exercise--cardio-duration) · [Cardio pace](./?path=/story/screens-exercise--cardio-pace) · [Cardio no data](./?path=/story/screens-exercise--cardio-no-data) · [No sessions in range](./?path=/story/screens-exercise--no-sessions-in-range) · [One session](./?path=/story/screens-exercise--one-session) · [Loading](./?path=/story/screens-exercise--loading) · [Load failed](./?path=/story/screens-exercise--load-failed)

`data-testid`: `back`, `exercise-tabs`, `exercise-tab-{tab}`, `exercise-chart-card`, `exercise-chart`, `exercise-chart-values`, `exercise-chart-empty`, `exercise-chart-caption`, `exercise-range`, `exercise-range-3m`, `exercise-range-6m`, `exercise-range-1y`, `exercise-range-all`, `exercise-records`, `exercise-session-{workout id}`, `exercise-loading`, `exercise-error`, `exercise-retry`.
