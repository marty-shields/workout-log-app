@upgrade
Feature: Upgrade
  This build installs over the last release and keeps the workouts saved in it. The last release's own E2E project saves
  them first (UpgradeSeed.feature, through upgrade-e2e.sh), so these steps never drive the last release's screens.

  @today
  Scenario: Workouts saved in the last release survive this build installed over it
    Given the last release is installed with the workouts it saved
    When I install this build over it
    Then Workouts lists 2 workouts, newest first
    And reopening workout 2 shows: Farmer's Walk, Treadmill
    When I open workout 1 from Workouts
    And I enter "65" in Barbell Back Squat set 2 weight
    And I save the workout
    Then reopening workout 1 shows Barbell Back Squat set 2 at 65 kg
    When I go back to the Dashboard
    And I open "Barbell Back Squat" on Exercises
    Then the Exercise sessions read:
      | day   | headline | sets        |
      | today | 65kg × 5 | 60×5 · 65×5 |
