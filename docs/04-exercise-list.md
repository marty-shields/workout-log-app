# 04 — Exercise List

The list ships with the app and changes only through an app update. The full list is [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json): 315 exercises (299 strength, 16 cardio). It is Core's embedded definition file.

## Source
- Strength exercises are based on [free-exercise-db](https://github.com/yuhonas/free-exercise-db) (public domain), strength and powerlifting categories only. Some exercises were added by hand.
- Names were checked against muscleandstrength.com for gaps. Only names are used; no descriptions or images.
- Martin's 44 must-have exercises are all included, under the names agreed for them (see D79).
- Cardio and carries were written by hand.

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
- Each exercise can have nicknames for search, e.g. Machine Chest Fly → Pec Deck, Butterfly.

## Fields per exercise
| Field | Values |
|---|---|
| Type | Strength, Cardio |
| Primary muscle (strength) | Abdominals, Abductors, Adductors, Biceps, Calves, Chest, Forearms, Glutes, Hamstrings, Lats, LowerBack, MiddleBack, Neck, Quadriceps, Shoulders, Traps, Triceps |
| Equipment (strength) | Barbell, Dumbbell, EzBar, Cable, Machine, SmithMachine, Kettlebell, Bodyweight, Band, TrapBar, Landmine, Other |
| Mechanic (strength) | Compound, Isolation |

Muscle, equipment and mechanic are stored but not shown or filtered on in v1.

## Activity types
**R** = required, **o** = optional, blank = not shown.

### Strength
| Type | Count | Reps | Weight kg | Duration | Distance |
|---|---|---|---|---|---|
| Lift | 284 | R | o | | |
| Hold (e.g. Plank, Wall Sit, Dead Hang) | 6 | | o | R | |
| Assisted (Machine Assisted Pull-up, Chin-up, Dip) | 3 | R | o, labelled **Assist (kg)** | | |
| Carry (Farmer's Walk, Suitcase Carry, Overhead Carry, Sled Push, Sled Pull, Yoke Walk) | 6 | | o | o | R (metres) |

- Dumbbell weight is per dumbbell.
- On bodyweight moves, weight is extra load.

### Cardio
Duration (h:mm:ss) is required for every cardio exercise. Everything else is optional.

| Name | Distance | Floors | Level (label) | Incline % | Calories | Rate |
|---|---|---|---|---|---|---|
| Treadmill | km | | | o | o | km/h |
| Exercise Bike | km | | Resistance | | o | RPM |
| Recumbent Bike | km | | Resistance | | o | RPM |
| Spin Bike | km | | Resistance | | o | RPM |
| Air Bike | km | | | | o | RPM |
| Rowing Machine | metres | | Damper | | o | strokes/min |
| Ski Erg | metres | | Damper | | o | strokes/min |
| Cross Trainer | km | | Resistance | o | o | strides/min |
| Arc Trainer | km | | Resistance | o | o | strides/min |
| Stair Climber | | o | Speed level | | o | steps/min |
| VersaClimber | feet | | | | o | feet/min |
| Outdoor Run | km | | | | o | min/km |
| Outdoor Walk | km | | | | o | min/km |
| Outdoor Cycle | km | | | | o | km/h |
| Swimming | metres | | | | o | |
| Jump Rope | | | | | o | |

Stair Climber also covers StepMill.

## Field labels and units
How each metric shows on a set or entry (D149). A label in the exercise file replaces the default.

| Metric | Default label | Unit shown | Entered as |
|---|---|---|---|
| Reps | Reps | | whole number |
| Weight | Weight | kg | decimal |
| Duration | Duration | | h:mm:ss |
| Distance | Distance | km, m, ft | decimal |
| Floors | Floors | | whole number |
| Level | (always labelled) | | whole number |
| Incline | Incline | % | decimal |
| Calories | Calories | | whole number |
| Rate | Rate | km/h, rpm, strokes/min, strides/min, steps/min, feet/min | decimal |
| Rate in min/km | Pace | min/km | m:ss |
