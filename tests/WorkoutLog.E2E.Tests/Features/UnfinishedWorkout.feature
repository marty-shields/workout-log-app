Feature: Unfinished workout
  A new workout survives the app being killed. This week offers to resume it or discard it.

  Background:
    Given I have started a new workout
    And I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps, 60 kg
    And I have tapped Add set on Barbell Back Squat
    And I have entered "8" in Barbell Back Squat set 2 reps

  @smoke
  Scenario: A killed workout is offered on This week and resumes where I was
    When I kill the app and open it again
    Then I am on This week
    And the unfinished workout banner shows "1 exercise"
    And this week shows no workouts
    When I tap Resume on the banner
    Then I am on New Workout
    And the timer shows the time since I started
    And Barbell Back Squat set 1 is a summary "60kg · 5 reps"
    And Barbell Back Squat set 2 is open
    And Barbell Back Squat set 2 still shows 8 reps
    When I enter "60" in Barbell Back Squat set 2 weight
    And I save the workout
    Then this week's newest workout is from today
    And no unfinished workout banner is shown
    And reopening the latest workout shows Barbell Back Squat set 1 at 5 reps, 60 kg

  Scenario: Discard on the banner says what is lost and Cancel keeps the workout
    When I kill the app and open it again
    And I tap Discard on the banner
    Then the prompt "Discard this workout?" shows Discard and Cancel
    And the prompt says "1 exercise, 2 sets will be lost"
    When I tap Cancel
    Then the unfinished workout banner shows "1 exercise"

  Scenario: Confirming Discard removes the workout and the banner
    When I kill the app and open it again
    And I tap Discard on the banner
    And I tap Discard
    Then no unfinished workout banner is shown
    And this week shows no workouts

  Scenario: + with an unfinished workout offers Resume or Discard and start new
    When I kill the app and open it again
    And I tap + in the bottom nav
    Then the sheet offers Resume and Discard and start new
    When I tap Resume on the sheet
    Then I am on New Workout
    And there is 1 exercise

  Scenario: Discard and start new begins an empty workout and leaves nothing to recover
    When I kill the app and open it again
    And I tap + in the bottom nav
    And I tap Discard and start new on the sheet
    Then I am on New Workout
    And there are 0 exercises
    When I kill the app and open it again
    Then no unfinished workout banner is shown

  Scenario: Back then Discard in a resumed workout removes it
    When I kill the app and open it again
    And I tap Resume on the banner
    And I discard the workout
    Then no unfinished workout banner is shown
    And this week shows no workouts

  Scenario: A workout with no exercises isn't offered
    Given I have removed Barbell Back Squat
    When I kill the app and open it again
    Then I am on This week
    And no unfinished workout banner is shown
