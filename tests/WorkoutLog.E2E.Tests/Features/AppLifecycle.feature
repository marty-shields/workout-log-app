Feature: App lifecycle
  Saved data stays; an unsaved workout doesn't.

  @smoke @today @US-08
  Scenario: Saved workouts survive closing and reopening the app
    Given I have saved a workout with Barbell Back Squat: 5 reps 60 kg
    When I close the app and open it again
    Then the last workout shows today's date and day
    And reopening the latest workout shows Barbell Back Squat set 1 at 5 reps, 60 kg

  @FR-12 @US-08 @US-15
  Scenario: A new workout is lost when the app is killed
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps
    When I close the app and open it again
    Then I am on the Dashboard
    And the last workout shows "No workouts yet"

  @D197 @US-20
  Scenario: The Dashboard reloads when the app comes back
    Given I am on the Dashboard
    When I send the app to the background and bring it back
    Then the log shows "Activity started" once more
    And the log shows "Dashboard loaded" once more

  @D197 @US-20
  Scenario: The Dashboard reloads each time it is shown
    Given I am on the Dashboard
    When I open History and go back
    Then the log shows "Dashboard loaded" once more
