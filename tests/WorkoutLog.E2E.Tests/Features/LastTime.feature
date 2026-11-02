Feature: Last time's values
  While logging, each exercise shows what its last saved workout had, set by set. It is display only: every value is still typed.

  Scenario: Each set shows the same set from the latest workout
    Given these saved workouts:
      | started           | exercises                                                      |
      | 10 days ago 18:05 | Barbell Back Squat: 5 reps 50 kg; 5 reps 50 kg                 |
      | yesterday 18:05   | Barbell Back Squat: 8 reps 60 kg; 8 reps 60 kg; 6 reps 62.5 kg |
    And I have started a new workout
    When I add "Barbell Back Squat"
    Then Barbell Back Squat set 1 last time is "60×8"

  Scenario: A set past the end of last time's sets shows a dash
    Given these saved workouts:
      | started         | exercises                                         |
      | yesterday 18:05 | Barbell Back Squat: 8 reps 60 kg; 6 reps 62.5 kg |
    And I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have tapped Add set on Barbell Back Squat
    And I have logged Barbell Back Squat set 2: 8 reps, 60 kg
    And I have tapped Add set on Barbell Back Squat
    Then Barbell Back Squat set 2 last time is "62.5×6"
    And Barbell Back Squat set 3 last time is "—"

  Scenario: Last time is never copied into a field
    Given these saved workouts:
      | started         | exercises                          |
      | yesterday 18:05 | Barbell Back Squat: 8 reps 60 kg |
    And I have started a new workout
    When I add "Barbell Back Squat"
    Then Barbell Back Squat set 1 last time is "60×8"
    And Barbell Back Squat set 1 weight and reps are empty

  Scenario: An exercise with no earlier workout shows nothing
    Given I have started a new workout
    When I add "Barbell Back Squat"
    Then Barbell Back Squat has no last time

  Scenario: A cardio entry shows last time's entry
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Treadmill: 0:20:00, 3.2 km       |
    And I have started a new workout
    When I add "Treadmill"
    Then Treadmill entry last time is "3.2km · 20:00 · 9.6 km/h"

  Scenario Outline: A hold shows its weight only when one was logged
    Given these saved workouts:
      | started         | exercises             |
      | yesterday 18:05 | Plank: <logged>       |
    And I have started a new workout
    When I add "Plank"
    Then Plank set 1 last time is "<shown>"
    Examples:
      | logged         | shown       |
      | 0:01:30 20 kg  | 20kg · 1:30 |
      | 0:01:30 0 kg   | 1:30        |

  Scenario: Edit Workout shows the last workout started before this one once an exercise is opened
    Given these saved workouts:
      | started           | exercises                       |
      | 4 days ago 18:05  | Barbell Back Squat: 5 reps 50 kg |
      | 2 days ago 18:05  | Barbell Back Squat: 5 reps 60 kg |
      | yesterday 18:05   | Barbell Back Squat: 5 reps 70 kg |
    And I have opened workout 2 from Workouts
    Then Barbell Back Squat has no last time
    When I tap the pencil on Barbell Back Squat
    Then Barbell Back Squat set 1 last time is "50×5"
