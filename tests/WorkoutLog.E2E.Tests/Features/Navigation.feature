Feature: Navigation
  The tab bar has This week, + and Trends. Android back does what the on-screen back or Cancel does, and a screen returns to where it was opened from.

  Scenario: The tab bar has This week, + and Trends
    Given I am on This week
    Then I can open Trends and start a New Workout from the tab bar

  Scenario: The tab bar switches between This week and Trends
    Given I am on This week
    When I tap Trends in the tab bar
    Then I am on Trends
    When I tap This week in the tab bar
    Then I am on This week

  Scenario Outline: + starts a new workout from either tab
    Given I am on <tab>
    When I tap + in the tab bar
    Then I am on New Workout
    Examples:
      | tab       |
      | This week |
      | Trends    |

  Scenario Outline: Discarding a new workout returns to the tab it was started from
    Given I am on <tab>
    And I have started a new workout
    And I have gone back with Android back
    When I tap Discard
    Then I am on <tab>
    Examples:
      | tab       |
      | This week |
      | Trends    |

  Scenario: Saving a workout started from Trends ends on This week
    Given I am on Trends
    And I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 10 reps
    When I save the workout
    Then I am on This week

  Scenario Outline: Android back from each screen
    Given I am on <screen>
    When I press Android back
    Then <result>
    Examples:
      | screen                                | result                                          |
      | the add-exercise list on Edit Workout | the add-exercise list is closed on Edit Workout |
      | This week                             | the app is in the background                    |
      | Trends                                | I am on This week                               |

  Scenario: Leaving with back and coming back shows This week
    Given I have pressed Android back on This week
    When I open the app again
    Then I am on This week

  Scenario Outline: Workouts has a back arrow and no tab bar, and back returns to Trends
    Given these saved workouts:
      | started         | exercises             |
      | yesterday 18:05 | Push-up: 10 reps 0 kg |
    And I have opened Workouts
    Then the tab bar is hidden
    When I go back from Workouts with <how>
    Then I am on Trends
    And the tab bar is shown
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
