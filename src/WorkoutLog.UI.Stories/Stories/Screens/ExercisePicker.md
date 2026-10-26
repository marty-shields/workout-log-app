---
$attribute: CustomPage("Screens/Exercise Picker/Overview")
---

# Exercise Picker

Add an exercise to the workout (FR-03, FR-04). Component: `ExercisePicker`, an overlay on `WorkoutView`.

| Part | Content |
|---|---|
| Bar | Back arrow and search box, on the dark teal bar. The search box is empty each time the picker opens (D152) and has focus (D109). |
| Results | No search text: Strength then Cardio, A–Z. With text: matches, name-prefix matches first ([how it matches](https://github.com/marty-shields/workout-log-app/blob/main/docs/02-data-model.md#exercise-search)). Exercises already in the workout still show. |
| No match | "No exercises match". |

Tapping an exercise adds it to the workout and closes the picker. Picking one already in the workout moves its card to the top instead of adding it again (FR-04, D154, D236); see [New Workout](./?path=/custom/screens-new-workout-overview).

Back closes the picker; with the soft keyboard open, Android back first only hides the keyboard (D206).

States: [No search text](./?path=/story/screens-exercise-picker--no-search-text) · [Results](./?path=/story/screens-exercise-picker--results) · [No match](./?path=/story/screens-exercise-picker--no-match)

`data-testid`: `exercise-picker`, `picker-back`, `picker-search`, `picker-group-{kind}`, `picker-item-{id}`, `picker-none`.
