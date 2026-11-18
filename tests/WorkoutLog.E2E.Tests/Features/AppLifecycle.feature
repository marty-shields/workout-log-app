Feature: App lifecycle
  Saved data stays; an unsaved workout doesn't.

  @smoke @release @today
  Scenario: Saved workouts survive closing and reopening the app
    Given I have saved a workout with Barbell Back Squat: 5 reps 60 kg
    When I close the app and open it again
    Then this week's newest workout is from today
    And reopening the latest workout shows Barbell Back Squat set 1 at 5 reps, 60 kg

  Scenario: A new workout is lost when the app is killed
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps
    When I close the app and open it again
    Then I am on This week
    And this week shows no workouts

  Scenario: This week reloads when the app comes back
    Given I am on This week
    When I send the app to the background and bring it back
    Then the log shows "Activity started" once more
    And the log shows "workouts for Week from" once more

  Scenario: Workouts reloads when the app comes back
    Given I am on Workouts
    When I send the app to the background and bring it back
    Then the log shows "Activity started" once more
    And the log shows "Counted" once more

  Scenario: Exercises reloads when the app comes back
    Given I am on Exercises
    When I send the app to the background and bring it back
    Then the log shows "Activity started" once more
    And the log shows "logged exercises" once more

  Scenario: This week reloads each time it is shown
    Given I am on This week
    When I open Workouts and go back
    Then the log shows "workouts for Week from" once more
