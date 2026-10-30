@US-20 @US-21 @US-22
Feature: Dashboard
  The Dashboard shows when I last trained, how long it took and what I did, opens that workout, this week against last week, and my last 12 weeks.

  Scenario: The last workout is the one with the latest start
    Given these saved workouts:
      | started           | exercises                  |
      | 10 days ago 09:00 | Barbell Back Squat: 5 reps 60 kg |
      | 3 days ago 18:05  | Barbell Back Squat: 5 reps 60 kg |
    Then the last workout shows the date and day of "3 days ago"

  Scenario: The last workout shows how long ago, how long it took and what I did
    Given these saved workouts:
      | started          | finished         | exercises                                                                                           |
      | 3 days ago 18:05 | 3 days ago 19:17 | Barbell Bench Press: 8 reps 60 kg / Cable Fly: 12 reps 15 kg / Dumbbell Incline Press: 10 reps 22.5 kg |
    Then the last workout shows the date and day of "3 days ago"
    And the last workout's time shows "3 days ago · 1:12:00"
    And the last workout's exercises show "Barbell Bench Press, Cable Fly +1"

  Scenario Outline: Tapping the last workout opens it; back returns to the Dashboard
    Given these saved workouts:
      | started         | exercises                        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg |
    When I tap the last workout on the Dashboard
    Then I am on Edit Workout titled with yesterday's date and day
    When I go back with <how>
    Then I am on the Dashboard
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  @today
  Scenario: Workouts this week are counted
    Given these saved workouts:
      | started           | exercises        |
      | 3 hours ago       | Push-up: 10 reps 0 kg |
      | 2 hours ago       | Push-up: 10 reps 0 kg |
      | 40 days ago 10:00 | Push-up: 10 reps 0 kg |
    Then this week shows 2 workouts

  @today
  Scenario: A workout from before this week isn't counted this week
    Given these saved workouts:
      | started          | exercises        |
      | 8 days ago 10:00 | Push-up: 10 reps 0 kg |
    Then this week shows 0 workouts

  Scenario: A workout dated after now is the last workout but isn't counted
    Given these saved workouts:
      | started        | exercises        |
      | tomorrow 09:00 | Push-up: 10 reps 0 kg |
    Then the last workout shows the date and day of "tomorrow"
    And this week shows 0 workouts

  @today
  Scenario: This week is compared with last week up to the same day and time
    Given these saved workouts:
      | started          | finished         | exercises                                                       |
      | today 00:00      | today 00:50      | Push-up: 10 reps 0 kg; 12 reps 0 kg / Treadmill: 0:20:00, 3.2 km |
      | 7 days ago 00:00 | 7 days ago 00:25 | Push-up: 10 reps 0 kg                                           |
      | 7 days ago 00:01 | 7 days ago 00:31 | Push-up: 10 reps 0 kg                                           |
      | 7 days ago 23:59 | 6 days ago 00:20 | Push-up: 10 reps 0 kg                                           |
    Then This week vs last week shows:
      | row      | this week | last week | change    |
      | Workouts | 1         | 2         | down 1    |
      | Time     | 50m       | 55m       | down 5m   |
      | Sets     | 2         | 2         | no change |

  Scenario: The last 12 weeks and the streak, seeded across three weeks
    Given these saved workouts:
      | started           | exercises             |
      | today 00:00       | Push-up: 10 reps 0 kg |
      | 7 days ago 00:00  | Push-up: 10 reps 0 kg |
      | 14 days ago 00:00 | Push-up: 10 reps 0 kg |
      | 14 days ago 00:01 | Push-up: 10 reps 0 kg |
      | 28 days ago 00:00 | Push-up: 10 reps 0 kg |
    Then the last 12 weeks show:
      | week        | workouts   |
      | This week   | 1 workout  |
      | Last week   | 1 workout  |
      | 2 weeks ago | 2 workouts |
      | 3 weeks ago | 0 workouts |
      | 4 weeks ago | 1 workout  |
    And the streak shows "3-week streak"

  Scenario Outline: The Dashboard links to Workouts and New Workout
    When I tap <link> on the Dashboard
    Then I am on <screen>
    Examples:
      | link        | screen      |
      | Workouts    | Workouts    |
      | New Workout | New Workout |
