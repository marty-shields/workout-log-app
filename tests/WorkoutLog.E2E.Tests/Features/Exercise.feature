@FR-61 @FR-60 @D192
Feature: Exercise page
  A lift's top set and estimated one-rep max over time, with its records and sessions.

  Scenario Outline: A lift opens its Exercise page and back returns to Exercises
    Given these saved workouts:
      | started          | exercises                                       |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg / Plank: 0:01:00 |
    And I have opened Exercises
    Then the tappable rows on Exercises are: Barbell Back Squat
    When I open "Barbell Back Squat" on Exercises
    Then the Exercise page is titled "Barbell Back Squat" with "Quadriceps · Barbell"
    When I go back with <how>
    Then I am on Exercises
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  @smoke
  Scenario: Top set shows each session's heaviest set, the records and every session
    Given these saved workouts:
      | started           | exercises                                              |
      | 10 days ago 18:05 | Barbell Back Squat: 8 reps 60 kg; 8 reps 62.5 kg       |
      | 3 days ago 18:05  | Barbell Back Squat: 8 reps 60 kg; 3 reps 70 kg; 8 reps |
    And I have opened the Exercise page for "Barbell Back Squat"
    Then the Exercise chart reads:
      | day         | value     |
      | 10 days ago | 62.5kg ×8 |
      | 3 days ago  | 70kg ×3   |
    And the Exercise records read:
      | record          | value    | day         |
      | Heaviest weight | 70kg × 3 | 3 days ago  |
      | Best e1RM       | 79.2kg   | 10 days ago |
    And the Exercise sessions read:
      | day         | headline   | sets                 |
      | 3 days ago  | 70kg × 3   | 60×8 · 70×3 · 8 reps |
      | 10 days ago | 62.5kg × 8 | 60×8 · 62.5×8        |

  Scenario: e1RM uses sets of 12 reps or fewer, and a longer range adds older sessions
    Given these saved workouts:
      | started            | exercises                                       |
      | 200 days ago 18:05 | Barbell Back Squat: 1 reps 100 kg               |
      | 3 days ago 18:05   | Barbell Back Squat: 15 reps 80 kg; 5 reps 70 kg |
    And I have opened the Exercise page for "Barbell Back Squat"
    When I choose the e1RM tab
    Then the Exercise chart reads:
      | day        | value  |
      | 3 days ago | 81.7kg |
    And the Exercise caption is "Estimated one-rep max"
    When I choose the 1Y range
    Then the Exercise chart reads:
      | day          | value  |
      | 200 days ago | 100kg  |
      | 3 days ago   | 81.7kg |
    And the Exercise records read:
      | record          | value     | day          |
      | Heaviest weight | 100kg × 1 | 200 days ago |
      | Best e1RM       | 100kg     | 200 days ago |

  Scenario: No sessions in the range says so
    Given these saved workouts:
      | started            | exercises                        |
      | 120 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg |
    And I have opened the Exercise page for "Barbell Back Squat"
    Then the Exercise chart says "No sessions in the last 3 months"
    And the Exercise records read:
      | record          | value | day |
      | Heaviest weight | —     |     |
      | Best e1RM       | —     |     |
    And the Exercise page lists no sessions
