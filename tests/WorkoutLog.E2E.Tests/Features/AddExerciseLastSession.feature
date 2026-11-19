Feature: Add exercise last session
  The add-exercise list shows each logged exercise's last session.

  Scenario: The add-exercise list shows each logged exercise's last session
    Given these saved workouts:
      | started          | exercises                                      |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg; 3 reps 70 kg |
    And I have started a new workout
    When I open the add-exercise list
    Then the add-exercise row for "Barbell Back Squat" reads Last 3 days ago · 70kg × 3
    And the add-exercise row for "Dumbbell Curl" has no last session
