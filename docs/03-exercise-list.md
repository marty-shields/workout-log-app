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
| Category | Lift, Bodyweight, Assisted, Hold, Carry (strength); Cardio |
| Primary muscle (strength) | Abdominals, Abductors, Adductors, Biceps, Calves, Chest, Forearms, Glutes, Hamstrings, Lats, LowerBack, MiddleBack, Neck, Quadriceps, Shoulders, Traps, Triceps |
| Equipment (strength) | Barbell, Dumbbell, EzBar, Cable, Machine, SmithMachine, Kettlebell, None, Band, TrapBar, Landmine, Other |
| Mechanic (strength) | Compound, Isolation |
| Loaded | true on Bulgarian Split Squat, Calf Raise, Donkey Calf Raise, Glute Bridge, Lateral Lunge, Lunge, Reverse Lunge, Squat, Step-up and Walking Lunge (all Bodyweight); false on every other exercise (D193) |

## Activity types
**R** = required, **o** = optional, blank = not shown. Every value is more than 0 except weight and incline, which can be 0. Limits and decimal places: [02](02-data-model.md#metrics).

### Strength
| Category | Count | Reps | Weight kg | Duration | Distance |
|---|---|---|---|---|---|
| Lift | 228 | R | R, typed; 0 allowed | | |
| Bodyweight (moves where weight is extra load, e.g. Push-up, Pull-up, Ab Wheel Rollout, Band Pull-Apart) | 56 | R | R; a blank becomes 0 (D23) | | |
| Hold (e.g. Plank, Wall Sit, Dead Hang) | 6 | | R; a blank becomes 0 | R | |
| Assisted (Machine Assisted Pull-up, Chin-up, Dip) | 3 | R | R, labelled **Assist (kg)**; a blank becomes 0 | | |
| Carry (Farmer's Walk, Suitcase Carry, Overhead Carry, Sled Push, Sled Pull, Yoke Walk) | 6 | | R; a blank becomes 0 | R | R (metres) |

- 0 kg is unweighted, or no assist. On a bodyweight move, hold or carry it shows as no weight; on a lift or assisted machine as 0kg.
- Equipment None means no equipment. A Bodyweight exercise may use a band, wheel or ball; a plate or harness exercise is a Lift.

### Cardio
Duration (h:mm:ss) is required for every cardio exercise, and distance on every one that has it. Everything else is optional. Pace and speed are worked out from distance and duration, never typed (D197).

| Name | Distance (R) | Floors | Level (label) | Incline % | Calories | Rate | Worked out |
|---|---|---|---|---|---|---|---|
| Treadmill | km | | | o | o | | km/h |
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
| Outdoor Run | km | | | | o | | pace min/km |
| Outdoor Walk | km | | | | o | | pace min/km |
| Outdoor Cycle | km | | | | o | | km/h |
| Swimming | metres | | | | o | | |
| Jump Rope | | | | | o | | |

Stair Climber also covers StepMill.

## Field labels and units
How each metric shows on a set or entry (D25). Every required field shows `*`. A label in the exercise file replaces the default. Entry formats: [02](02-data-model.md#entry-formats-ui).

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
| Rate | Rate | rpm, strokes/min, strides/min, steps/min, feet/min |

Pace and speed show as a line under the fields, not a field: *Pace 5:46/km*, *Speed 10.4 km/h*, or *Pace —* / *Speed —* until distance and duration are both filled and more than 0 (D197).
