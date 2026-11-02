Feature: Workout Summary
  Saving a new workout shows what I did and what improved; Done or Android back returns to the Dashboard. Saving an edit goes straight to the Dashboard.

  @smoke @release
  Scenario: Saving a new workout opens the summary and Done returns to the Dashboard
    Given I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 10 reps
    When I save the new workout
    Then I am on the Workout Summary
    And the summary shows 1 exercise and 1 set
    And the summary says it is my first workout
    When I tap Done
    Then I am on the Dashboard

  Scenario: Android back on the summary returns to the Dashboard
    Given I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 10 reps
    When I save the new workout
    And I go back with Android back
    Then I am on the Dashboard

  Scenario: Beating last time shows a record and the difference
    Given I have saved a workout with Push-up: 10 reps
    And I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 12 reps
    When I save the new workout
    Then the summary lists a record for "Push-up"
    And "Push-up" changed by "2 reps" since last time

  Scenario: Matching last time is not a record
    Given I have saved a workout with Push-up: 10 reps
    And I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 10 reps
    When I save the new workout
    Then the summary lists no record for "Push-up"

  Scenario: Saving an edit goes to the Dashboard
    Given I have saved a workout with Push-up: 10 reps
    And I have opened workout 1 from Workouts
    When I enter "12" in Push-up set 1 reps
    And I save the workout
    Then I am on the Dashboard
