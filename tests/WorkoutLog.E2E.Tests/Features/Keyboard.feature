Feature: Keyboard
  Number fields bring up a number keypad. With the keyboard open, Android back hides it first.

  Background:
    Given I have started a new workout

  @US-12 @US-13
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
      | Outdoor Run        | pace minutes         | numeric |

  @US-11
  Scenario: The picker search shows the soft keyboard
    When I open the exercise picker
    Then the soft keyboard is shown

  @D78 @US-15 @US-28
  Scenario: On a new workout, Android back with the keyboard open hides the keyboard first
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 reps
    And I go back with Android back
    Then the soft keyboard is hidden
    And no prompt is shown
    And I am still on New Workout
    When I go back with Android back
    Then the prompt "Discard this workout?" shows Discard and Cancel

  @D78 @US-15
  Scenario: On a new workout, the back arrow with the keyboard open goes straight to the prompt
    Given I have added "Barbell Back Squat"
    When I tap Barbell Back Squat set 1 reps
    And I go back with the back arrow
    Then the prompt "Discard this workout?" shows Discard and Cancel

  @D78 @US-18
  Scenario: On an edited workout, Android back with the keyboard open hides the keyboard first
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps |
    And I have opened workout 1 from History
    When I enter "6" in Barbell Back Squat set 1 reps
    And I go back with Android back
    Then the soft keyboard is hidden
    And no prompt is shown
    When I go back with Android back
    Then the prompt "Discard changes?" shows Discard and Cancel

  @D78 @US-18
  Scenario: On an edited workout, the back arrow with the keyboard open goes straight to the prompt
    Given these saved workouts:
      | started         | exercises                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps |
    And I have opened workout 1 from History
    When I enter "6" in Barbell Back Squat set 1 reps
    And I go back with the back arrow
    Then the prompt "Discard changes?" shows Discard and Cancel

  @D78 @US-11
  Scenario: In the picker, Android back with the keyboard open hides the keyboard first
    When I open the exercise picker
    And I go back with Android back
    Then the soft keyboard is hidden
    And the picker is still open
    When I go back with Android back
    Then the picker is closed

  @D78 @US-11
  Scenario: In the picker, its back arrow with the keyboard open closes it at once
    When I open the exercise picker
    And I close the picker with the picker back arrow
    Then the picker is closed
