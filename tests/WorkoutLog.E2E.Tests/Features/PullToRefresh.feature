Feature: Pull to refresh
  Pulling a top-level screen down from the top reads its data again.

  Scenario: Pulling down This week reloads it
    Given I am on This week
    When I pull down the list
    Then the log shows "Loaded 0 workouts for Week from" once more

  Scenario: Pulling down Workouts reloads it
    Given I have saved a workout with Push-up: 10 reps
    And I am on Workouts
    When I pull down the list
    Then the log shows "Counted" once more

  Scenario: Pulling down Trends reloads it
    Given I am on Trends
    When I pull down the list
    Then the log shows "records for Week from" once more
