@US-11 @FR-03 @FR-04
Feature: Add exercise
  I find exercises in the Exercises list over the workout and add them.

  Background:
    Given I have started a new workout

  @D81 @D64
  Scenario: The list opens with the search box focused and empty
    When I open the add-exercise list
    Then the search box is empty and focused

  @US-07
  Scenario: With no search text every exercise shows, Strength then Cardio, A–Z, each with its kind
    When I open the add-exercise list
    Then every exercise shows, Strength then Cardio, A–Z in each
    And each row shows its kind

  Scenario Outline: A chip shows only that kind
    When I choose the <kind> chip
    Then only <kind> exercises show, A–Z
    Examples:
      | kind     |
      | Strength |
      | Cardio   |

  Scenario: A chip and the search combine
    Given I have chosen the Cardio chip
    When I search the add-exercise list for "row"
    Then the results are only Cardio exercises and include "Rowing Machine"

  Scenario Outline: Search matches names and nicknames, words in any order
    When I search the add-exercise list for "<query>"
    Then the results include "<exercise>"
    Examples:
      | query                  | exercise                 |
      | pec deck               | Machine Chest Fly        |
      | deck pec               | Machine Chest Fly        |

  @D63
  Scenario: Names that start with the query come first
    When I search the add-exercise list for "dumb curl"
    Then the first result is "Dumbbell Curl"

  Scenario: A search with no match says so
    When I search the add-exercise list for "zumba"
    Then the add-exercise list shows "No exercises match"

  Scenario: The search box takes at most 50 characters
    When I type 60 characters into the add-exercise search
    Then the search box holds the first 50

  @D64
  Scenario: The search and chip are cleared each time the list opens
    Given I have searched the add-exercise list for "squat"
    And I have chosen the Strength chip
    When I close the add-exercise list with Android back
    And I open the add-exercise list
    Then the search box is empty, the All chip is on and every exercise shows

  @smoke @D42
  Scenario: Picking an exercise adds it at the top and focuses its first field
    Given I have added "Barbell Back Squat"
    When I add "Dumbbell Curl"
    Then the exercises are, in order: Dumbbell Curl, Barbell Back Squat
    And the add-exercise list is closed
    And focus is on Dumbbell Curl set 1 reps

  @D42
  Scenario: A picked exercise above the screen scrolls into view
    Given I have added "Treadmill"
    And I have added "Rowing Machine"
    And I have added "Farmer's Walk"
    And I have scrolled to the bottom of the workout
    When I add "Dumbbell Curl"
    Then focus is on Dumbbell Curl set 1 reps
    And the focused field is in view

  Scenario: A strength exercise starts with one empty set
    When I add "Barbell Back Squat"
    Then Barbell Back Squat has 1 set and it is empty

  Scenario: A cardio exercise starts with its empty entry and focuses its duration
    When I add "Treadmill"
    Then Treadmill has its entry and it is empty
    And focus is on Treadmill duration hours

  Scenario Outline: Closing the list adds nothing
    Given I have opened the add-exercise list
    When I close the add-exercise list with <how>
    Then the add-exercise list is closed
    And the workout shows "No exercises yet"
    Examples:
      | how             |
      | its back arrow  |
      | Android back    |

  @D42
  Scenario: Picking a strength exercise already added adds a set to its card
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps, 60 kg
    When I add "Barbell Back Squat"
    Then there is 1 exercise
    And Barbell Back Squat has 2 sets
    And focus is on Barbell Back Squat set 2 reps

  @D42
  Scenario: Picking a strength exercise again while its last set is empty is blocked like Add set
    Given I have added "Barbell Back Squat"
    When I add "Barbell Back Squat"
    Then Barbell Back Squat has 1 set
    And Barbell Back Squat set 1 shows "Enter reps"
    And focus is on Barbell Back Squat set 1 reps

  @D42
  Scenario: Picking a cardio exercise already added moves it to the top and focuses its entry
    Given I have added "Rowing Machine"
    And I have added "Barbell Back Squat"
    When I add "Rowing Machine"
    Then the exercises are, in order: Rowing Machine, Barbell Back Squat
    And focus is on Rowing Machine duration hours

  @D42
  Scenario: Picking a strength exercise already added moves it to the top with a new set
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 5 reps, 60 kg
    And I have added "Treadmill"
    And I have added "Plank"
    When I add "Barbell Back Squat"
    Then the exercises are, in order: Barbell Back Squat, Plank, Treadmill
    And Barbell Back Squat has 2 sets
    And focus is on Barbell Back Squat set 2 reps
    And the focused field is in view
