Feature: Recent records
  The Dashboard shows the records I've set in the last 30 days: the latest per exercise, newest first, up to 3.

  Scenario: Records from the last 30 days, latest per exercise, newest first
    Given these saved workouts:
      | started           | exercises                                                                                          |
      | 60 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg / Push-up: 20 reps 0 kg / Plank: 0:01:00 0 kg / Outdoor Run: 0:30:00, 5 km |
      | 40 days ago 18:05 | Barbell Back Squat: 5 reps 80 kg                                                                   |
      | 10 days ago 18:05 | Outdoor Run: 0:30:00, 5.2 km                                                                       |
      | 5 days ago 18:05  | Plank: 0:01:45 0 kg / Push-up: 20 reps 0 kg                                                        |
      | 2 days ago 18:05  | Barbell Back Squat: 5 reps 70 kg / Push-up: 25 reps 0 kg                                           |
      | yesterday 18:05   | Outdoor Run: 0:20:00, 5.5 km                                                                       |
    Then Recent records reads, in order:
      | record              | when       |
      | Outdoor Run · 5.5km | Yesterday  |
      | Push-up · 25 reps   | 2 days ago |
      | Plank · 1:45        | 5 days ago |

  Scenario: An exercise's first session isn't a record
    Given these saved workouts:
      | started          | exercises                        |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg |
    Then Recent records says "No new records in the last 30 days"

  Scenario Outline: A record opens its Exercise page and back returns to the Dashboard
    Given these saved workouts:
      | started           | exercises                        |
      | 10 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg |
      | 2 days ago 18:05  | Barbell Back Squat: 5 reps 70 kg |
    When I open "Barbell Back Squat" in Recent records
    Then the Exercise page is titled "Barbell Back Squat" with "Quadriceps · Barbell"
    When I go back with <how>
    Then I am on the Dashboard
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
