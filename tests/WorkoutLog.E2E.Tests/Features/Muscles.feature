Feature: Sets per muscle group
  Strength sets per muscle group in the last 7 days, opened from the Dashboard.

  @today
  Scenario: Each group shows its sets in the last 7 days, and the rest are Not trained
    Given these saved workouts:
      | started          | exercises                                                     |
      | 7 days ago 23:59 | Dumbbell Curl: 10 reps 12 kg                                  |
      | 6 days ago 00:00 | Barbell Back Squat: 5 reps 60 kg; 5 reps 60 kg                |
      | yesterday 18:05  | Push-up: 20 reps 0 kg / Outdoor Run: 0:30:00, 5.2 km          |
    When I open Sets per muscle group from the Dashboard
    Then Sets per muscle group reads, in order:
      | group     | sets        |
      | Chest     | 1 set       |
      | Back      | Not trained |
      | Shoulders | Not trained |
      | Arms      | Not trained |
      | Legs      | 2 sets      |
      | Core      | Not trained |

  Scenario: Nothing in the last 7 days shows every group Not trained
    Given these saved workouts:
      | started           | exercises                    |
      | 10 days ago 10:00 | Dumbbell Curl: 10 reps 12 kg |
    When I open Sets per muscle group from the Dashboard
    Then Sets per muscle group reads, in order:
      | group     | sets        |
      | Chest     | Not trained |
      | Back      | Not trained |
      | Shoulders | Not trained |
      | Arms      | Not trained |
      | Legs      | Not trained |
      | Core      | Not trained |

  Scenario Outline: Back returns to the Dashboard
    Given I have opened Sets per muscle group from the Dashboard
    When I go back with <how>
    Then I am on the Dashboard
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
