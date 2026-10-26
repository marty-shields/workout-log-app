@US-18 @FR-23
Feature: Discard edits
  Back asks only when I have changed something.

  Background:
    Given these saved workouts:
      | started         | exercises                                                  |
      | yesterday 18:05 | Barbell Back Squat: 5 reps 60 kg / Rowing Machine: 0:10:00 |
    And I have opened workout 1 from History

  @US-28
  Scenario Outline: Back with no changes goes straight to History
    When I go back with <how>
    Then I am on History with no prompt
    Examples:
      | how            |
      | the back arrow |
      | Android back   |

  @D189 @D236
  Scenario Outline: Any change makes back ask "Discard changes?"
    When I <change>
    And I go back with the back arrow
    Then the prompt "Discard changes?" shows Discard and Cancel
    And focus is on Cancel
    Examples:
      | change                                          |
      | enter "62.5" in Barbell Back Squat set 1 weight |
      | add "Push-up"                                   |
      | add "Rowing Machine"                            |

  @D189
  Scenario Outline: Opening the picker isn't a change
    When I <action>
    And I go back with the back arrow
    Then I am on History with no prompt
    Examples:
      | action                                |
      | open the exercise picker and close it |

  Scenario: Cancel keeps my edit
    Given I have entered "62.5" in Barbell Back Squat set 1 weight
    And I have gone back with Android back
    When I tap Cancel
    Then I am still on Edit Workout
    And Barbell Back Squat set 1 still shows 62.5 kg

  @smoke
  Scenario: Discard returns to History and keeps the saved workout as it was
    Given I have entered "62.5" in Barbell Back Squat set 1 weight
    And I have gone back with the back arrow
    When I tap Discard
    Then I am on History
    And reopening workout 1 shows Barbell Back Squat set 1 at 60 kg

  @D144 @US-28
  Scenario: Android back on the prompt cancels it
    Given I have entered "62.5" in Barbell Back Squat set 1 weight
    And I have gone back with the back arrow
    When I press Android back
    Then the prompt is closed
    And I am still on Edit Workout
