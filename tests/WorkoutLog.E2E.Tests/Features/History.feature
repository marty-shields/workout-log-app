@US-16 @FR-20 @FR-21
Feature: History
  My saved workouts, newest first.

  Scenario: History with no workouts says so
    When I open History
    Then History shows "No workouts yet"

  Scenario: Rows are newest first with date, day and start time
    Given these saved workouts:
      | started           | exercises        |
      | 10 days ago 07:30 | Push-up: 10 reps |
      | 3 days ago 18:05  | Push-up: 10 reps |
      | yesterday 12:45   | Push-up: 10 reps |
    When I open History
    Then the rows read, in order:
      | yesterday 12:45   |
      | 3 days ago 18:05  |
      | 10 days ago 07:30 |

  @D70
  Scenario: Workouts with the same start show the newest saved first
    Given these saved workouts:
      | started         | exercises        |
      | yesterday 18:05 | Push-up: 10 reps |
      | yesterday 18:05 | Push-up: 12 reps |
    When I open History
    Then workout 2 is above workout 1

  @D70
  Scenario: Every workout loads at once
    Given 40 saved workouts on different days
    When I open History
    Then History lists 40 workouts

  @smoke @D71
  Scenario: Tapping a row opens that workout
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps |
    When I open History
    And I tap the row for yesterday 18:05
    Then I am on Edit Workout titled with yesterday's date and day

  @US-28
  Scenario Outline: Back returns to the Dashboard
    Given I have opened History
    When I go back with <how>
    Then I am on the Dashboard
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
