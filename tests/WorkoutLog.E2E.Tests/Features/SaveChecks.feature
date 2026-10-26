@US-09 @US-13 @US-14 @FR-09
Feature: Save checks
  Save only keeps a complete workout and shows what's wrong.

  Background:
    Given I have started a new workout

  @smoke @D171
  Scenario: Save with no exercises is blocked
    When I save the workout
    Then the workout shows "Add at least one exercise"
    And focus is on Add exercise
    And I am still on New Workout

  @D108
  Scenario: Adding an exercise clears the workout error
    Given I have saved the workout
    When I add "Barbell Back Squat"
    Then the workout error is gone

  Scenario: An exercise with no sets is blocked and Add set clears it
    Given I have added "Barbell Back Squat"
    And I have removed Barbell Back Squat set 1
    When I save the workout
    Then Barbell Back Squat shows "Add at least one set"
    And focus is on Barbell Back Squat Add set
    When I tap Add set on Barbell Back Squat
    Then Barbell Back Squat shows no exercise error

  @D171 @D236
  Scenario: Save shows every error and focuses the first
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have added "Machine Assisted Pull-up"
    And I have logged Machine Assisted Pull-up set 1: 8 reps, 1001 kg
    And I have added "Plank"
    And I have logged Plank set 1: 0:00:00
    When I save the workout
    Then Machine Assisted Pull-up set 1 shows "Assist must be 1,000 or less"
    And Plank set 1 shows "Duration must be more than 0"
    And focus is on Plank set 1 duration hours
    And I am still on New Workout

  Scenario: Fixing every error lets the workout save
    Given I have added "Plank"
    And I have logged Plank set 1: 0:00:00
    And I have saved the workout
    When I log Plank set 1: 0:01:00
    And I save the workout
    Then I am on the Dashboard

  Scenario: A blocked Save keeps nothing
    Given I have saved the workout
    When I discard the workout
    Then the last workout shows "No workouts yet"
