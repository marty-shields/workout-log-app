@US-04 @US-23 @FR-30
Feature: Launch
  The app opens on the Dashboard.

  @smoke
  Scenario: A fresh install opens on the empty Dashboard
    Then I am on the Dashboard titled "Workout Log"
    And the last workout shows "No workouts yet"
    And This week shows 0 and This month shows 0
    And I can open Workouts and start a New Workout
    And no startup error is shown
