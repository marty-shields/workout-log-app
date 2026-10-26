@US-12 @D105 @D118
Feature: Carry sets
  Carries have more fields, so one set is open and the others are summary lines.

  Background:
    Given I have started a new workout
    And I have added "Farmer's Walk"

  Scenario: A new carry opens its set with every field labelled
    Then Farmer's Walk set 1 is open with labels: Distance (m), Weight (kg), Duration

  Scenario: Add set closes the valid set into a summary and opens the new one
    Given I have logged Farmer's Walk set 1: 40 m, 32 kg, 0:00:45
    When I tap Add set on Farmer's Walk
    Then Farmer's Walk set 1 is a summary "40m · 32kg · 0:00:45"
    And Farmer's Walk set 2 is open

  Scenario: Add set is blocked while the carry's distance is missing
    Given I have logged Farmer's Walk set 1: 32 kg, 0:00:45
    When I tap Add set on Farmer's Walk
    Then Farmer's Walk set 1 shows "Enter distance"
    And focus is on Farmer's Walk set 1 distance

  Scenario: Tapping a summary opens that set and closes the open one
    Given I have logged Farmer's Walk sets: 40 m 32 kg; 50 m 32 kg
    When I tap the summary of Farmer's Walk set 1
    Then Farmer's Walk set 1 is open
    And Farmer's Walk set 2 is a summary "50m · 32kg"

  Scenario: A summary doesn't open while the open set has errors
    Given I have logged Farmer's Walk set 1: 40 m, 32 kg
    And I have tapped Add set on Farmer's Walk
    When I tap the summary of Farmer's Walk set 1
    Then Farmer's Walk set 2 is still open and shows "Enter distance"

  Scenario: Removing the open set leaves every set closed
    Given I have logged Farmer's Walk sets: 40 m 32 kg; 50 m 32 kg
    When I remove Farmer's Walk set 2
    Then every Farmer's Walk set is a summary
