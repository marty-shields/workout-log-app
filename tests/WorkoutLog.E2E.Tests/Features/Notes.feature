Feature: Notes
  I can jot a note on a workout and on each exercise in it, like the margins of a paper notebook,
  and see last time's note when I add the exercise again.

  Scenario: Both notes written on New Workout are there when I reopen the workout
    Given I have started a new workout
    And I have logged Barbell Back Squat: 5 reps 60 kg
    When I add the workout note "Slept badly, kept it light"
    And I add the note "Belt on from set 3" to Barbell Back Squat
    And I save the workout
    Then reopening workout 1 shows the workout note "Slept badly, kept it light"
    And reopening workout 1 shows the note "Belt on from set 3" on Barbell Back Squat

  Scenario: A cardio exercise keeps its note
    Given I have started a new workout
    And I have logged Rowing Machine: 0:10:00 2000 m
    When I add the note "Headwind on the last 500 m" to Rowing Machine
    And I save the workout
    Then reopening workout 1 shows the note "Headwind on the last 500 m" on Rowing Machine

  Scenario: Notes of a saved workout show in Edit Workout and are kept when I save it unchanged
    Given these saved workouts:
      | started         | exercises                                                                                   | note               |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg (note: Belt on from set 3) / Rowing Machine: 0:10:00 2000 m | Slept badly        |
    When I open the latest workout from Workouts and save it
    Then reopening workout 1 shows the workout note "Slept badly"
    And reopening workout 1 shows the note "Belt on from set 3" on Barbell Back Squat

  Scenario: I can change both notes in Edit Workout
    Given these saved workouts:
      | started         | exercises                                                   | note        |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg (note: Belt on from set 3) | Slept badly |
    And I have opened workout 1 from Workouts
    When I add the workout note "Slept well"
    And I add the note "Belt off" to Barbell Back Squat
    And I save the workout
    Then reopening workout 1 shows the workout note "Slept well"
    And reopening workout 1 shows the note "Belt off" on Barbell Back Squat

  Scenario: Last time's note shows when I add the exercise, for strength and cardio
    Given these saved workouts:
      | started         | exercises                                                                                          |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg (note: Belt on from set 3) / Rowing Machine: 0:10:00 2000 m (note: Headwind) |
    And I have started a new workout
    When I add "Barbell Back Squat"
    And I add "Rowing Machine"
    And I add "Dumbbell Curl"
    Then Barbell Back Squat has last time's note "Belt on from set 3"
    And Rowing Machine has last time's note "Headwind"
    And Dumbbell Curl has no last time's note

  Scenario: Only the most recent workout with the exercise counts
    Given these saved workouts:
      | started          | exercises                                                   |
      | 3 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg (note: Belt on from set 3) |
      | yesterday 18:05  | Barbell Back Squat: 5 reps 62.5 kg                           |
    And I have started a new workout
    When I add "Barbell Back Squat"
    Then Barbell Back Squat has no last time's note

  Scenario: Edit Workout shows the note from the workout before it
    Given these saved workouts:
      | started           | exercises                                                      |
      | 10 days ago 18:05 | Barbell Back Squat: 5 reps 60 kg (note: Belt on from set 3)    |
      | 3 days ago 18:05  | Barbell Back Squat: 5 reps 62.5 kg (note: Left knee twinge)    |
      | yesterday 18:05   | Barbell Back Squat: 5 reps 65 kg                               |
    When I open workout 2 from Workouts
    Then Barbell Back Squat has last time's note "Belt on from set 3"
    And Barbell Back Squat has the note "Left knee twinge"

  Scenario: Tapping Add note again closes the workout note
    Given I have started a new workout
    When I tap the workout note button
    Then the workout note box is open
    When I tap the workout note button
    Then the workout note box is closed
    And the workout note is gone

  Scenario Outline: Tapping the note button again closes an exercise note
    Given I have started a new workout
    And I have added "<exercise>"
    When I tap the note button of <exercise>
    Then the note box of <exercise> is open
    When I tap the note button of <exercise>
    Then the note box of <exercise> is closed
    And the note of <exercise> is gone
    Examples:
      | exercise           |
      | Barbell Back Squat |
      | Rowing Machine     |

  Scenario: Closing a note with text keeps it as one line
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    When I add the note "Belt on from set 3" to Barbell Back Squat
    And I tap the note button of Barbell Back Squat
    Then the note box of Barbell Back Squat is open
    When I tap the note button of Barbell Back Squat
    Then Barbell Back Squat has the note "Belt on from set 3"

  Scenario: Opening a note while editing a set keeps the set
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have entered "8" in Barbell Back Squat set 1 reps
    When I tap the note button of Barbell Back Squat
    And I tap the note button of Barbell Back Squat
    Then Barbell Back Squat set 1 is open
    And Barbell Back Squat set 1 reps shows "8"
