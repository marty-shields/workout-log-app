@US-20 @US-21 @US-22
Feature: Dashboard
  The Dashboard shows when I last trained and how often this week and month.

  Scenario: The last workout is the one with the latest start
    Given these saved workouts:
      | started           | exercises                  |
      | 10 days ago 09:00 | Barbell Back Squat: 5 reps 60 kg |
      | 3 days ago 18:05  | Barbell Back Squat: 5 reps 60 kg |
    Then the last workout shows the date and day of "3 days ago"

  @today
  Scenario: Workouts this week and this month are counted
    Given these saved workouts:
      | started           | exercises        |
      | 3 hours ago       | Push-up: 10 reps 0 kg |
      | 2 hours ago       | Push-up: 10 reps 0 kg |
      | 40 days ago 10:00 | Push-up: 10 reps 0 kg |
    Then This week shows 2 and This month shows 2

  @today
  Scenario: A workout from before this week isn't counted this week
    Given these saved workouts:
      | started          | exercises        |
      | 8 days ago 10:00 | Push-up: 10 reps 0 kg |
    Then This week shows 0
    And This month counts it only if it started this month

  Scenario: A workout dated after now is the last workout but isn't counted
    Given these saved workouts:
      | started        | exercises        |
      | tomorrow 09:00 | Push-up: 10 reps 0 kg |
    Then the last workout shows the date and day of "tomorrow"
    And This week shows 0 and This month shows 0

  Scenario Outline: The Dashboard links to Workouts and New Workout
    When I tap <link> on the Dashboard
    Then I am on <screen>
    Examples:
      | link        | screen      |
      | Workouts    | Workouts    |
      | New Workout | New Workout |
