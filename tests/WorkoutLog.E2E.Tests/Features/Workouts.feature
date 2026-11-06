Feature: Workouts
  My saved workouts, newest first. The screen opens on the last 3 months; 3M, 6M, 1Y, All and the calendar change the dates.

  Scenario: Workouts with no workouts says so
    When I open Workouts
    Then Workouts shows "No workouts yet"

  Scenario: Rows are newest first with date, day and start time
    Given these saved workouts:
      | started           | exercises        |
      | 10 days ago 07:30 | Push-up: 10 reps 0 kg |
      | 3 days ago 18:05  | Push-up: 10 reps 0 kg |
      | yesterday 12:45   | Push-up: 10 reps 0 kg |
    When I open Workouts
    Then the rows read, in order:
      | yesterday 12:45   |
      | 3 days ago 18:05  |
      | 10 days ago 07:30 |

  Scenario: Every workout in the last 3 months loads at once
    Given 40 saved workouts on different days
    When I open Workouts
    Then Workouts lists 40 workouts

  Scenario: Workouts opens on the last 3 months, and All lists the older ones
    Given these saved workouts:
      | started            | exercises             |
      | 125 days ago 18:00 | Push-up: 10 reps 0 kg |
      | yesterday 18:05    | Push-up: 10 reps 0 kg |
    When I open Workouts
    Then the rows read, in order:
      | yesterday 18:05 |
    When I tap All in Workouts
    Then the rows read, in order:
      | yesterday 18:05    |
      | 125 days ago 18:00 |

  Scenario: 1Y lists a workout from 8 months ago
    Given these saved workouts:
      | started            | exercises             |
      | 400 days ago 18:00 | Push-up: 10 reps 0 kg |
      | 240 days ago 18:00 | Push-up: 10 reps 0 kg |
      | yesterday 18:05    | Push-up: 10 reps 0 kg |
    When I open Workouts
    And I tap 1Y in Workouts
    Then the rows read, in order:
      | yesterday 18:05    |
      | 240 days ago 18:00 |

  Scenario: Picking one day lists only that day's workouts
    Given these saved workouts:
      | started          | exercises             |
      | 5 days ago 18:00 | Push-up: 10 reps 0 kg |
      | 3 days ago 07:30 | Push-up: 10 reps 0 kg |
      | 3 days ago 18:05 | Push-up: 10 reps 0 kg |
      | yesterday 18:05  | Push-up: 10 reps 0 kg |
    When I open Workouts
    And I pick the day 3 days ago in the calendar
    Then the rows read, in order:
      | 3 days ago 18:05 |
      | 3 days ago 07:30 |

  @smoke
  Scenario: Tapping a row opens that workout
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg |
    When I open Workouts
    And I tap the row for yesterday 18:05
    Then I am on Edit Workout titled with yesterday's date and day

  Scenario Outline: Android back, or Dashboard in the navigation bar, returns to the Dashboard
    Given I have opened Workouts
    When I <leave>
    Then I am on the Dashboard
    Examples:
      | leave                               |
      | press Android back                  |
      | tap Dashboard in the navigation bar |

  Scenario: Exercises in the navigation bar opens Exercises, and Android back then returns to the Dashboard
    Given I have opened Workouts
    When I tap Exercises in the navigation bar
    Then I am on Exercises
    When I press Android back
    Then I am on the Dashboard
