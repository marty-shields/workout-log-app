# 05 — Screens & UX

## Screens

### Startup
- Shown while the database opens, before the Dashboard (T36)
- Spinner with "Loading…" (D134), or "Couldn't open the workout database." if it fails (D133)

### Error
- Shown instead of any screen that hits an unhandled error (D140)
- "Something went wrong." and **Restart**, which goes to the Dashboard. An unsaved workout is lost.

### Dashboard (home)
- **New Workout** button
- Last workout: date + day of the latest start, or an empty state (FR-32)
- Workouts this week: from Monday 00:00 to now (FR-33, D196)
- Workouts this month: from the 1st at 00:00 to now (FR-34, D196)
- A workout dated after now shows as the last workout but isn't counted (D196)
- **History** link
- Loads each time it's shown and when the app comes back (D197). In place of the last workout and counts: while loading, spinner "Loading…"; if it fails, "Couldn't load the Dashboard." and **Try again**, which reloads and takes focus. History and New Workout still work (D195)

### New / Edit Workout
- Header: date + day (from the start time)
  - New: live elapsed timer (h:mm:ss)
  - Edit: total time (h:mm:ss)
- List of added exercises, newest first (D236). Each one shows the fields its definition declares ([03](03-exercise-list.md)):
  - Strength: rows of sets (e.g. reps + weight, duration for Plank, distance for carries), **Add set**, remove set. Starts with one empty set. Add set does nothing while the last set (or the open set, below) is empty or has errors; the errors show on that set and focus moves to its first field at fault (D106, D162). Format errors and Core's rules are in [02](02-data-model.md#entry-formats-ui).
    - Exercises with more than two fields, one of them a duration (carries): one set is open, with every field labelled; the others show as a summary line (e.g. *40 m · 32 kg · 0:00:45*). Tap a summary to open it, once the open set is valid; Add set opens the new set. A new workout opens the last set; an edited workout starts with all sets closed. Errors on a closed set show under its summary, which gets a red outline (D105, D118).
  - Cardio: one entry (duration + the exercise's other metrics, with units). Starts empty; not removable on its own.
  - Fields use the exercise's label where it has one (e.g. Assist (kg), Resistance, Damper).
  - A partly filled duration or pace fills in when focus leaves it: empty boxes become 0, minutes and seconds two digits (e.g. *1:00:00*, *0:00:22*, *5:03*) (D107).
  - Remove exercise: removes it at once, with no confirm (D150)
- **Add exercise** opens the Exercise picker. A picked exercise goes on top; picking one already in the workout moves its card to the top (D154, D236). After a pick, focus moves to its first field, which scrolls it into view (D151).
- **Save** button: validates (FR-09), saves and returns to the Dashboard.
  - Save failed: "Couldn't save the workout. Try again." shows at the top, the workout stays and focus moves to Save (D176).
  - Taps while a save runs are ignored (D177); on Edit, also while a delete runs and after one succeeds (D191).
  - Errors show on the exercise, set or entry at fault: each faulty field is outlined, with one message per problem under its set or entry (D103). Focus moves to the first error: Add exercise, an exercise's Add set or Remove, the first field at fault, or a closed set's summary line (D171).
  - Once a set or entry fails a check, every change re-checks it until it passes Add set or opening another set, or the whole workout passes Save (D108, D174). Adding a set clears "Add at least one set" and adding an exercise clears the workout error (D108). Cardio entries are first checked on Save (D166).
- Edit only: **Delete** asks "Delete this workout?" **Delete / Cancel**, the same with or without unsaved edits. Delete removes the workout and goes to History (FR-24). If it fails, "Couldn't delete the workout. Try again." shows at the top, the workout and its edits stay and focus moves to Delete. Ignored while a save or delete runs, after one succeeds and after a discard; while a delete runs and after it succeeds, Cancel and Android back leave the prompt open (D191).
- Edit only, until the workout loads, under the title "Workout" (D185):
  - Loading: spinner "Loading…"
  - Load failed: "Couldn't load the workout." and **Try again**, which reloads. Focus moves to Try again. An exercise missing from the list, or saved as the wrong kind, also shows this.
  - Not found: "This workout no longer exists.", no Try again
- Back:
  - New workout: **Discard / Cancel**, even with no exercises; ignored while a save runs and after one succeeds (D179)
  - Edit with unsaved changes: **Discard changes? Discard / Cancel**; Discard goes to History. A change is a value typed, a set added or removed, or an exercise added, picked again or removed, even if later undone (D189, D236)
  - Edit with no unsaved changes: History, no prompt
  - Edit: ignored while a save runs and after one succeeds (D189), and while a delete runs and after one succeeds (D191)
  - Edit while loading, after a failed load or when not found: History (D185)
  - With the soft keyboard open, Android back only hides the keyboard; the next back acts as above. The back arrow acts at once (D206)

### Exercise picker
- An overlay on the workout page. Back closes it; with the soft keyboard open, Android back first only hides the keyboard (D206).
- Search box at the top, empty each time the picker opens (D152). How it matches: [02](02-data-model.md#exercise-search).
- With no search text: the full list A–Z, grouped Strength / Cardio
- Tap to add to the workout; the picker closes
- Exercises already in the workout are still shown. Picking one moves its card to the top instead of adding it again: strength adds a set, cardio focuses its entry (FR-04, D154, D236)

### History
- Saved workouts, newest first by start time; same start, newest saved first (D182). All load at once (D183)
- Row: date, day of week and start time (e.g. *Tue 21 Oct 2026 · 18:05*)
- Tap opens the workout in edit mode (D186)
- Empty state: "No workouts yet"
- While loading: spinner "Loading…" (D181)
- Load failed: "Couldn't load your history." and **Try again**, which reloads. Focus moves to Try again (D180)
- Back: Dashboard

Back on the Dashboard leaves the app.

## Navigation
```mermaid
flowchart TD
    D[Dashboard] -->|New Workout| W[New Workout]
    D -->|History| H[History]
    H -->|tap row| E[Edit Workout]
    W -->|Add exercise| P[Exercise picker overlay]
    E -->|Add exercise| P
    P -->|pick or back| W
    P -->|pick or back| E
    H -->|Back| D
    W -->|Save| D
    W -->|Back: Discard| D
    E -->|Save| D
    E -->|Back: Discard| H
    E -->|Back: no changes| H
    E -->|Delete confirmed| H
```

## Design system & prototyping
The final designs are stories in `WorkoutLog.UI.Stories`, with a Design System page for each component group and screen (D99, D114, D115). The layout rules (themes, keyboards, focus, thumb reach, safe-area insets) are on those pages: [10 Design system](10-design-system.md).
