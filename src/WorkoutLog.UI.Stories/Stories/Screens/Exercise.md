---
$attribute: CustomPage("Screens/Exercise/Overview")
---

# Exercise

One exercise's progress: a chart, its records and its sessions. View: `ExerciseView`; page: `ExercisePage`, for lifts, bodyweight moves, assisted machines, holds and carries, and cardio.

`ExercisePage` opens from a row on [Exercises](./?path=/custom/screens-exercises-overview) (`/exercises/{id}`) on 3M and the type's first tab: Top set for a lift, Best set for a bodyweight move, Duration for a hold, Weight for a carry, and the first of its metrics for cardio. It loads every session through `IExerciseProgressService.LoadAsync`, or `ICardioProgressService.LoadAsync` for cardio, when it opens; changing the tab or range works the chart, records and sessions out again in memory, with no spinner. Back, or Android back, returns to Exercises. An id not in the exercise list shows Load failed.

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
| Bodyweight | Best set, Total, Top set; e1RM too on a loaded move | e1RM: "Estimated one-rep max" |
| Assisted | None; chart titled "Least assist per session", lower is better | "Up = less assist" |
| Hold | Duration, Weight | |
| Carry | Weight, Distance | |
| Cardio | The metrics it has: Distance, Duration, then Pace (Outdoor Run, Walk; lower is better) or Speed (Treadmill, Outdoor Cycle), Floors, Calories. Pace and Speed are worked out from each session's distance and duration | Pace: "Up = faster" |

Range text: last 3 months, last 6 months, last year, all time. A tab with no values shows its own message in place of the chart: on strength, "No sets in the last 3 months", or "No sets yet" for All; on cardio, its metric (below); with no sessions in the range it is "No sessions in the last 3 months", or "No sessions yet" for All. Records with no value show "—".

Rules: a range is calendar months in the phone's time zone; a session is in range when its local start date is on or after today minus 3, 6 or 12 months, and All has no limit. One point per session, by start time; a session with no value for the tab leaves a gap that the line joins across. e1RM is the best Epley value, weight × (1 + reps / 30), of the sets with a weight and 1–12 reps, a 1-rep set being its weight, compared unrounded. A record tie takes the earliest session. Pace and speed are compared and charted unrounded.

Lift text: records "Heaviest weight" (e.g. *70kg × 3*) and "Best e1RM" (e.g. *82.3kg*), each with its date, e.g. *20 Oct*. Each session shows its date (e.g. *Tue 20 Oct*), its top set as the headline, or its most reps (e.g. *8 reps*) when no set has a weight, and every set, e.g. *60×8 · 62.5×8 · 8 reps*. Dates have the year when it isn't this year.

Bodyweight text: a set of 0 kg is unweighted. Best set is the most reps in an unweighted set and Total the unweighted reps added up, as plain numbers; Top set and e1RM use the sets over 0 kg, as on a lift. Records "Most reps in a set" and "Most reps in a session" (e.g. *25 reps*), "Heaviest weight" (e.g. *16kg × 8*) and, on a loaded move, "Best e1RM". Each session shows its top set as the headline, or its most reps (e.g. *25 reps*) when no set is over 0 kg, and every set, weighted as *16×8* and unweighted as reps alone, e.g. *16×8 · 20*.

Assisted text: no assist is 0kg. Each point is the session's least assist, tie → most reps, labelled ×reps. Record "Least assist", e.g. *0kg × 5*. Each session shows its least assist as the headline and every set with its assist, e.g. *5kg×8 · 0kg×5*.

Hold text: Duration is each session's longest hold, m:ss. Weight is each session's heaviest set over 0kg, tie → longest, labelled with its duration, e.g. *0:45*. Records "Longest hold" (e.g. *1:45*) and "Heaviest weight" (e.g. *10kg · 1:00*). Each session shows its longest hold and every set, e.g. *1:45 · 10kg 1:00*.

Carry text: Weight is each session's heaviest set over 0kg, tie → longest, labelled with its distance, e.g. *40m*. Distance is each session's longest set, tie → heaviest, labelled with its weight when over 0kg. Records "Heaviest weight" (e.g. *32kg · 40m*) and "Longest distance" (e.g. *60m · 24kg*, or *60m*). Each session shows its heaviest set, or its longest when none has a weight, and every set's weight, distance and duration, e.g. *28kg 40m 0:45 · 60m*.

Holds and carries leave out a weight of 0kg.

Cardio text: records "Longest distance" (e.g. *5.2km*, in the exercise's unit: km, m or ft), "Longest duration" (*30:00*, h:mm:ss from an hour), "Fastest pace" (*5:30/km*, to the second), "Fastest speed" (*10.4 km/h*, to 2 decimal places), "Most floors" (*120 floors*) and "Most calories" (*410 calories*), only for the metrics the exercise has. Each session shows its date, its longest distance as the headline, or its longest duration when the exercise has no distance (— when the session has none), and its entry's other values, e.g. *30:00 · 5:46/km · 410 calories · Resistance 8 · 2% · 85rpm* or *30:00 · 10.4 km/h · 410 calories · 1.5%*. Tab messages: "No distance logged in the last 3 months", and likewise for duration, pace, speed, floors and calories; "… yet" for All.

States: [Lift top set](./?path=/story/screens-exercise--lift-top-set) · [Lift e1RM](./?path=/story/screens-exercise--lift-e1rm) · [Lift e1RM no data](./?path=/story/screens-exercise--lift-e1rm-no-data) · [Bodyweight best set](./?path=/story/screens-exercise--bodyweight-best-set) · [Bodyweight total](./?path=/story/screens-exercise--bodyweight-total) · [Bodyweight top set never weighted](./?path=/story/screens-exercise--bodyweight-top-set-never-weighted) · [Loaded bodyweight](./?path=/story/screens-exercise--loaded-bodyweight) · [Assisted](./?path=/story/screens-exercise--assisted) · [Hold duration](./?path=/story/screens-exercise--hold-duration) · [Hold weight](./?path=/story/screens-exercise--hold-weight) · [Hold weight no data](./?path=/story/screens-exercise--hold-weight-no-data) · [Carry weight](./?path=/story/screens-exercise--carry-weight) · [Carry distance](./?path=/story/screens-exercise--carry-distance) · [Carry weight no data](./?path=/story/screens-exercise--carry-weight-no-data) · [Cardio distance](./?path=/story/screens-exercise--cardio-distance) · [Cardio duration](./?path=/story/screens-exercise--cardio-duration) · [Cardio pace](./?path=/story/screens-exercise--cardio-pace) · [Cardio speed](./?path=/story/screens-exercise--cardio-speed) · [Cardio no data](./?path=/story/screens-exercise--cardio-no-data) · [Cardio floors](./?path=/story/screens-exercise--cardio-floors) · [No sessions in range](./?path=/story/screens-exercise--no-sessions-in-range) · [One session](./?path=/story/screens-exercise--one-session) · [Loading](./?path=/story/screens-exercise--loading) · [Load failed](./?path=/story/screens-exercise--load-failed)

`data-testid`: `back`, `exercise-tabs`, `exercise-tab-{tab}`, `exercise-chart-card`, `exercise-chart`, `exercise-chart-values`, `exercise-chart-empty`, `exercise-chart-caption`, `exercise-range`, `exercise-range-3m`, `exercise-range-6m`, `exercise-range-1y`, `exercise-range-all`, `exercise-records`, `exercise-session-{workout id}`, `exercise-loading`, `exercise-error`, `exercise-retry`.
