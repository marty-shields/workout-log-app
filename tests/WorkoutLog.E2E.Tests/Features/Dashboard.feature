Feature: This week
  This week shows the days I trained, the week's workouts, this week against last week, my streak and the sets per muscle group. A workout opens for editing.

  @today
  Scenario: The week's workouts are listed newest first with their exercises and how long they took
    Given these saved workouts:
      | started     | finished    | exercises                                                                                               |
      | today 00:00 | today 00:02 | Barbell Bench Press: 8 reps 60 kg / Cable Fly: 12 reps 15 kg / Dumbbell Incline Press: 10 reps 22.5 kg |
      | today 00:03 | today 00:04 | Push-up: 10 reps 0 kg                                                                                   |
    Then this week's workouts read, in order:
      | started     | exercises                         | duration |
      | today 00:03 | Push-up                           | 0:01     |
      | today 00:00 | Barbell Bench Press, Cable Fly +1 | 0:02     |

  @today
  Scenario: The day I trained is marked in the day strip
    Given these saved workouts:
      | started     | finished    | exercises             |
      | today 00:00 | today 00:03 | Push-up: 10 reps 0 kg |
    Then the day strip marks today as trained

  Scenario: A week with no workouts says so and marks no day
    Then this week shows no workouts
    And the day strip marks no day as trained

  Scenario: A workout from before this week isn't listed or counted
    Given these saved workouts:
      | started          | exercises             |
      | 8 days ago 10:00 | Push-up: 10 reps 0 kg |
    Then this week shows no workouts
    And this week shows 0 workouts

  Scenario: A workout dated after now isn't listed or counted
    Given these saved workouts:
      | started        | exercises             |
      | tomorrow 09:00 | Push-up: 10 reps 0 kg |
    Then this week shows no workouts
    And this week shows 0 workouts

  Scenario: An empty week shows last week's totals
    Given these saved workouts:
      | started          | finished         | exercises             |
      | 7 days ago 10:00 | 7 days ago 10:25 | Push-up: 10 reps 0 kg |
    Then this week shows no workouts
    And the Last week row shows "1 workout · 25m · 1 set"

  Scenario: Before the first workout there is no Last week row
    Then no Last week row is shown

  @today
  Scenario Outline: Tapping a workout opens it; back returns to This week
    Given these saved workouts:
      | started     | finished    | exercises                        |
      | today 00:00 | today 00:03 | Barbell Back Squat: 5 reps 60 kg |
    When I tap the newest workout on This week
    Then I am on Edit Workout titled with today's date and day
    When I go back with <how>
    Then I am on This week
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  @today
  Scenario: Deleting a workout opened from This week returns to This week without it
    Given these saved workouts:
      | started     | finished    | exercises                        |
      | today 00:00 | today 00:01 | Barbell Back Squat: 5 reps 60 kg |
      | today 00:02 | today 00:03 | Push-up: 10 reps 0 kg            |
    When I tap the newest workout on This week
    And I tap Delete
    And I tap Delete on the prompt
    Then I am on This week
    And this week's workouts read, in order:
      | started     | exercises          | duration |
      | today 00:00 | Barbell Back Squat | 0:01     |

  @today
  Scenario: Workouts this week are counted
    Given these saved workouts:
      | started           | exercises             |
      | 4 minutes ago     | Push-up: 10 reps 0 kg |
      | 2 minutes ago     | Push-up: 10 reps 0 kg |
      | 40 days ago 10:00 | Push-up: 10 reps 0 kg |
    Then this week shows 2 workouts

  @today
  Scenario: This week so far is compared with last week up to the end of the same day
    Given these saved workouts:
      | started           | finished          | exercises                                                        |
      | today 00:00       | today 00:50       | Push-up: 10 reps 0 kg; 12 reps 0 kg / Treadmill: 0:20:00, 3.2 km |
      | 7 days ago 00:00  | 7 days ago 00:25  | Push-up: 10 reps 0 kg                                            |
      | 7 days ago 00:01  | 7 days ago 00:31  | Push-up: 10 reps 0 kg                                            |
      | 14 days ago 12:00 | 14 days ago 12:20 | Push-up: 10 reps 0 kg                                            |
    Then This week vs last week shows:
      | row      | this week | change    |
      | Workouts | 1         | down 1    |
      | Time     | 50m       | down 5m   |
      | Sets     | 2         | no change |

  @today
  Scenario: A workout later in the day than now, on the same day last week, is in last week's total
    Given these saved workouts:
      | started          | finished         | exercises             |
      | today 00:00      | today 00:30      | Push-up: 10 reps 0 kg |
      | 7 days ago 23:59 | 6 days ago 00:19 | Push-up: 10 reps 0 kg |
    Then This week vs last week shows:
      | row      | this week | change    |
      | Workouts | 1         | no change |
      | Time     | 30m       | up 10m    |
      | Sets     | 1         | no change |

  @today
  Scenario: The streak counts the weeks in a row with a workout, this week included
    Given these saved workouts:
      | started           | exercises             |
      | today 00:00       | Push-up: 10 reps 0 kg |
      | 7 days ago 00:00  | Push-up: 10 reps 0 kg |
      | 14 days ago 00:00 | Push-up: 10 reps 0 kg |
      | 14 days ago 00:01 | Push-up: 10 reps 0 kg |
      | 28 days ago 00:00 | Push-up: 10 reps 0 kg |
    Then the streak shows "3-week streak"

  @today
  Scenario: Sets per muscle group counts this week's strength sets, most first
    Given these saved workouts:
      | started          | finished         | exercises                                            |
      | 8 days ago 10:00 | 8 days ago 10:30 | Dumbbell Curl: 10 reps 12 kg                         |
      | today 00:00      | today 00:02      | Push-up: 20 reps 0 kg / Outdoor Run: 0:30:00, 5.2 km |
      | today 00:03      | today 00:04      | Barbell Back Squat: 5 reps 60 kg; 5 reps 60 kg       |
    Then Sets per muscle group reads, in order:
      | group     | sets        |
      | Legs      | 2 sets      |
      | Chest     | 1 set       |
      | Back      | Not trained |
      | Shoulders | Not trained |
      | Arms      | Not trained |
      | Core      | Not trained |

  Scenario Outline: This week links to Workouts and New Workout
    When I tap <link> on This week
    Then I am on <screen>
    Examples:
      | link        | screen      |
      | Workouts    | Workouts    |
      | New Workout | New Workout |
