@D198 @FR-04 @FR-06
Feature: Workout limits
  A workout has at most 10 exercises, and a strength exercise at most 10 sets.

  Scenario: At 10 sets Add set is off with a note
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg; 5 reps 60 kg |
    When I open workout 1 from Workouts
    Then Add set on Barbell Back Squat is off with "At most 10 sets"

  Scenario: At 10 exercises Add exercise is off with a note
    Given these saved workouts:
      | started         | exercises |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg / Barbell Bench Press: 8 reps 60 kg / Dumbbell Curl: 10 reps 12 kg / Push-up: 20 reps 0 kg / Pull-up: 8 reps 0 kg / Plank: 0:01:00 0 kg / Farmer's Walk: 40 m 32 kg 0:00:45 / Machine Assisted Pull-up: 8 reps 20 kg / Treadmill: 0:20:00, 3.2 km / Outdoor Run: 0:30:00 5.2 km |
    When I open workout 1 from Workouts
    Then Add exercise is off with "At most 10 exercises"