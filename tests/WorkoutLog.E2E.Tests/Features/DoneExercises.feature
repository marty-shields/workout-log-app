Feature: Mark an exercise done
  While logging I mark an exercise done, so it shrinks to one line and is hard to touch by accident.

  Background:
    Given I have started a new workout

  Scenario: Done shrinks a valid exercise to one line
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    When I tap Done on Barbell Back Squat
    Then Barbell Back Squat is shown as one line with its name, the pencil and the trash

  Scenario: Done is blocked by a blank set
    Given I have added "Barbell Back Squat"
    When I tap Done on Barbell Back Squat
    Then Barbell Back Squat is shown open
    And Barbell Back Squat set 1 shows "Enter reps"
    And focus is on Barbell Back Squat set 1 reps

  Scenario: Done checks a cardio entry too
    Given I have added "Treadmill"
    When I tap Done on Treadmill
    Then Treadmill is shown open

  Scenario: The pencil opens the exercise as I left it
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat sets: 5 reps 60 kg; 5 reps 62.5 kg
    And I have marked Barbell Back Squat done
    When I tap the pencil on Barbell Back Squat
    Then Barbell Back Squat is shown open
    And Barbell Back Squat sets are: 5 reps 60 kg; 5 reps 62.5 kg

  Scenario: Done does not reorder the exercises
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have marked Barbell Back Squat done
    When I add "Dumbbell Curl"
    Then the exercises are, in order: Dumbbell Curl, Barbell Back Squat

  Scenario: Picking a done exercise again opens it on top with a new set
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have marked Barbell Back Squat done
    And I have added "Dumbbell Curl"
    When I add "Barbell Back Squat"
    Then the exercises are, in order: Barbell Back Squat, Dumbbell Curl
    And Barbell Back Squat is shown open
    And Barbell Back Squat has 2 sets

  Scenario: The trash asks first on an open exercise
    Given I have added "Barbell Back Squat"
    When I tap the trash on Barbell Back Squat
    Then the prompt "Delete Barbell Back Squat?" shows Delete and Cancel
    When I tap Cancel
    Then the exercises are, in order: Barbell Back Squat

  Scenario: The trash asks first on a done exercise
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have marked Barbell Back Squat done
    When I tap the trash on Barbell Back Squat
    Then the prompt "Delete Barbell Back Squat?" shows Delete and Cancel
    When I tap Delete on the prompt
    Then no prompt is shown
    And the workout shows "No exercises yet"

  Scenario: A done exercise is saved as it stands
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have marked Barbell Back Squat done
    When I save the workout
    Then I am on the Dashboard
