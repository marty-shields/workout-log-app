@FR-52
Feature: Import
  Settings imports a file made by Export: the file is read and checked, reviewed, then written in one go.

  @smoke
  Scenario: Importing into an empty phone adds every workout
    Given an export file "e2e-import.json" in Downloads with these workouts:
      | started          | exercises                         |
      | yesterday 18:05  | Barbell Bench Press: 8 reps 60 kg |
      | 3 days ago 18:05 | Treadmill: 0:20:00, 3.2 km        |
    And I have opened Settings
    When I import "e2e-import.json"
    Then the review says "2 workouts in the file"
    And the review counts 2 new, 0 already on this phone, 0 different and 0 that can't be imported
    When I tap Import on the review
    Then I am on Settings
    And Settings shows "Added 2 workouts" under Import
    And Settings says "Export all 2 workouts to a JSON file"
    And the log shows "Imported with Add and KeepPhones: 2 new, 0 already on the phone, 0 different, 0 can't be imported"

  Scenario Outline: A workout on the phone with different contents is kept or replaced
    Given these saved workouts:
      | started          | exercises                         |
      | yesterday 18:05  | Barbell Bench Press: 8 reps 60 kg |
      | 3 days ago 18:05 | Push-up: 10 reps 0 kg                  |
    And an export file "e2e-import.json" in Downloads with these workouts:
      | started          | exercises                         |
      | yesterday 18:05  | Barbell Bench Press: 8 reps 60 kg |
      | 3 days ago 18:05 | Push-up: 12 reps 0 kg                  |
      | 5 days ago 18:05 | Push-up: 20 reps 0 kg                  |
    And I have opened Settings
    When I import "e2e-import.json"
    Then the review counts 1 new, 1 already on this phone, 1 different and 0 that can't be imported
    When I choose "<choice>" on the review
    And I tap Import on the review
    Then Settings shows "<message>" under Import
    And Settings says "Export all 3 workouts to a JSON file"
    When I go back with the back arrow
    Then reopening workout 2 shows Push-up sets: <reps> reps 0 kg
    Examples:
      | choice            | message                     | reps |
      | Keep this phone's | Added 1 workout             | 10   |
      | Use the file's    | Added 1 workout, replaced 1 | 12   |

  Scenario: Replace all asks first, then deletes the phone's workouts and imports the file's
    Given these saved workouts:
      | started          | exercises        |
      | yesterday 18:05  | Push-up: 10 reps 0 kg |
      | 3 days ago 18:05 | Push-up: 11 reps 0 kg |
      | 4 days ago 18:05 | Push-up: 12 reps 0 kg |
    And an export file "e2e-import.json" in Downloads with these workouts:
      | started          | exercises                         |
      | yesterday 18:05  | Push-up: 10 reps 0 kg                  |
      | 6 days ago 18:05 | Barbell Bench Press: 8 reps 60 kg |
    And I have opened Settings
    When I import "e2e-import.json"
    And I choose "Replace all my workouts" on the review
    Then Import on the review reads "Replace all"
    When I tap Import on the review
    Then the review asks "Delete your 3 workouts and import 2?"
    When I tap Replace on the review's prompt
    Then I am on Settings
    And Settings shows "Added 2 workouts" under Import
    And Settings says "Export all 2 workouts to a JSON file"

  Scenario: A file with nothing new can't be imported with Add
    Given these saved workouts:
      | started         | exercises        |
      | yesterday 18:05 | Push-up: 10 reps 0 kg |
    And an export file "e2e-import.json" in Downloads with these workouts:
      | started         | exercises        |
      | yesterday 18:05 | Push-up: 10 reps 0 kg |
    And I have opened Settings
    When I import "e2e-import.json"
    Then the review counts 0 new, 1 already on this phone, 0 different and 0 that can't be imported
    And the review shows "Nothing new to import"
    And Import on the review is disabled

  @FR-04
  Scenario: A workout with an exercise twice can't be imported
    Given an export file "e2e-import.json" in Downloads with these workouts:
      | started          | exercises                                    |
      | yesterday 18:05  | Push-up: 10 reps 0 kg / Push-up: 8 reps 0 kg |
      | 3 days ago 18:05 | Push-up: 12 reps 0 kg                        |
    And I have opened Settings
    When I import "e2e-import.json"
    Then the review counts 1 new, 0 already on this phone, 0 different and 1 that can't be imported
    And the log shows "Workout 1 in the file can't be imported: RepeatedExercise"

  Scenario: A workout with a property the format doesn't have can't be imported
    Given an export file "e2e-import.json" in Downloads with these workouts, the first with the unknown property "note":
      | started          | exercises             |
      | yesterday 18:05  | Push-up: 10 reps 0 kg |
      | 3 days ago 18:05 | Push-up: 12 reps 0 kg |
    And I have opened Settings
    When I import "e2e-import.json"
    Then the review counts 1 new, 0 already on this phone, 0 different and 1 that can't be imported
    And the log shows "Workout 1 in the file can't be imported: Malformed"

  Scenario: A file that isn't an export shows an error, and another file can be chosen
    Given a file "e2e-notes.json" in Downloads holding "{}"
    And an export file "e2e-import.json" in Downloads with these workouts:
      | started         | exercises        |
      | yesterday 18:05 | Push-up: 10 reps 0 kg |
    And I have opened Settings
    When I import "e2e-notes.json"
    Then the review shows the error "This file isn't a Workout Log export"
    When I choose another file "e2e-import.json"
    Then the review says "1 workout in the file"
    And the log shows "The import file can't be imported: NotAnExport"

  Scenario: Cancelling the file picker leaves Settings as it was
    Given I have opened Settings
    When I tap Import and cancel the file picker
    Then I am on Settings
    And Settings has no message under Import

  Scenario Outline: Back from the review returns to Settings without importing
    Given an export file "e2e-import.json" in Downloads with these workouts:
      | started         | exercises        |
      | yesterday 18:05 | Push-up: 10 reps 0 kg |
    And I have opened Settings
    When I import "e2e-import.json"
    And I go back with <how>
    Then I am on Settings
    And Settings says "No workouts to export"
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
