@US-10
Feature: New workout
  Opening New Workout starts the workout and its timer.

  @smoke @today
  Scenario: A new workout shows today's date and a running timer
    When I start a new workout
    Then the title is today's date and day
    And the timer shows "Elapsed" and counts up
    And the workout shows "No exercises yet"

  Scenario: The timer keeps running while I log
    Given I have started a new workout
    When I add "Barbell Back Squat"
    And I log Barbell Back Squat set 1: 5 reps, 60 kg
    Then the timer shows the time since I started
