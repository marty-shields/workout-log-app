# Exercise List

The list ships with the app and changes only through an app update. The full list is [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json), embedded in Core.

## Source
Strength exercises are based on [free-exercise-db](https://github.com/yuhonas/free-exercise-db) (public domain; strength and powerlifting only), checked for gaps against muscleandstrength.com, and include the reviewer's 44 must-haves. Cardio, carries and a few others were written by hand. Only names are used.

## Adding an exercise
- Ids are one sequence for all exercises, never reused or changed; a new exercise takes the next number.
- Exercises are never removed or hidden.
- **Folded into the base exercise:** alternating and one-arm versions (kept where the exercise is normally one-sided, e.g. Dumbbell One-Arm Row); seated and standing versions; stance and foot position (Sumo is kept as its own exercise); weighted bodyweight moves, bands and chains, pause, speed and partial reps.
- **Kept separate:** equipment, bench angle, grip and attachment (e.g. Cable V-Bar Tricep Pushdown).
- **Left out:** stretching, plyometrics, Olympic lifts, strongman (except carries), and rarely used exercises.
- **Names:** equipment first (`Dumbbell Incline Press`); no prefix for bodyweight moves, carries and `Deadlift` (`Pull-up`, `Farmer's Walk`); singular forms (Curl, Fly, Tricep, Pulldown).

## Categories
| Category | Fields | Weight |
|---|---|---|
| Lift | Reps, weight | Must be typed; 0 allowed |
| Bodyweight (weight is extra load, e.g. Push-up, Pull-up) | Reps, weight | A blank becomes 0 |
| Hold (e.g. Plank, Wall Sit) | Duration, weight | A blank becomes 0 |
| Assisted (Machine Assisted Pull-up, Chin-up, Dip) | Reps, weight labelled **Assist (kg)** | A blank becomes 0 |
| Carry (e.g. Farmer's Walk, Sled Push) | Duration, distance (metres), weight | A blank becomes 0 |
| Cardio | Duration, distance where it has one, then optional metrics per exercise | |

- 0 kg is unweighted, or no assist. On a bodyweight move, hold or carry it shows as no weight.
- `loaded` is true on a bodyweight move often done holding weight; its Exercise page adds an e1RM tab.
- A plate or harness exercise is a Lift; a Bodyweight exercise may use a band, wheel or ball.
