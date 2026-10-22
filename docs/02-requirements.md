# 02 — Requirements

## Functional

### Workout logging
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-01 | Start a workout | Opening New Workout records the start time (date + time). |
| FR-02 | Live timer | The New Workout screen shows elapsed time (h:mm:ss) since the start, updating live. |
| FR-03 | Add an exercise | Pick from the fixed exercise list. A search box filters it by name or nickname, every word in any order ([03](03-data-model.md#exercise-search)). With no search text, the full list shows A–Z, grouped Strength / Cardio. The exercise is added at the bottom of the workout. A strength exercise starts with one empty set; a cardio exercise starts with its empty entry. |
| FR-04 | Same exercise twice | The same exercise can be added more than once in one workout. |
| FR-05 | Order | Exercises and sets show in the order they were added. No reordering. |
| FR-06 | Strength sets | Any number of sets. Each set shows only the metrics the exercise declares (reps, weight kg, duration, distance) and enforces the ones it marks as required. Dumbbell weight is per dumbbell. See [04](04-exercise-list.md). |
| FR-07 | Cardio entry | Exactly one entry per cardio exercise. Duration is always required. The other metrics (distance, floors, level, incline, calories, rate) show only if the exercise declares them, and are optional. |
| FR-08 | Remove | A set or a whole exercise can be removed while logging or editing. A cardio entry is removed only with its exercise. |
| FR-09 | Save button | The workout page has a Save button. Save is allowed only when the workout has at least one exercise, every exercise has at least one set or entry, and every set and entry has all its required metrics. Otherwise the page stays open, the error shows on the exercise or set at fault, and nothing is saved. |
| FR-10 | Finish time | The first save records the finish time. Total time = finish − start. |
| FR-11 | Times fixed | After the first save, start and finish never change. There is no manual edit. |
| FR-12 | Leave unsaved new workout | The back button asks **Discard / Cancel**. Discard drops the workout; Cancel stays on the page. If the app is killed or crashes, the workout is lost. |
| FR-13 | After save | Saving (new or edit) returns to the Dashboard. |
| FR-14 | Multiple per day | More than one workout can be logged on the same day. |

### History
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-20 | History list | All saved workouts, newest first. Each row shows date, day of week and start time (e.g. *Tue 21 Oct 2026 · 18:05*). With no workouts it shows "No workouts yet". |
| FR-21 | Open | Tapping a row opens the workout in edit mode. |
| FR-22 | Edit | Exercises and sets can be added, changed and removed; cardio entries can be changed (FR-08). FR-09 still applies. Times do not change (FR-11). |
| FR-23 | Leave unsaved edits | If there are unsaved changes, the back button asks **Discard changes / Cancel**. With no unsaved changes, back returns to History with no prompt. |
| FR-24 | Delete | A confirm dialog appears ("Delete this workout?"). On confirm the workout is permanently removed. |

### Dashboard
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-30 | Home | The app opens to the Dashboard. |
| FR-31 | New workout | A button starts a new workout (FR-01). |
| FR-32 | Last workout | Shows the date + day of the workout with the latest start time. Shows an empty state if there are none. |
| FR-33 | This week | Count of workouts started from Monday 00:00 to now. |
| FR-34 | This month | Count of workouts started in the current calendar month. |
| FR-35 | History link | Opens the History list. |

### Exercise list
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-40 | Fixed list | Exercises ship with the app. They cannot be added, edited or deleted in the app. |
| FR-41 | Type | Each exercise is Strength or Cardio. |
| FR-42 | Metrics | Each exercise declares which metrics it shows, which are required and any field label (e.g. Assist (kg), Resistance, Damper), plus its distance unit and rate unit where relevant. |
| FR-43 | Exercise details | Each exercise has nicknames for search. Strength exercises also have a primary muscle, equipment and mechanic (compound/isolation), which are stored but not shown in v1. |

## Non-functional
| ID | Requirement |
|---|---|
| NFR-01 | Works fully offline. No network calls. |
| NFR-02 | Android 16 (API 36) only. |
| NFR-03 | Single user. No login. |
| NFR-04 | Weight in kg only. |
| NFR-05 | Data is stored on the device in SQLite. |
| NFR-06 | Times are stored in UTC and shown in the phone's current time zone. A workout's date, day and week/month count come from its start time. |
