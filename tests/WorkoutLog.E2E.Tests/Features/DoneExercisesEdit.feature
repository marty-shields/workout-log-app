Feature: Mark an exercise done on Edit Workout
  A saved workout opens with every exercise as one line; the pencil opens one.

  Background:
    Given these saved workouts:
      | started         | finished        | exercises                                                       |
      | yesterday 18:05 | yesterday 18:52 | Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg / Treadmill: 0:20:00, 3.2 km |
    And I have opened workout 1 from Workouts

  Scenario: Every exercise starts as one line
    Then every exercise is shown as one line

  Scenario: The pencil opens one exercise with its saved values
    When I tap the pencil on Barbell Back Squat
    Then Barbell Back Squat sets are: 5 reps 60 kg; 5 reps 62.5 kg
    And Treadmill is shown as one line with its name, the pencil and the trash

  Scenario: Opening and closing an exercise is not a change
    When I tap the pencil on Barbell Back Squat
    And I tap Done on Barbell Back Squat
    And I go back with the back arrow
    Then I am on Workouts with no prompt
