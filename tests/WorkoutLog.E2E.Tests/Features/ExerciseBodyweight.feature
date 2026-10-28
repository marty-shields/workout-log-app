@FR-62 @FR-60 @D194
Feature: Exercise page for bodyweight moves
  A bodyweight move's best set, total reps, top set and, on a loaded move, estimated one-rep max over time, with its records and sessions.

  Scenario Outline: A bodyweight move opens its Exercise page and back returns to Exercises
    Given these saved workouts:
      | started          | exercises                                                          |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg / Push-up: 20 reps / Plank: 0:01:00 |
    And I have opened Exercises
    Then the tappable rows on Exercises are: Barbell Back Squat, Plank, Push-up
    When I open "Push-up" on Exercises
    Then the Exercise page is titled "Push-up" with "Chest"
    And the Exercise tabs are: Best set, Total, Top set
    When I go back with <how>
    Then I am on Exercises
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  @smoke
  Scenario: Best set and Total count the unweighted sets, and a weighted set heads its session
    Given these saved workouts:
      | started           | exercises                                    |
      | 10 days ago 18:05 | Push-up: 25 reps; 20 reps; 18 reps           |
      | 3 days ago 18:05  | Push-up: 20 reps; 22 reps 0 kg; 8 reps 10 kg |
    And I have opened the Exercise page for "Push-up"
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 25    |
      | 3 days ago  | 22    |
    And the Exercise records read:
      | record                 | value    | day         |
      | Most reps in a set     | 25 reps  | 10 days ago |
      | Most reps in a session | 63 reps  | 10 days ago |
      | Heaviest weight        | 10kg × 8 | 3 days ago  |
    And the Exercise sessions read:
      | day         | headline | sets           |
      | 3 days ago  | 10kg × 8 | 20 · 22 · 10×8 |
      | 10 days ago | 25 reps  | 25 · 20 · 18   |
    When I choose the Total tab
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 63    |
      | 3 days ago  | 42    |
    When I choose the Top set tab
    Then the Exercise chart reads:
      | day        | value   |
      | 3 days ago | 10kg ×8 |

  @D193
  Scenario: A loaded move adds e1RM from its weighted sets
    Given these saved workouts:
      | started           | exercises                                                    |
      | 10 days ago 18:05 | Bulgarian Split Squat: 12 reps                               |
      | 3 days ago 18:05  | Bulgarian Split Squat: 10 reps 14 kg; 8 reps 16 kg; 15 reps |
    And I have opened the Exercise page for "Bulgarian Split Squat"
    Then the Exercise tabs are: Best set, Total, Top set, e1RM
    When I choose the e1RM tab
    Then the Exercise chart reads:
      | day        | value  |
      | 3 days ago | 20.3kg |
    And the Exercise caption is "Estimated one-rep max"
    And the Exercise records read:
      | record                 | value    | day        |
      | Most reps in a set     | 15 reps  | 3 days ago |
      | Most reps in a session | 15 reps  | 3 days ago |
      | Heaviest weight        | 16kg × 8 | 3 days ago |
      | Best e1RM              | 20.3kg   | 3 days ago |

  Scenario: A tab with no values says there are no sets
    Given these saved workouts:
      | started          | exercises        |
      | 3 days ago 18:05 | Push-up: 20 reps |
    And I have opened the Exercise page for "Push-up"
    When I choose the Top set tab
    Then the Exercise chart says "No sets in the last 3 months"
    And the Exercise records read:
      | record                 | value   | day        |
      | Most reps in a set     | 20 reps | 3 days ago |
      | Most reps in a session | 20 reps | 3 days ago |
      | Heaviest weight        | —       |            |
    When I choose the All range
    Then the Exercise chart says "No sets yet"
