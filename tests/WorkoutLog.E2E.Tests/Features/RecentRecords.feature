Feature: Records this week
  This week shows the records I've set since Monday, newest first, one per exercise, up to five and then how many more.

  @today
  Scenario: Records set this week, newest first, with the weekday
    Given these saved workouts:
      | started           | finished          | exercises                                                                                                    |
      | 60 days ago 18:05 | 60 days ago 18:50 | Barbell Back Squat: 5 reps 60 kg / Push-up: 20 reps 0 kg / Plank: 0:01:00 0 kg / Outdoor Run: 0:30:00, 5 km |
      | 40 days ago 18:05 | 40 days ago 18:50 | Barbell Back Squat: 5 reps 80 kg                                                                             |
      | 8 days ago 18:05  | 8 days ago 18:50  | Outdoor Run: 0:30:00, 5.2 km                                                                                 |
      | today 00:00       | today 00:02       | Barbell Back Squat: 5 reps 90 kg / Push-up: 25 reps 0 kg                                                     |
      | today 00:03       | today 00:04       | Outdoor Run: 0:20:00, 5.5 km / Plank: 0:01:45 0 kg                                                           |
    Then Records this week reads, in order:
      | record                        | day   |
      | Outdoor Run · 5.5km           | today |
      | Plank · 1:45                  | today |
      | Barbell Back Squat · 90kg × 5 | today |
      | Push-up · 25 reps             | today |

  @today
  Scenario: A record from before this week isn't listed
    Given these saved workouts:
      | started           | finished          | exercises                        |
      | 14 days ago 18:05 | 14 days ago 18:50 | Barbell Back Squat: 5 reps 60 kg |
      | 8 days ago 18:05  | 8 days ago 18:50  | Barbell Back Squat: 5 reps 70 kg |
      | today 00:00       | today 00:03       | Push-up: 10 reps 0 kg            |
    Then Records this week says "No new records this week"

  @today
  Scenario: An exercise's first session isn't a record
    Given these saved workouts:
      | started     | finished    | exercises                        |
      | today 00:00 | today 00:03 | Barbell Back Squat: 5 reps 60 kg |
    Then Records this week says "No new records this week"

  @today
  Scenario: More than five records show five and how many more
    Given these saved workouts:
      | started           | finished          | exercises                                                                                                                                                                       |
      | 14 days ago 18:05 | 14 days ago 18:50 | Barbell Back Squat: 5 reps 60 kg / Push-up: 10 reps 0 kg / Plank: 0:01:00 0 kg / Dumbbell Curl: 10 reps 10 kg / Barbell Bench Press: 5 reps 50 kg / Outdoor Run: 0:30:00, 5 km   |
      | today 00:00       | today 00:04       | Barbell Back Squat: 5 reps 70 kg / Push-up: 15 reps 0 kg / Plank: 0:01:45 0 kg / Dumbbell Curl: 10 reps 12 kg / Barbell Bench Press: 5 reps 60 kg / Outdoor Run: 0:30:00, 5.5 km |
    Then Records this week shows 5 records and "1 more"

  @today
  Scenario Outline: A record opens its Exercise page and back returns to This week
    Given these saved workouts:
      | started           | finished          | exercises                        |
      | 10 days ago 18:05 | 10 days ago 18:50 | Barbell Back Squat: 5 reps 60 kg |
      | today 00:00       | today 00:03       | Barbell Back Squat: 5 reps 70 kg |
    When I open "Barbell Back Squat" in Records this week
    Then the Exercise page is titled "Barbell Back Squat" with "Quadriceps · Barbell"
    When I go back with <how>
    Then I am on This week
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
