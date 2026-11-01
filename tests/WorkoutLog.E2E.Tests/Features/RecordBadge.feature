Feature: Record badge
  On New Workout, a set or cardio entry that beats my best for the exercise shows a Record badge, on the one giving this session's best.

  Scenario: A set heavier than every saved workout shows the badge
    Given these saved workouts:
      | started          | exercises                                        |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 70 kg                 |
      | yesterday 18:05  | Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg |
    And I have started a new workout
    And I have added "Barbell Back Squat"
    When I log Barbell Back Squat set 1: 5 reps, 75 kg
    Then Barbell Back Squat set 1 has the Record badge

  Scenario: A lighter set shows no badge
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 70 kg |
    And I have started a new workout
    And I have added "Barbell Back Squat"
    When I log Barbell Back Squat set 1: 5 reps, 60 kg
    Then Barbell Back Squat set 1 has no Record badge

  Scenario: The first session of an exercise shows no badge
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 70 kg |
    And I have started a new workout
    And I have added "Barbell Bench Press"
    When I log Barbell Bench Press set 1: 5 reps, 100 kg
    Then Barbell Bench Press set 1 has no Record badge

  Scenario: A tie shows no badge
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 70 kg |
    And I have started a new workout
    And I have added "Barbell Back Squat"
    When I log Barbell Back Squat set 1: 5 reps, 70 kg
    Then Barbell Back Squat set 1 has no Record badge

  Scenario: The badge moves to a later, better set
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 70 kg |
    And I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat sets: 5 reps 75 kg; 5 reps 72.5 kg
    Then Barbell Back Squat set 1 has the Record badge
    And Barbell Back Squat set 2 has no Record badge
    When I enter "80" in Barbell Back Squat set 2 weight
    Then Barbell Back Squat set 2 has the Record badge
    And Barbell Back Squat set 1 has no Record badge

  Scenario: The badge goes when the value drops below the best
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 70 kg |
    And I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps, 75 kg
    When I enter "65" in Barbell Back Squat set 1 weight
    Then Barbell Back Squat set 1 has no Record badge

  Scenario: A cardio entry that beats the best distance shows the badge
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Outdoor Run: 0:30:00, 5 km |
    And I have started a new workout
    And I have added "Outdoor Run"
    When I log Outdoor Run: 0:30:00, 5.5 km
    Then Outdoor Run has the Record badge

  Scenario: A cardio entry that doesn't beat the best shows no badge
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Outdoor Run: 0:30:00, 5 km |
    And I have started a new workout
    And I have added "Outdoor Run"
    When I log Outdoor Run: 0:30:00, 4.5 km
    Then Outdoor Run has no Record badge

  Scenario: Edit Workout shows no badge
    Given these saved workouts:
      | started          | exercises                        |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg |
      | yesterday 18:05  | Barbell Back Squat: 5 reps 70 kg |
    When I open the latest workout from Workouts
    Then Barbell Back Squat set 1 has no Record badge
