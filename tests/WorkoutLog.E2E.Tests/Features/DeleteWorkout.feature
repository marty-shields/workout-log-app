@US-19 @FR-24
Feature: Delete a workout
  Delete asks first, then the workout is gone for good.

  Background:
    Given these saved workouts:
      | started          | exercises        |
      | 3 days ago 10:00 | Push-up: 10 reps 0 kg |
      | yesterday 18:05  | Push-up: 12 reps 0 kg |
    And I have opened workout 2 from Workouts

  @D81
  Scenario: Delete asks "Delete this workout?"
    When I tap Delete
    Then the prompt "Delete this workout?" shows Delete and Cancel
    And focus is on Cancel

  Scenario: Cancel keeps the workout
    Given I have tapped Delete
    When I tap Cancel
    Then I am still on Edit Workout
    And Workouts still lists 2 workouts

  @smoke @US-08
  Scenario: Confirming removes the workout
    Given I have tapped Delete
    When I tap Delete on the prompt
    Then I am on Workouts
    And Workouts lists only workout 1

  @D46
  Scenario: Delete with unsaved edits removes the workout too
    Given I have entered "15" in Push-up set 1 reps
    When I tap Delete
    And I tap Delete on the prompt
    Then I am on Workouts
    And Workouts lists only workout 1

  Scenario: Deleting the latest workout updates the Dashboard
    Given I have deleted workout 2
    When I go back to the Dashboard
    Then the last workout shows the date and day of "3 days ago"

  Scenario: Deleting the only workout empties Workouts and the Dashboard
    Given I have deleted workout 2
    And I have deleted workout 1
    Then Workouts shows "No workouts yet"
    And on the Dashboard the last workout shows "No workouts yet", This week 0 and This month 0

  @D78 @US-28
  Scenario: Android back on the prompt cancels it
    Given I have tapped Delete
    When I press Android back
    Then the prompt is closed
    And I am still on Edit Workout
