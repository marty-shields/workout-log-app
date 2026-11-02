@backup
Feature: Backup and restore
  Android's backup brings the workouts back after the app is uninstalled and installed again; the logs stay behind.

  Scenario: Workouts come back after a backup and a reinstall
    Given I have saved a workout with Barbell Back Squat: 5 reps 60 kg
    And I have saved a workout with Treadmill: 0:20:00, 3.2 km
    When Android backs up the app, and it is uninstalled and installed again
    Then Workouts lists 2 workouts, newest first
    And reopening workout 1 shows Barbell Back Squat set 1 at 5 reps, 60 kg
    And the log has no line from before the backup
