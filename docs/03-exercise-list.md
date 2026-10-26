# 03 — Exercise List

The list ships with the app and changes only through an app update. The full list is [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json). It is Core's embedded definition file.

## Source
Strength exercises are based on [free-exercise-db](https://github.com/yuhonas/free-exercise-db) (public domain; strength and powerlifting only), checked for gaps against muscleandstrength.com, and include the reviewer's 44 must-haves (D51). Cardio, carries and a few others were written by hand. Only names are used.

## Selection rules
- **Folded into the base exercise:**
  - alternating and one-arm versions (kept where the exercise is normally one-sided, e.g. Dumbbell One-Arm Row)
  - seated and standing versions
  - stance and foot position (Sumo is kept as its own exercise)
  - weighted bodyweight moves, bands and chains, pause, speed and partial reps
- **Kept separate:** equipment, bench angle, grip and attachment (e.g. Cable V-Bar Tricep Pushdown).
- **Left out:** stretching, plyometrics, Olympic lifts, strongman (except the carries below), and rarely used exercises.

## Names
- Equipment comes first: `Dumbbell Incline Press`, `Cable Rope Tricep Pushdown`, `Machine Leg Extension`.
- Bodyweight moves, carries and `Deadlift` have no prefix: `Pull-up`, `Plank`, `Farmer's Walk`.
- Singular forms: Curl, Fly, Tricep, Pulldown, Pushdown, Pull-up.

## Fields per exercise
| Field | Values |
|---|---|
| Type | Strength, Cardio |
| Primary muscle (strength) | Abdominals, Abductors, Adductors, Biceps, Calves, Chest, Forearms, Glutes, Hamstrings, Lats, LowerBack, MiddleBack, Neck, Quadriceps, Shoulders, Traps, Triceps |
| Equipment (strength) | Barbell, Dumbbell, EzBar, Cable, Machine, SmithMachine, Kettlebell, Bodyweight, Band, TrapBar, Landmine, Other |
| Mechanic (strength) | Compound, Isolation |

## Activity types
**R** = required, **o** = optional, blank = not shown.

### Strength
| Type | Count | Reps | Weight kg | Duration | Distance |
|---|---|---|---|---|---|
| Lift | 284 | R | o | | |
| Hold (e.g. Plank, Wall Sit, Dead Hang) | 6 | | o | R | |
| Assisted (Machine Assisted Pull-up, Chin-up, Dip) | 3 | R | o, labelled **Assist (kg)** | | |
| Carry (Farmer's Walk, Suitcase Carry, Overhead Carry, Sled Push, Sled Pull, Yoke Walk) | 6 | | o | o | R (metres) |

- On bodyweight moves, weight is extra load.

### Cardio
Duration (h:mm:ss) is required for every cardio exercise. Everything else is optional.

| Name | Distance | Floors | Level (label) | Incline % | Calories | Rate | Pace |
|---|---|---|---|---|---|---|---|
| Treadmill | km | | | o | o | km/h | |
| Exercise Bike | km | | Resistance | | o | RPM | |
| Recumbent Bike | km | | Resistance | | o | RPM | |
| Spin Bike | km | | Resistance | | o | RPM | |
| Air Bike | km | | | | o | RPM | |
| Rowing Machine | metres | | Damper | | o | strokes/min | |
| Ski Erg | metres | | Damper | | o | strokes/min | |
| Cross Trainer | km | | Resistance | o | o | strides/min | |
| Arc Trainer | km | | Resistance | o | o | strides/min | |
| Stair Climber | | o | Speed level | | o | steps/min | |
| VersaClimber | feet | | | | o | feet/min | |
| Outdoor Run | km | | | | o | | min/km |
| Outdoor Walk | km | | | | o | | min/km |
| Outdoor Cycle | km | | | | o | km/h | |
| Swimming | metres | | | | o | | |
| Jump Rope | | | | | o | | |

Stair Climber also covers StepMill.

## Field labels and units
How each metric shows on a set or entry (D25). A label in the exercise file replaces the default. Entry formats: [02](02-data-model.md#entry-formats-ui).

| Metric | Default label | Unit shown |
|---|---|---|
| Reps | Reps | |
| Weight | Weight | kg |
| Duration | Duration | |
| Distance | Distance | km, m, ft |
| Floors | Floors | |
| Level | (always labelled) | |
| Incline | Incline | % |
| Calories | Calories | |
| Rate | Rate | km/h, rpm, strokes/min, strides/min, steps/min, feet/min |
| Pace | Pace | min/km |
