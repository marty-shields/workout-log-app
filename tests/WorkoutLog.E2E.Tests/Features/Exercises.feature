Feature: Exercises
  Exercises is no longer a screen of its own: the add-exercise list shows each logged exercise's last session, and an exercise's page opens from Trends.

  Scenario: The tab bar has This week, + and Trends
    Given I am on This week
    Then I can open Trends and start a New Workout from the tab bar

  Scenario: The add-exercise list shows each logged exercise's last session
    Given these saved workouts:
      | started          | exercises                                      |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg; 3 reps 70 kg |
    And I have started a new workout
    When I open the add-exercise list
    Then the add-exercise row for "Barbell Back Squat" reads Last 3 days ago · 70kg × 3
    And the add-exercise row for "Dumbbell Curl" has no last session
