@US-14 @US-08 @US-09
Feature: Save a workout
  Saving keeps the workout, records the finish time and returns to the Dashboard.

  @smoke @today
  Scenario: Save a strength workout with two sets
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat sets: 8 reps 60 kg; 8 reps 62.5 kg
    When I save the workout
    Then I am on the Dashboard
    And the last workout shows today's date and day
    And this week shows 1 workout
    And Workouts lists 1 workout, today at the time I started

  @smoke
  Scenario: Save a cardio workout and see its values again
    Given I have started a new workout
    And I have added "Treadmill"
    And I have logged Treadmill: 0:20:00, 3.2 km, incline 1.5
    And I have added "Outdoor Run"
    And I have logged Outdoor Run: 0:25:30, 5 km
    When I save the workout
    And I open the latest workout from Workouts
    Then Treadmill shows 0:20:00, 3.2 km, incline 1.5
    And Outdoor Run shows 0:25:30, 5 km

  Scenario Outline: A blank weight other than a lift's is saved as 0 kg
    Given I have started a new workout
    And I have added "<exercise>"
    And I have logged <exercise> sets: 8 reps
    When I save the workout
    Then reopening workout 1 shows <exercise> sets: 8 reps 0 kg
    Examples:
      | exercise                 |
      | Machine Assisted Pull-up |
      | Pull-up                  |

  Scenario: A lift's blank weight can't be saved
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat sets: 8 reps
    When I save the workout
    Then Barbell Back Squat set 1 shows "Enter weight"
    And I am still on New Workout

  Scenario: Save a mixed workout and see it newest first
    Given I have started a new workout
    And I have logged:
      | exercise           | values                                     |
      | Barbell Back Squat | 5 reps 60 kg; 5 reps 62.5 kg; 5 reps 65 kg |
      | Plank              | 0:01:00 0 kg                               |
      | Farmer's Walk      | 40 m 32 kg 0:00:45; 40 m 32 kg 0:00:50     |
      | Treadmill          | 0:20:00, 3.2 km                            |
      | Rowing Machine     | 0:10:00, 2000 m, damper 5, 120 calories    |
    When I save the workout
    And I open the latest workout from Workouts
    Then it holds exactly what I logged, newest first

  Scenario: Optional zero values are kept
    Given I have started a new workout
    And I have added "Treadmill"
    And I have logged Treadmill: 0:20:00, 3.2 km, incline 0
    When I save the workout
    And I open the latest workout from Workouts
    Then Treadmill shows incline 0

  @today
  Scenario: Two workouts can be saved on the same day
    Given I have saved a workout with Push-up: 10 reps
    And I have saved a workout with Push-up: 12 reps
    Then Workouts lists 2 workouts, newest first
    And this week shows 2 workouts

  Scenario: Tapping Save twice saves once
    Given I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 10 reps
    When I tap Save twice quickly
    Then Workouts lists 1 workout

  Scenario: The finish time is recorded on Save
    Given I have started a new workout
    And I have added "Push-up"
    And I have logged Push-up set 1: 10 reps
    And 5 seconds have passed since I started
    When I save the workout
    And I open the latest workout from Workouts
    Then the total time is at least 0:00:05 and no more than the time I took
