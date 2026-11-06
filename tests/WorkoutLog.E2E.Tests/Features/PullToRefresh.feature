Feature: Pull to refresh
  Pulling a top-level screen down from the top reads its data again.

  Scenario: Pulling down the Dashboard reloads it
    Given I am on the Dashboard
    When I pull down the list
    Then the log shows "Last workout loaded" once more

  Scenario: Pulling down Exercises reloads it
    Given I am on Exercises
    When I pull down the list
    Then the log shows "logged exercises" once more
