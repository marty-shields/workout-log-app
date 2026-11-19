Feature: Workouts
  Workouts opens from Trends for the dates I tapped, newest first. 3M, 6M, 1Y, All and the calendar change the dates, and back returns to Trends.

  Scenario: Rows are newest first with date, day and start time
    Given these saved workouts:
      | started           | exercises             |
      | 10 days ago 07:30 | Push-up: 10 reps 0 kg |
      | 3 days ago 18:05  | Push-up: 10 reps 0 kg |
      | yesterday 12:45   | Push-up: 10 reps 0 kg |
    When I open Workouts
    Then the rows read, in order:
      | yesterday 12:45   |
      | 3 days ago 18:05  |
      | 10 days ago 07:30 |

  Scenario: Every workout loads at once
    Given 40 saved workouts on different days
    When I open Workouts
    Then Workouts lists 40 workouts

  @today
  Scenario: Opened from a day in Trends it lists only that day, and All lists the older ones
    Given these saved workouts:
      | started            | exercises             |
      | 125 days ago 18:00 | Push-up: 10 reps 0 kg |
      | today 00:00        | Push-up: 10 reps 0 kg |
    And I have opened Trends
    When I open the workouts of today from the Trends week view
    Then the rows read, in order:
      | today 00:00 |
    When I tap All in Workouts
    Then the rows read, in order:
      | today 00:00        |
      | 125 days ago 18:00 |

  @today
  Scenario: 1Y lists a workout from 8 months ago
    Given these saved workouts:
      | started            | exercises             |
      | 400 days ago 18:00 | Push-up: 10 reps 0 kg |
      | 240 days ago 18:00 | Push-up: 10 reps 0 kg |
      | today 00:00        | Push-up: 10 reps 0 kg |
    And I have opened Trends
    When I open the workouts of today from the Trends week view
    And I tap 1Y in Workouts
    Then the rows read, in order:
      | today 00:00        |
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
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg |
    When I open Workouts
    And I tap the row for yesterday 18:05
    Then I am on Edit Workout titled with yesterday's date and day

  Scenario Outline: Back from a workout opened from Workouts returns to Workouts, and back again to Trends
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg |
    And I have opened workout 1 from Workouts
    When I go back with <how>
    Then I am on Workouts
    When I go back from Workouts with <how>
    Then I am on Trends
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
