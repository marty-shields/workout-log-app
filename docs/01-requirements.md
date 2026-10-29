# 01 — Requirements

## Functional

### Workout logging
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-01 | Start a workout | Opening New Workout records the start time (date + time). |
| FR-02 | Live timer | The New Workout screen shows elapsed time (h:mm:ss) since the start, updating live. It counts while the phone sleeps and doesn't move when the phone's clock changes (D199). |
| FR-03 | Add an exercise | Pick from the fixed exercise list, shown as the Exercises list over the workout ([Add Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/AddExercise.md)). A search box filters it by name or nickname, every word in any order ([02](02-data-model.md#exercise-search)), and All / Strength / Cardio chips by type. With no search text, the whole list shows Strength then Cardio, A–Z. Each row shows its type, and logged exercises their last session (FR-60). The exercise is added at the top of the workout. A strength exercise starts with one empty set; a cardio exercise starts with its empty entry. |
| FR-04 | Same exercise again | An exercise appears once in a workout; Save, Import and the database all hold to it. Picking one already added moves its card to the top (D42): a strength exercise gets a new set, as Add set does, unless it has 10 sets, and focus moves to it; a cardio exercise focuses its entry (D42, D198). A workout has at most 10 exercises: at 10, Add exercise is off with "At most 10 exercises" (D198). |
| FR-05 | Order | Exercises show newest first; sets in the order added. Picking an exercise already in the workout moves it to the top (D42); nothing else reorders. |
| FR-06 | Strength sets | 1 to 10 sets; at 10, Add set is off with "At most 10 sets" (D198). Each set shows only the metrics the exercise declares (reps, weight kg, duration, distance) and enforces the ones it marks as required. Weight is required on every strength set; 0 kg is unweighted or no assist. A blank weight on a bodyweight move, assisted machine, hold or carry becomes 0 before every check; a lift's must be typed (D23). Dumbbell weight is per dumbbell. See [03](03-exercise-list.md). Add set, and opening another set, wait until the current set is valid; its errors show on it (D37). |
| FR-07 | Cardio entry | Exactly one entry per cardio exercise. Duration is always required, and distance on every exercise that has it. The other metrics (floors, level, incline, calories, rate) show only if the exercise declares them, and are optional. Outdoor Run and Walk show their pace, and Treadmill and Outdoor Cycle their speed, worked out from distance and duration; they are never typed (D197). |
| FR-08 | Remove | A set or a whole exercise can be removed while logging or editing. A cardio entry is removed only with its exercise. |
| FR-09 | Save button | The workout page has a Save button. Save is allowed only when the workout has 1 to 10 exercises, each once, every strength exercise has 1 to 10 sets, and every set and entry has all its required metrics and values within the [rules](02-data-model.md#validation). Otherwise the page stays open, the error shows on the exercise or set at fault, and nothing is saved. |
| FR-10 | Finish time | The first save records the finish time: the start plus the time New Workout was open, as the timer counts it (FR-02, D199). The finish is always after the start. Total time = finish − start. |
| FR-11 | Times fixed | After the first save, start and finish never change. There is no manual edit. The one exception is Import's **Use the file's**, which sets the finish from the file (FR-52). |
| FR-12 | Leave unsaved new workout | The back button asks **Discard / Cancel**. Discard drops the workout; Cancel stays on the page. If the app is killed or crashes, the workout is lost. |
| FR-13 | After save | Saving (new or edit) returns to the Dashboard. |
| FR-14 | Multiple per day | More than one workout can be logged on the same day. |
| FR-15 | Last and PB weight | On New Workout, a strength exercise added to the workout shows *Last 60kg · PB 70kg* under its name: the best weight in the last saved workout with it, and in any saved workout. Best is the heaviest, or the lowest assist on an assisted machine; 0 kg counts (D23). On a bodyweight move, hold or carry, 0 kg shows as no weight: Last shows — when the last is 0, and there is no line when the best is 0. With no saved weight at all, there is no line (D48). |

### Workouts
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-20 | Workouts list | All saved workouts, newest first. Each row shows date, day of week and start time (e.g. *Tue 21 Oct 2026 · 18:05*). With no workouts it shows "No workouts yet". |
| FR-21 | Open | Tapping a row opens the workout in edit mode. |
| FR-22 | Edit | Exercises and sets can be added, changed and removed; cardio entries can be changed (FR-08). FR-09 still applies. Times do not change (FR-11). |
| FR-23 | Leave unsaved edits | If there are unsaved changes, the back button asks **Discard / Cancel**. With no unsaved changes, back returns to Workouts with no prompt. |
| FR-24 | Delete | A confirm dialog appears ("Delete this workout?"). On confirm the workout is permanently removed. |

### Dashboard
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-30 | Home | The app opens to the Dashboard. |
| FR-31 | New workout | A button starts a new workout (FR-01). |
| FR-32 | Last workout | Shows the date + day of the workout with the latest start time. Shows an empty state if there are none. |
| FR-33 | This week | Count of workouts started from Monday 00:00 to now. |
| FR-34 | This month | Count of workouts started from the 1st at 00:00 to now (D66). |
| FR-35 | Workouts and Exercises links | Workouts opens the Workouts list; Exercises opens the Exercises list (FR-60). |
| FR-36 | Settings | A gear in the top bar opens Settings. |

### Exercise list
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-40 | Fixed list | Exercises ship with the app. They cannot be added, edited or deleted in the app. |
| FR-41 | Type | Each exercise is Strength or Cardio. |
| FR-42 | Metrics | Each exercise declares which metrics it shows, which are required and any field label (e.g. Assist (kg), Resistance, Damper), plus its distance, rate and pace units where relevant. |
| FR-43 | Exercise details | Each exercise has nicknames for search. Strength exercises also have a primary muscle, equipment and mechanic (compound/isolation), which are stored but not shown. |

### Settings
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-50 | Settings screen | Shows the title "Settings" and a back arrow. Back, or Android back, returns to the Dashboard. |
| FR-51 | Export workouts | Settings shows how many workouts are saved and exports them all to one JSON file ([format](02-data-model.md#export-file)), then opens Android's share sheet. With no workouts, Export is disabled. A file over 5 MB isn't written. |
| FR-52 | Import workouts | Settings imports a file made by Export, picked with Android's file picker. The whole file is read, brought up to the current format and checked against the same rules as Save ([rules](02-data-model.md#import)), then [Import Review](../src/WorkoutLog.UI.Stories/Stories/Screens/ImportReview.md) shows how many workouts are new, already on the phone, different or can't be imported. **Add to my workouts** adds the new ones and, for different ones, keeps the phone's or uses the file's; **Replace all my workouts** deletes the phone's after a prompt and adds every one that can be imported. Nothing is written until Import; the write is all or nothing. Settings then says what was added, replaced and skipped. |

### Exercises
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-60 | Exercises list | Lists each exercise with a saved workout, latest start first; exercises from the same workout A–Z. Each row shows the name, its type, and `Last <date> · <best>` of its last session, the date in the phone's time zone with the year only when it isn't this year (D191). A search box and All / Strength / Cardio chips filter it, combined. Shows a message when nothing is logged and when nothing matches. Every row has a chevron and opens the exercise's Exercise page (FR-61, FR-62, FR-63, FR-64). Back returns to the Dashboard ([Exercises](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercises.md)). |
| FR-61 | Exercise page: lifts | Shows a lift's progress (D192): Top set and e1RM tabs, a chart for the last 3 months, 6 months, year or all time (opens on Top set and 3 months), the records in the range (heaviest weight, best e1RM) and every session in the range, newest first, with its sets. Top set is the heaviest set, tie → most reps; e1RM is the best Epley estimate of the sets with a weight and 12 reps or fewer. Shows a message when the range has no sessions, or "No sets in the <range>" ("No sets yet" for all time) when none has a value for the tab. Back returns to the Exercises list ([Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercise.md)). |
| FR-62 | Exercise page: bodyweight | Shows a bodyweight move's progress as FR-61 does for a lift (D194). A set of 0 kg is unweighted. Tabs: Best set (most reps in an unweighted set), Total (the unweighted reps added up) and Top set, plus e1RM on a loaded move (D193); every tab always shows, and the page opens on Best set and 3 months. Top set and e1RM use the sets over 0 kg, with the lift rules. Records in the range: most reps in a set, most reps in a session, heaviest weight × reps and, on a loaded move, best e1RM. Each session shows its top set, or its most reps when no set is over 0 kg, and its sets. Back returns to the Exercises list ([Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercise.md)). |
| FR-63 | Exercise page: assisted, holds, carries | Shows an assisted machine's, hold's or carry's progress as FR-61 does for a lift (D195). Assisted: one chart of the least assist per session, lower is better; record least assist. Hold: Duration (longest hold) and Weight (heaviest set, tie → longest) tabs, opens on Duration; records longest hold, heaviest weight. Carry: Weight (heaviest set, tie → longest) and Distance (longest set, tie → heaviest) tabs, opens on Weight; records heaviest weight, longest distance. Holds and carries count a weight only over 0 kg; every tab always shows, and one with no values says "No sets in the <range>", or "No sets yet" for All ([Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercise.md)). |
| FR-64 | Exercise page: cardio | Shows a cardio exercise's progress (D196): a tab per metric it has, in the order Distance, Duration, Pace or Speed, Floors, Calories, with pace and speed worked out from each session's distance and duration (D197); a chart for the last 3 months, 6 months, year or all time (opens on the first tab and 3 months), the records in the range (longest distance, longest duration, fastest pace, fastest speed, most floors, most calories) and every session in the range, newest first, with its values. Higher is better, except Pace. Shows a message when the range has no sessions, or none with a value for the tab. Back returns to the Exercises list ([Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercise.md)). |

## Non-functional
| ID | Requirement |
|---|---|
| NFR-01 | Works fully offline. No network calls. |
| NFR-02 | Android 16 (API 36) only. |
| NFR-03 | Single user. No login. |
| NFR-04 | Weight in kg only. |
| NFR-05 | Data is stored on the device in SQLite. |
| NFR-06 | Times are stored in UTC and shown in the phone's current time zone. A workout's date, day and week/month count come from its start time. |
| NFR-07 | Android Auto Backup backs up the Release app's data. |
