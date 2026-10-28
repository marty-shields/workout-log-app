@FR-60 @FR-35 @D191
Feature: Exercises
  The exercises I've logged, latest first, with when I last did them and that session's best.

  Scenario: Exercises with nothing logged says so
    When I open Exercises
    Then Exercises shows "No exercises logged yet"

  @smoke
  Scenario: Rows are latest first, same workout A–Z, with the last session's best and the kind
    Given these saved workouts:
      | started            | exercises                                                                   |
      | 400 days ago 10:00 | Dumbbell Curl: 10 reps 12 kg                                                |
      | 5 days ago 18:05   | Barbell Back Squat: 8 reps 80 kg / Push-up: 20 reps                         |
      | 3 days ago 18:05   | Barbell Back Squat: 5 reps 60 kg; 3 reps 70 kg; 3 reps 70 kg / Plank: 0:01:45 |
      | yesterday 07:30    | Outdoor Run: 0:30:00, 5.2 km / Farmer's Walk: 40 m 32 kg 0:00:45             |
    When I open Exercises
    Then the Exercises rows read, in order:
      | exercise           | last                    | kind     |
      | Farmer's Walk      | yesterday · 32kg · 40m  | Strength |
      | Outdoor Run        | yesterday · 5.2km       | Cardio   |
      | Barbell Back Squat | 3 days ago · 70kg × 3   | Strength |
      | Plank              | 3 days ago · 1:45       | Strength |
      | Push-up            | 5 days ago · 20 reps    | Strength |
      | Dumbbell Curl      | 400 days ago · 12kg × 10 | Strength |

  Scenario: A lift with no weight shows the date only
    Given these saved workouts:
      | started          | exercises                  |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps |
    When I open Exercises
    Then the Exercises rows read, in order:
      | exercise           | last       | kind     |
      | Barbell Back Squat | 3 days ago | Strength |

  Scenario: Search ranks the matches among the logged exercises
    Given these saved workouts:
      | started          | exercises                                                                          |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg / Rowing Machine: 0:10:00 / Dumbbell Curl: 10 reps 12 kg |
    And I have opened Exercises
    When I search Exercises for "barbell"
    Then Exercises lists, in order: Barbell Back Squat

  Scenario Outline: A chip shows only that kind
    Given these saved workouts:
      | started          | exercises                                                                          |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg / Rowing Machine: 0:10:00 / Dumbbell Curl: 10 reps 12 kg |
    And I have opened Exercises
    When I choose the <kind> chip on Exercises
    Then Exercises lists, in order: <exercises>
    Examples:
      | kind     | exercises                         |
      | Strength | Barbell Back Squat, Dumbbell Curl |
      | Cardio   | Rowing Machine                    |

  Scenario: A search with no match says so
    Given these saved workouts:
      | started          | exercises                        |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg |
    And I have opened Exercises
    When I search Exercises for "zumba"
    Then Exercises says "No exercises match" under the search

  Scenario: The add-exercise list shows each logged exercise's last session
    Given these saved workouts:
      | started          | exercises                                      |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg; 3 reps 70 kg |
    And I have started a new workout
    When I open the add-exercise list
    Then the add-exercise row for "Barbell Back Squat" reads Last 3 days ago · 70kg × 3
    And the add-exercise row for "Dumbbell Curl" has no last session

  @US-28
  Scenario Outline: Back returns to the Dashboard
    Given I have opened Exercises
    When I go back with <how>
    Then I am on the Dashboard
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
