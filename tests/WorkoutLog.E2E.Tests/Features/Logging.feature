@US-35 @D135
Feature: Logging
  The log records what happened, never what I typed.

  Scenario: Startup is logged
    Then the log shows "started on Android API "
    And the log shows "Startup finished in"
    And the log shows "Activity created"

  @US-10 @today
  Scenario: Starting and saving a workout are logged with ids and counts
    Given I have started a new workout
    And I have logged Barbell Back Squat set 1: 5 reps
    And I have logged Push-up set 1: 10 reps
    When I save the workout
    Then the log shows "New workout started"
    And the log shows "Workout 1 added with 2 exercises"
    And the log shows "Dashboard loaded: 1 this week, 1 this month"

  @US-15
  Scenario: Discarding a new workout is logged
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    When I discard the workout
    Then the log shows "New workout discarded with 1 exercises"

  @US-17 @US-18
  Scenario: Opening, saving and discarding edits are logged
    Given these saved workouts:
      | started         | exercises                                     |
      | yesterday 18:05 | Barbell Back Squat: 5 reps / Push-up: 10 reps |
    When I open workout 1 from History
    And I save the workout
    And I open workout 1 from History
    And I tap Add set on Push-up
    And I discard the changes
    Then the log shows "Workout 1 loaded with 2 exercises"
    And the log shows "Workout 1 updated with 2 exercises"
    And the log shows "Edits to workout 1 discarded"

  @D46 @US-19
  Scenario: Deleting with unsaved edits logs the delete only
    Given these saved workouts:
      | started         | exercises        |
      | yesterday 18:05 | Push-up: 10 reps |
    And I have opened workout 1 from History
    And I have entered "12" in Push-up set 1 reps
    When I delete the workout
    Then the log shows "Workout 1 deleted"
    And the log doesn't show "Edits to workout 1 discarded"

  @smoke
  Scenario: Values I type never reach the log
    Given I have started a new workout
    And I have logged Barbell Back Squat set 1: 5 reps, 987.65 kg
    And I have logged Treadmill: 0:20:00, 876.54 km, incline 87.65, 76.54 km/h
    When I save the workout
    And I open the latest workout from History and save it
    Then the log contains none of: 987.65, 876.54, 87.65, 76.54

  @US-12 @US-13
  Scenario: Validation errors aren't logged
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have entered "60.255" in Barbell Back Squat set 1 weight
    And I have tapped Add set on Barbell Back Squat
    And I have added "Treadmill"
    And I have saved the workout
    Then the log contains none of: must be, Enter reps, Enter duration, decimal places
