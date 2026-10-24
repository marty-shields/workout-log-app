@US-12 @FR-06 @FR-08
Feature: Strength sets
  A strength exercise shows only its own fields, and Add set waits for a valid set.

  Background:
    Given I have started a new workout

  Scenario Outline: Each kind of strength exercise shows its own fields
    When I add "<exercise>"
    Then set 1 shows exactly the fields: <fields>
    Examples:
      | exercise                 | fields                              |
      | Barbell Back Squat       | Reps, Weight (kg)                   |
      | Plank                    | Duration, Weight (kg)               |
      | Farmer's Walk            | Distance (m), Weight (kg), Duration |

  @smoke @FR-05
  Scenario: Add set after a valid set adds the next set below it
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    When I tap Add set on Barbell Back Squat
    Then Barbell Back Squat has 2 sets
    And set 1 still shows 8 reps, 60 kg

  @D162
  Scenario: Add set on an empty set is blocked and shows why
    Given I have added "Barbell Back Squat"
    When I tap Add set on Barbell Back Squat
    Then Barbell Back Squat has 1 set
    And Barbell Back Squat set 1 shows "Enter reps"
    And focus is on Barbell Back Squat set 1 reps

  Scenario: Add set shows every problem on the set and focuses the first field at fault
    Given I have added "Barbell Back Squat"
    When I enter "60.255" in Barbell Back Squat set 1 weight
    And I tap Add set on Barbell Back Squat
    Then Barbell Back Squat set 1 shows:
      | Enter reps                             |
      | Weight can have up to 2 decimal places |
    And focus is on Barbell Back Squat set 1 reps

  Scenario Outline: Strength values are checked
    Given I have added "<exercise>"
    And I have logged <exercise> set 1: <valid>
    When I enter "<value>" in <exercise> set 1 <field>
    And I tap Add set on <exercise>
    Then <exercise> set 1 shows "<message>"
    And <exercise> has 1 set
    Examples:
      | exercise                 | valid   | field    | value   | message                                    |
      | Barbell Back Squat       | 8 reps  | reps     | 8.5     | Reps must be a whole number                |
      | Barbell Back Squat       | 8 reps  | weight   | 22,75   | Weight must be a number                    |
      | Machine Assisted Pull-up | 8 reps  | weight   | 1001    | Assist must be 1,000 or less               |
      | Plank                    | 0:01:00 | duration | 0:75:00 | Duration minutes and seconds must be 00–59 |
      | Farmer's Walk            | 40 m    | distance | 0       | Distance must be more than 0               |

  Scenario: An accepted value lets me add the next set
    Given I have added "Barbell Back Squat"
    When I log Barbell Back Squat set 1: 8 reps, 22.75 kg
    And I tap Add set on Barbell Back Squat
    Then Barbell Back Squat has 2 sets

  @D174
  Scenario: Once a set fails, each change re-checks it
    Given I have added "Barbell Back Squat"
    And I have entered "60.255" in Barbell Back Squat set 1 weight
    And I have tapped Add set on Barbell Back Squat
    When I enter "60.25" in Barbell Back Squat set 1 weight
    Then Barbell Back Squat set 1 shows only "Enter reps"
    When I enter "8" in Barbell Back Squat set 1 reps
    Then Barbell Back Squat set 1 shows no errors

  Scenario: Remove set is never blocked
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat set 1: 8 reps, 60 kg
    And I have tapped Add set on Barbell Back Squat
    And I have entered "8.5" in Barbell Back Squat set 2 reps
    When I remove Barbell Back Squat set 2
    Then Barbell Back Squat has 1 set

  @FR-05
  Scenario: Removing a middle set keeps the others in order
    Given I have added "Barbell Back Squat"
    And I have logged Barbell Back Squat sets: 5 reps 60 kg; 5 reps 62.5 kg; 5 reps 65 kg
    When I remove Barbell Back Squat set 2
    Then Barbell Back Squat sets are: 5 reps 60 kg; 5 reps 65 kg

  @D150 @US-11
  Scenario: Remove exercise removes it at once
    Given I have added "Barbell Back Squat"
    And I have added "Dumbbell Curl"
    When I remove Barbell Back Squat
    Then no prompt is shown
    And the exercises are, in order: Dumbbell Curl

  @D107
  Scenario: A partly filled duration fills in when I leave it
    Given I have added "Plank"
    When I enter "1::" in Plank set 1 duration and leave the field
    Then Plank set 1 duration shows "1:00:00"

  Scenario: Each duration box takes two digits
    Given I have added "Plank"
    When I type "123" into Plank set 1 duration minutes
    Then Plank set 1 duration minutes shows "12"
