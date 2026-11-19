Feature: Launch
  The app opens on This week, with its content clear of the status bar and the navigation bar, and the tab bar showing This week, + and Trends.

  @smoke @release
  Scenario: A fresh install opens on an empty This week
    Then I am on This week titled "Workout Log"
    And this week shows no workouts
    And this week shows 0 workouts
    And no streak is shown
    And I can open Trends and start a New Workout from the tab bar
    And no startup error is shown

  Scenario: The title and the tabs clear the system bars
    Given I am on This week
    Then the screen title is below the status bar and the tabs are above the navigation bar
