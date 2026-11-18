Feature: Pull to refresh
  Pulling a top-level screen down from the top reads its data again.

  Scenario: Pulling down This week reloads it
    Given I am on This week
    When I pull down the list
    Then the log shows "workouts for Week from" once more

  Scenario: Pulling down Workouts reloads it
    Given I am on Workouts
    When I pull down the list
    Then the log shows "Counted" once more

  Scenario: Pulling down Exercises reloads it
    Given I am on Exercises
    When I pull down the list
    Then the log shows "logged exercises" once more
