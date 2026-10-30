Feature: Export
  Settings writes every saved workout to one JSON file and opens the share sheet.

  @smoke
  Scenario: Export opens the share sheet with a file of the saved workouts
    Given these saved workouts:
      | started          | exercises                                                                    |
      | yesterday 18:05  | Treadmill: 0:20:00, 3.2 km, incline 1.5 / Barbell Bench Press: 8 reps 60 kg |
      | 3 days ago 18:05 | Barbell Bench Press: 8 reps 60 kg; 6 reps 62.5 kg / Farmer's Walk: 40 m 32 kg 0:00:45 |
    And I have opened Settings
    Then Settings says "Export all 2 workouts to a JSON file"
    When I tap Export
    Then the share sheet opens
    And the exported file holds the saved workouts

  Scenario: With no workouts, Export is disabled
    Given I have opened Settings
    Then Settings says "No workouts to export"
    And Export is disabled
