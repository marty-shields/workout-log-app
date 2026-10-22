---
$attribute: CustomPage("Design System/Screens/Exercise Picker")
---

# Exercise Picker

Add an exercise to the workout (FR-03, FR-04). Component: `ExercisePicker`, shown in the `WorkoutView` overlay.

| Part | Content |
|---|---|
| Bar | Back arrow and search box, on the dark teal bar. The search box has focus when the picker opens. |
| Results | No search text: Strength then Cardio, A–Z. With text: matches, name-prefix matches first. Exercises already in the workout still show. |
| No match | "No exercises match". |

Back closes the picker.

States: [No search text](/?path=/story/screens-exercise-picker--no-search-text) · [Results](/?path=/story/screens-exercise-picker--results) · [No match](/?path=/story/screens-exercise-picker--no-match)

`data-testid`: `exercise-picker`, `picker-back`, `picker-search`, `picker-group-{kind}`, `picker-item-{id}`, `picker-none`.
