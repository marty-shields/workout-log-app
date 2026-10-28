@FR-15 @D48
Feature: Last and PB weight
  On New Workout, a strength exercise I add shows the best weight from the last saved workout with it and from all of them.

  Scenario: One saved workout: each exercise shows only its own weights
    Given these saved workouts:
      | started         | exercises                                                                                    |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg / Barbell Bench Press: 5 reps 80 kg; 3 reps 85 kg |
    And I have started a new workout
    When I add "Barbell Back Squat"
    And I add "Barbell Bench Press"
    And I add "Dumbbell Curl"
    Then Barbell Back Squat has last and PB "Last 62.5kg · PB 62.5kg"
    And Barbell Bench Press has last and PB "Last 85kg · PB 85kg"
    And Dumbbell Curl has no last and PB

  Scenario: Many saved workouts, the PB in an older one
    Given these saved workouts:
      | started            | exercises                                         |
      | 10 days ago 18:05  | Barbell Back Squat: 5 reps 65 kg; 3 reps 70 kg    |
      | 3 days ago 18:05   | Barbell Back Squat: 5 reps 67.5 kg               |
      | yesterday 18:05    | Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg |
    And I have started a new workout
    When I add "Barbell Back Squat"
    Then Barbell Back Squat has last and PB "Last 62.5kg · PB 70kg"

  Scenario: The last workout has no weight
    Given these saved workouts:
      | started           | exercises                       |
      | 3 days ago 18:05  | Barbell Back Squat: 5 reps 70 kg |
      | yesterday 18:05   | Barbell Back Squat: 5 reps       |
    And I have started a new workout
    When I add "Barbell Back Squat"
    Then Barbell Back Squat has last and PB "Last — · PB 70kg"

  Scenario: An assisted machine's PB is the lowest assist
    Given these saved workouts:
      | started           | exercises                                           |
      | 3 days ago 18:05  | Machine Assisted Pull-up: 8 reps 20 kg              |
      | yesterday 18:05   | Machine Assisted Pull-up: 8 reps 30 kg; 6 reps 25 kg |
    And I have started a new workout
    When I add "Machine Assisted Pull-up"
    Then Machine Assisted Pull-up has last and PB "Last 25kg · PB 20kg"

  Scenario: Edit Workout shows no last and PB
    Given these saved workouts:
      | started           | exercises                       |
      | 3 days ago 18:05  | Barbell Back Squat: 5 reps 70 kg |
      | yesterday 18:05   | Barbell Back Squat: 5 reps 60 kg |
    When I open the latest workout from Workouts
    Then Barbell Back Squat has no last and PB
