@US-15 @FR-12
Feature: Discard a new workout
  Back asks before dropping a workout I haven't saved.

  Background:
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps

  @D81 @US-28
  Scenario Outline: Back asks "Discard this workout?"
    When I go back with <how>
    Then the prompt "Discard this workout?" shows Discard and Cancel
    And focus is on Cancel
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  Scenario: Cancel keeps the workout as it was
    Given I have gone back with the back arrow
    When I tap Cancel
    Then I am still on New Workout
    And Barbell Back Squat set 1 still shows 5 reps

  @smoke
  Scenario: Discard drops the workout
    Given I have gone back with Android back
    When I tap Discard
    Then I am on the Dashboard
    And the last workout shows "No workouts yet"
    And History is empty

  @D78 @US-28
  Scenario: Android back on the prompt cancels it
    Given I have gone back with the back arrow
    When I press Android back
    Then the prompt is closed
    And I am still on New Workout

  @D45
  Scenario: Back asks even with no exercises
    Given I have removed Barbell Back Squat
    When I go back with the back arrow
    Then the prompt "Discard this workout?" shows Discard and Cancel
