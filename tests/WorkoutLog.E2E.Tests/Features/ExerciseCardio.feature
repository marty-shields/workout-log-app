Feature: Exercise page: cardio
  A cardio exercise's distance, duration, pace, floors and calories over time, with its records and sessions.

  Scenario Outline: A cardio exercise opens its Exercise page and back returns to Exercises
    Given these saved workouts:
      | started          | exercises                                                  |
      | 3 days ago 18:05 | Outdoor Run: 0:30:00 5.2 km / Barbell Back Squat: 5 reps 60 kg |
    And I have opened Exercises
    Then the tappable rows on Exercises are: Barbell Back Squat, Outdoor Run
    When I open "Outdoor Run" on Exercises
    Then the Exercise page is titled "Outdoor Run" with "Cardio"
    And the Exercise tabs are: Distance, Duration, Pace, Calories
    When I go back with <how>
    Then I am on Exercises
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  @smoke
  Scenario: Distance shows each session's distance, the records and every session
    Given these saved workouts:
      | started           | exercises                                       |
      | 10 days ago 18:05 | Outdoor Run: 0:29:20 5 km             |
      | 3 days ago 18:05  | Outdoor Run: 0:30:00 5.2 km 410 calories |
    And I have opened the Exercise page for "Outdoor Run"
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 5km   |
      | 3 days ago  | 5.2km |
    And the Exercise records read:
      | record           | value        | day        |
      | Longest distance | 5.2km        | 3 days ago |
      | Longest duration | 30:00        | 3 days ago |
      | Fastest pace     | 5:46/km      | 3 days ago |
      | Most calories    | 410 calories | 3 days ago |
    And the Exercise sessions read:
      | day         | headline | sets                           |
      | 3 days ago  | 5.2km    | 30:00 · 5:46/km · 410 calories |
      | 10 days ago | 5km      | 29:20 · 5:52/km                |

  Scenario: A longer range adds older sessions
    Given these saved workouts:
      | started            | exercises                             |
      | 120 days ago 18:05 | Outdoor Run: 0:25:00 4 km   |
      | 3 days ago 18:05   | Outdoor Run: 0:30:00 5.2 km |
    And I have opened the Exercise page for "Outdoor Run"
    Then the Exercise chart reads:
      | day        | value |
      | 3 days ago | 5.2km |
    When I choose the 1Y range
    Then the Exercise chart reads:
      | day          | value |
      | 120 days ago | 4km   |
      | 3 days ago   | 5.2km |
    And the Exercise records read:
      | record           | value   | day          |
      | Longest distance | 5.2km   | 3 days ago   |
      | Longest duration | 30:00   | 3 days ago   |
      | Fastest pace     | 5:46/km | 3 days ago   |
      | Most calories    | —       |              |
    And the Exercise sessions read:
      | day          | headline | sets            |
      | 3 days ago   | 5.2km    | 30:00 · 5:46/km |
      | 120 days ago | 4km      | 25:00 · 6:15/km |

  Scenario: Pace is lower is better, and a tab with no values says so
    Given these saved workouts:
      | started           | exercises                            |
      | 10 days ago 18:05 | Outdoor Run: 0:29:20 5 km  |
      | 3 days ago 18:05  | Outdoor Run: 0:30:00 5.2 km |
    And I have opened the Exercise page for "Outdoor Run"
    When I choose the Pace tab
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 5:52  |
      | 3 days ago  | 5:46  |
    And the Exercise caption is "Up = faster"
    When I choose the Calories tab
    Then the Exercise chart says "No calories logged in the last 3 months"

  Scenario: An exercise with no distance opens on Duration and shows Floors, level and rate
    Given these saved workouts:
      | started           | exercises                                                       |
      | 10 days ago 18:05 | Stair Climber: 0:20:00 100 floors                               |
      | 3 days ago 18:05  | Stair Climber: 0:25:00 120 floors level 8 300 calories 70 steps/min |
    And I have opened the Exercise page for "Stair Climber"
    Then the Exercise tabs are: Duration, Floors, Calories
    And the Exercise page is on the Duration tab
    When I choose the Floors tab
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 100   |
      | 3 days ago  | 120   |
    And the Exercise records read:
      | record           | value        | day        |
      | Longest duration | 25:00        | 3 days ago |
      | Most floors      | 120 floors   | 3 days ago |
      | Most calories    | 300 calories | 3 days ago |
    And the Exercise sessions read:
      | day         | headline | sets                                                     |
      | 3 days ago  | 25:00    | 120 floors · 300 calories · Speed level 8 · 70 steps/min |
      | 10 days ago | 20:00    | 100 floors                                               |

  Scenario: A treadmill's speed is worked out, faster is better
    Given these saved workouts:
      | started           | exercises                    |
      | 10 days ago 18:05 | Treadmill: 0:30:00, 4.8 km   |
      | 3 days ago 18:05  | Treadmill: 0:30:00, 5.2 km   |
    And I have opened the Exercise page for "Treadmill"
    Then the Exercise tabs are: Distance, Duration, Speed, Calories
    When I choose the Speed tab
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 9.6   |
      | 3 days ago  | 10.4  |
    And the Exercise records read:
      | record           | value        | day        |
      | Longest distance | 5.2km        | 3 days ago |
      | Longest duration | 30:00        | 10 days ago |
      | Fastest speed    | 10.4 km/h    | 3 days ago |
      | Most calories    | —            |             |
    And the Exercise sessions read:
      | day         | headline | sets              |
      | 3 days ago  | 5.2km    | 30:00 · 10.4 km/h |
      | 10 days ago | 4.8km    | 30:00 · 9.6 km/h  |
