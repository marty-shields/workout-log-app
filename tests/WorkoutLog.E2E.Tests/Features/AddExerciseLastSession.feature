Feature: Add exercise last session
  The add-exercise list shows each logged exercise's last session, and lists the exercises with a saved workout first.

  Scenario: The add-exercise list shows each logged exercise's last session
    Given these saved workouts:
      | started          | exercises                                      |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg; 3 reps 70 kg |
    And I have started a new workout
    When I open the add-exercise list
    Then the add-exercise row for "Barbell Back Squat" reads Last 3 days ago · 70kg × 3
    And the add-exercise row for "Dumbbell Curl" has no last session

  Scenario: Exercises with a saved workout are listed first under Recent Exercises, most recent first
    Given these saved workouts:
      | started          | exercises                                       |
      | 9 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg               |
      | 3 days ago 18:05 | Dumbbell Curl: 8 reps 14 kg                    |
    And I have started a new workout
    When I open the add-exercise list
    Then the add-exercise list has the groups: Recent Exercises, Other Exercises
    And the Recent Exercises group starts with: Dumbbell Curl, Barbell Back Squat
    And every exercise is listed once

  Scenario: With nothing logged the list is flat with no headers
    Given I have started a new workout
    When I open the add-exercise list
    Then the add-exercise list shows no group headers

  Scenario: A done exercise is in the first group when searching for it
    Given these saved workouts:
      | started          | exercises                                    |
      | 3 days ago 18:05 | Machine Bicep Curl: 10 reps 30 kg           |
    And I have started a new workout
    When I search the add-exercise list for "bicep curl"
    Then the Recent Exercises group starts with: Machine Bicep Curl

  Scenario: A done exercise is above a better text match that has not been done
    Given these saved workouts:
      | started          | exercises                         |
      | 3 days ago 18:05 | Dumbbell Curl: 8 reps 14 kg      |
    And I have started a new workout
    When I search the add-exercise list for "bicep curl"
    Then the Recent Exercises group starts with: Dumbbell Curl
    And the Other Exercises group starts with "Machine Bicep Curl"

  Scenario: A chip with no done exercises leaves only the Other Exercises group
    Given these saved workouts:
      | started          | exercises                                   |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg           |
    And I have started a new workout
    And I have chosen the Cardio chip
    Then the add-exercise list has the groups: Other Exercises
