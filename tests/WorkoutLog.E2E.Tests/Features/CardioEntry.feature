@US-13 @FR-07
Feature: Cardio entry
  A cardio exercise has one entry: duration plus the machine's own fields.

  Background:
    Given I have started a new workout

  Scenario Outline: Each cardio machine shows duration and its own fields
    When I add "<machine>"
    Then the entry shows exactly the fields: <fields>
    Examples:
      | machine        | fields                                                                         |
      | Treadmill      | Duration, Distance (km), Incline (%), Calories, Rate (km/h)                    |
      | Rowing Machine | Duration, Distance (m), Damper, Calories, Rate (strokes/min)                   |
      | Stair Climber  | Duration, Floors, Speed level, Calories, Rate (steps/min)                      |
      | Outdoor Run    | Duration, Distance (km), Calories, Pace (min/km)                               |

  @smoke
  Scenario: Stair Climber saves with duration only
    Given I have added "Stair Climber"
    When I log Stair Climber: 0:15:00
    And I save the workout
    Then I am on the Dashboard

  @FR-08
  Scenario: A cardio exercise has exactly one entry
    When I add "Treadmill"
    Then Treadmill has no Add set and its entry can't be removed on its own
    And Treadmill can be removed as an exercise

  @D166
  Scenario: A cardio entry isn't checked until Save
    Given I have added "Treadmill"
    When I enter "1001" in Treadmill distance
    Then Treadmill shows no errors

  Scenario Outline: Cardio values are checked on Save
    Given I have added "<machine>"
    And I have logged <machine>: <valid>
    When I enter "<value>" in <machine> <field>
    And I save the workout
    Then <machine> shows "<message>"
    And I am still on New Workout
    Examples:
      | machine        | valid   | field    | value   | message                                    |
      | Rowing Machine | 0:10:00 | level    | 5.5     | Damper must be a whole number              |
      | Rowing Machine | 0:10:00 | calories | abc     | Calories must be a number                  |
      | Outdoor Run    | 0:25:30 | pace     | 5:75    | Pace seconds must be 00–59                 |

  @D171
  Scenario: Save shows every problem on an entry and focuses the first field at fault
    Given I have added "Treadmill"
    And I have entered "1001" in Treadmill distance
    And I have entered "1.255" in Treadmill incline
    When I save the workout
    Then Treadmill shows:
      | Enter duration                          |
      | Distance must be 1,000 or less          |
      | Incline can have up to 2 decimal places |
    And focus is on Treadmill duration hours

  @D174
  Scenario: After a failed Save each change re-checks the entry
    Given I have added "Treadmill"
    And I have logged Treadmill: 0:20:00
    And I have entered "1.255" in Treadmill incline
    And I have saved the workout
    When I enter "1.5" in Treadmill incline
    Then Treadmill shows no errors

  @D107 @D167
  Scenario: A partly filled pace fills in when I leave it
    Given I have added "Outdoor Run"
    When I enter "5:3" in Outdoor Run pace and leave the field
    Then Outdoor Run pace shows "5:03"
