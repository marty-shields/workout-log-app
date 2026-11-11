Feature: Keyboard
  Number fields bring up a number keypad. With the keyboard open, Android back hides it first.

  Background:
    Given I have started a new workout

  Scenario Outline: Focusing a number field shows the soft keyboard with a number keypad
    Given I have added "<exercise>"
    When I tap <exercise> <field>
    Then the soft keyboard is shown
    And the field asks for a <keypad> keypad
    Examples:
      | exercise           | field                | keypad  |
      | Barbell Back Squat | set 1 reps           | numeric |
      | Barbell Back Squat | set 1 weight         | decimal |
      | Plank              | set 1 duration hours | numeric |
      | Treadmill          | distance             | decimal |

  Scenario: The add-exercise search shows the soft keyboard
    When I open the add-exercise list
    Then the soft keyboard is shown

  Scenario: On a new workout, Android back with the keyboard open hides the keyboard first
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 reps
    And I go back with Android back
    Then the soft keyboard is hidden
    And no prompt is shown
    And I am still on New Workout
    When I go back with Android back
    Then the prompt "Discard this workout?" shows Discard and Cancel

  Scenario: On a new workout, the back arrow with the keyboard open goes straight to the prompt
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 reps
    And I go back with the back arrow
    Then the prompt "Discard this workout?" shows Discard and Cancel

  Scenario: On an edited workout, Android back with the keyboard open hides the keyboard first
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg |
    And I have opened workout 1 from Workouts
    When I enter "6" in Barbell Back Squat set 1 reps
    And I go back with Android back
    Then the soft keyboard is hidden
    And no prompt is shown
    When I go back with Android back
    Then the prompt "Discard changes?" shows Discard and Cancel

  Scenario: On an edited workout, the back arrow with the keyboard open goes straight to the prompt
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg |
    And I have opened workout 1 from Workouts
    When I enter "6" in Barbell Back Squat set 1 reps
    And I go back with the back arrow
    Then the prompt "Discard changes?" shows Discard and Cancel

  Scenario: In the add-exercise list, Android back with the keyboard open hides the keyboard first
    When I open the add-exercise list
    And I go back with Android back
    Then the soft keyboard is hidden
    And the add-exercise list is still open
    When I go back with Android back
    Then the add-exercise list is closed

  Scenario: In the add-exercise list, its back arrow with the keyboard open closes it at once
    When I open the add-exercise list
    And I close the add-exercise list with its back arrow
    Then the add-exercise list is closed

  Scenario: Enter in a set's first field adds no set
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 weight
    And I type "60" into Barbell Back Squat set 1 weight
    And I press the Android Enter key
    Then Barbell Back Squat has 1 set

  Scenario: Enter in a set's last field adds a set and focuses its first field
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 weight
    And I type "60" into Barbell Back Squat set 1 weight
    And I tap Barbell Back Squat set 1 reps
    And I type "8" into Barbell Back Squat set 1 reps
    And I press the Android Enter key
    Then Barbell Back Squat has 2 sets
    And focus is on Barbell Back Squat set 2 weight

  Scenario: Regression guard, passes without the fix on stock images: locking and unlocking with the keyboard open keeps it
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 weight
    And I type "60" into Barbell Back Squat set 1 weight
    And I tap Barbell Back Squat set 1 reps
    And I lock and unlock the phone
    Then the log shows "App resumed, keyboard wanted True" since I locked the phone
    And the soft keyboard is shown
    And focus is on Barbell Back Squat set 1 reps

  Scenario: Regression guard, passes without the fix on stock images: locking and unlocking with the keyboard hidden keeps it hidden
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 reps
    And I go back with Android back
    Then the soft keyboard is hidden
    When I lock and unlock the phone
    Then the log shows "App resumed, keyboard wanted False" since I locked the phone
    And the soft keyboard is hidden
    And focus is on Barbell Back Squat set 1 reps
