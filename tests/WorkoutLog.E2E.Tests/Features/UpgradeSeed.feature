@upgrade-seed
Feature: Upgrade seed
  The first half of the next release's upgrade check. upgrade-e2e.sh runs this scenario from the last release's own E2E
  project, on that release's own build, and leaves the app installed with these workouts; Upgrade.feature then checks
  them in the build installed over it. So keep the workouts as they are. To check more, add a workout here and check it
  in Upgrade.feature only once a release has it. The shards and the Release E2E run this scenario too, so every release
  proves its own seed works.

  @release @today
  Scenario: Save the workouts an upgrade must keep
    Given I have saved a workout with Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg
    And I have saved a workout with Treadmill: 0:20:00, 3.2 km, incline 1.5 / Farmer's Walk: 40 m 32 kg 0:00:45
    Then Workouts lists 2 workouts, newest first
