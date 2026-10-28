@US-17 @FR-21 @FR-22 @FR-11 @FR-13
Feature: Edit a workout
  I open a saved workout and fix it; its times never change.

  Background:
    Given these saved workouts:
      | started         | finished        | exercises                                                                                                                                                |
      | yesterday 18:05 | yesterday 18:52 | Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg; 5 reps 65 kg / Farmer's Walk: 40 m 32 kg 0:00:45; 50 m 32 kg 0:00:45 / Treadmill: 0:20:00, 3.2 km, incline 1.5 |
    And I have opened workout 1 from Workouts

  Scenario: The saved workout shows with its total time
    Then the title is yesterday's date and day
    And the time shows "Total 0:47:00" and doesn't count up
    And Delete is shown
    And it holds exactly what was saved, newest first

  @D80
  Scenario: Carry sets start closed on Edit
    Then every Farmer's Walk set is a summary

  Scenario: Change a set and save
    When I enter "70" in Barbell Back Squat set 3 weight
    And I save the workout
    Then I am on the Dashboard
    And reopening workout 1 shows Barbell Back Squat set 3 at 70 kg

  @smoke @US-09
  Scenario: Add a set and save; the times don't change
    When I tap Add set on Barbell Back Squat
    And I log Barbell Back Squat set 4: 5 reps, 65 kg
    And I save the workout
    Then reopening workout 1 shows 4 Barbell Back Squat sets
    And the time still shows "Total 0:47:00"
    And Workouts still shows workout 1 at yesterday 18:05

  Scenario: Remove a set and save
    When I remove Barbell Back Squat set 2
    And I save the workout
    Then reopening workout 1 shows Barbell Back Squat sets: 5 reps 60 kg; 5 reps 65 kg

  Scenario: Swap an exercise and save
    When I remove Treadmill
    And I add "Dumbbell Incline Press"
    And I log Dumbbell Incline Press set 1: 10 reps, 22 kg
    And I save the workout
    Then reopening workout 1 shows: Dumbbell Incline Press, Farmer's Walk, Barbell Back Squat

  @D42
  Scenario: Picking an exercise again saves it on top
    When I add "Barbell Back Squat"
    And I log Barbell Back Squat set 4: 5 reps, 65 kg
    And I save the workout
    Then reopening workout 1 shows: Barbell Back Squat, Treadmill, Farmer's Walk

  Scenario: Change a cardio entry and save
    When I enter "4" in Treadmill distance
    And I save the workout
    Then reopening workout 1 shows Treadmill at 4 km

  Scenario: Edits are checked as on New Workout
    When I clear Barbell Back Squat set 1 reps
    And I save the workout
    Then Barbell Back Squat set 1 shows "Enter reps"
    And focus is on Barbell Back Squat set 1 reps
    And I am still on Edit Workout

  Scenario: Removing every exercise blocks Save
    When I remove every exercise
    And I save the workout
    Then the workout shows "Add at least one exercise"

