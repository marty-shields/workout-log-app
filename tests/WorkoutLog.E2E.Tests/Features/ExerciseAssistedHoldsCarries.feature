@FR-63 @FR-60 @D195
Feature: Exercise page: assisted machines, holds and carries
  An assisted machine's least assist, a hold's duration and weight, and a carry's weight and distance over time, with their records and sessions.

  Scenario Outline: An assisted machine, hold or carry opens its Exercise page and back returns to Exercises
    Given these saved workouts:
      | started          | exercises    |
      | 3 days ago 18:05 | <exercise>   |
    And I have opened Exercises
    Then the tappable rows on Exercises are: <name>
    When I open "<name>" on Exercises
    Then the Exercise page is titled "<name>" with "<subtitle>"
    When I go back with the back arrow
    Then I am on Exercises
    Examples:
      | exercise                                  | name                     | subtitle          |
      | Machine Assisted Pull-up: 8 reps 10 kg    | Machine Assisted Pull-up | Lats · Machine    |
      | Plank: 0:01:00                            | Plank                    | Abdominals        |
      | Farmer's Walk: 40 m 32 kg 0:00:45         | Farmer's Walk            | Forearms · Other  |

  Scenario: An assisted machine shows its least assist per session, a blank assist as 0kg
    Given these saved workouts:
      | started           | exercises                                                    |
      | 10 days ago 18:05 | Machine Assisted Pull-up: 8 reps 10 kg; 6 reps 5 kg          |
      | 3 days ago 18:05  | Machine Assisted Pull-up: 8 reps 5 kg; 4 reps 0 kg; 5 reps   |
    And I have opened the Exercise page for "Machine Assisted Pull-up"
    Then the Exercise page has no tab switcher
    And the Exercise chart is titled "Least assist per session"
    And the Exercise chart reads:
      | day         | value  |
      | 10 days ago | 5kg ×6 |
      | 3 days ago  | 0kg ×5 |
    And the Exercise caption is "Up = less assist"
    And the Exercise records read:
      | record       | value   | day        |
      | Least assist | 0kg × 5 | 3 days ago |
    And the Exercise sessions read:
      | day         | headline | sets                  |
      | 3 days ago  | 0kg × 5  | 5kg×8 · 0kg×4 · 0kg×5 |
      | 10 days ago | 5kg × 6  | 10kg×8 · 5kg×6        |

  Scenario: An assisted machine with no sessions in the range says so, and a longer range adds them
    Given these saved workouts:
      | started            | exercises                              |
      | 120 days ago 18:05 | Machine Assisted Pull-up: 8 reps 20 kg |
    And I have opened the Exercise page for "Machine Assisted Pull-up"
    Then the Exercise chart says "No sessions in the last 3 months"
    And the Exercise records read:
      | record       | value | day |
      | Least assist | —     |     |
    And the Exercise page lists no sessions
    When I choose the 1Y range
    Then the Exercise chart reads:
      | day          | value   |
      | 120 days ago | 20kg ×8 |
    And the Exercise records read:
      | record       | value    | day          |
      | Least assist | 20kg × 8 | 120 days ago |

  Scenario: A hold opens on Duration with its longest hold per session, and a longer range adds older sessions
    Given these saved workouts:
      | started            | exercises                               |
      | 200 days ago 18:05 | Plank: 0:03:00                          |
      | 10 days ago 18:05  | Plank: 0:01:30                          |
      | 3 days ago 18:05   | Plank: 0:01:45; 10 kg 0:01:00; 0:01:20  |
    And I have opened the Exercise page for "Plank"
    Then the Exercise chart reads:
      | day         | value |
      | 10 days ago | 1:30  |
      | 3 days ago  | 1:45  |
    And the Exercise records read:
      | record          | value       | day        |
      | Longest hold    | 1:45        | 3 days ago |
      | Heaviest weight | 10kg · 1:00 | 3 days ago |
    And the Exercise sessions read:
      | day         | headline | sets                    |
      | 3 days ago  | 1:45     | 1:45 · 10kg 1:00 · 1:20 |
      | 10 days ago | 1:30     | 1:30                    |
    When I choose the 1Y range
    Then the Exercise chart reads:
      | day          | value |
      | 200 days ago | 3:00  |
      | 10 days ago  | 1:30  |
      | 3 days ago   | 1:45  |
    And the Exercise records read:
      | record          | value       | day          |
      | Longest hold    | 3:00        | 200 days ago |
      | Heaviest weight | 10kg · 1:00 | 3 days ago   |

  Scenario: A hold's Weight tab shows the heaviest set per session, a tie taking the longest
    Given these saved workouts:
      | started           | exercises                                       |
      | 10 days ago 18:05 | Plank: 5 kg 0:00:45                             |
      | 3 days ago 18:05  | Plank: 10 kg 0:00:45; 10 kg 0:01:00; 0:02:00    |
    And I have opened the Exercise page for "Plank"
    When I choose the Weight tab
    Then the Exercise chart reads:
      | day         | value     |
      | 10 days ago | 5kg 0:45  |
      | 3 days ago  | 10kg 1:00 |

  Scenario: A hold's Weight tab with no weighted sets says so
    Given these saved workouts:
      | started          | exercises                    |
      | 3 days ago 18:05 | Plank: 0:01:00; 0 kg 0:01:30 |
    And I have opened the Exercise page for "Plank"
    When I choose the Weight tab
    Then the Exercise chart says "No sets in the last 3 months"
    And the Exercise records read:
      | record          | value | day        |
      | Longest hold    | 1:30  | 3 days ago |
      | Heaviest weight | —     |            |
    And the Exercise sessions read:
      | day        | headline | sets        |
      | 3 days ago | 1:30     | 1:00 · 1:30 |
    When I choose the All range
    Then the Exercise chart says "No sets yet"

  Scenario: A carry opens on Weight with its heaviest set per session, a tie taking the longest, and a longer range adds older sessions
    Given these saved workouts:
      | started            | exercises                                                     |
      | 200 days ago 18:05 | Farmer's Walk: 40 m 50 kg                                     |
      | 10 days ago 18:05  | Farmer's Walk: 40 m 28 kg                                     |
      | 3 days ago 18:05   | Farmer's Walk: 30 m 32 kg 0:00:30; 40 m 32 kg 0:00:45; 60 m   |
    And I have opened the Exercise page for "Farmer's Walk"
    Then the Exercise chart reads:
      | day         | value     |
      | 10 days ago | 28kg 40m  |
      | 3 days ago  | 32kg 40m  |
    And the Exercise records read:
      | record           | value      | day        |
      | Heaviest weight  | 32kg · 40m | 3 days ago |
      | Longest distance | 60m        | 3 days ago |
    And the Exercise sessions read:
      | day         | headline   | sets                                |
      | 3 days ago  | 32kg · 40m | 32kg 30m 0:30 · 32kg 40m 0:45 · 60m |
      | 10 days ago | 28kg · 40m | 28kg 40m                            |
    When I choose the 1Y range
    Then the Exercise chart reads:
      | day          | value    |
      | 200 days ago | 50kg 40m |
      | 10 days ago  | 28kg 40m |
      | 3 days ago   | 32kg 40m |
    And the Exercise records read:
      | record           | value      | day          |
      | Heaviest weight  | 50kg · 40m | 200 days ago |
      | Longest distance | 60m        | 3 days ago   |

  Scenario: A carry's Distance tab shows the longest set per session, a tie taking the heaviest
    Given these saved workouts:
      | started           | exercises                               |
      | 10 days ago 18:05 | Farmer's Walk: 60 m 24 kg; 60 m 28 kg   |
      | 3 days ago 18:05  | Farmer's Walk: 80 m; 40 m 32 kg         |
    And I have opened the Exercise page for "Farmer's Walk"
    When I choose the Distance tab
    Then the Exercise chart reads:
      | day         | value    |
      | 10 days ago | 60m 28kg |
      | 3 days ago  | 80m      |
    And the Exercise records read:
      | record           | value      | day         |
      | Heaviest weight  | 32kg · 40m | 3 days ago  |
      | Longest distance | 80m        | 3 days ago  |

  Scenario: A carry's Weight tab with no weighted sets says so
    Given these saved workouts:
      | started          | exercises                       |
      | 3 days ago 18:05 | Farmer's Walk: 60 m; 40 m 0 kg  |
    And I have opened the Exercise page for "Farmer's Walk"
    Then the Exercise chart says "No sets in the last 3 months"
    And the Exercise records read:
      | record           | value | day        |
      | Heaviest weight  | —     |            |
      | Longest distance | 60m   | 3 days ago |
    And the Exercise sessions read:
      | day        | headline | sets      |
      | 3 days ago | 60m      | 60m · 40m |
    When I choose the All range
    Then the Exercise chart says "No sets yet"
