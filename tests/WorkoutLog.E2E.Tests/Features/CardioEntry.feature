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
      | Treadmill      | Duration, Distance (km), Incline (%), Calories                                 |
      | Rowing Machine | Duration, Distance (m), Damper, Calories, Rate (strokes/min)                   |
      | Stair Climber  | Duration, Floors, Speed level, Calories, Rate (steps/min)                      |
      | Outdoor Run    | Duration, Distance (km), Calories                                              |

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

  @D39
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
      | machine        | valid             | field    | value | message                       |
      | Rowing Machine | 0:10:00, 2000 m   | level    | 5.5   | Damper must be a whole number |
      | Rowing Machine | 0:10:00, 2000 m   | calories | abc   | Calories must be a number     |
      | Outdoor Run    | 0:25:30           | distance | 0     | Distance must be more than 0  |
      | Stair Climber  | 0:15:00           | floors   | 0     | Floors must be more than 0    |

  @FR-07
  Scenario: A cardio entry with no distance can't be saved
    Given I have added "Outdoor Run"
    And I have logged Outdoor Run: 0:25:30
    When I save the workout
    Then Outdoor Run shows "Enter distance"
    And I am still on New Workout

  @D40
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

  @D38
  Scenario: After a failed Save each change re-checks the entry
    Given I have added "Treadmill"
    And I have logged Treadmill: 0:20:00, 3.2 km
    And I have entered "1.255" in Treadmill incline
    And I have saved the workout
    When I enter "1.5" in Treadmill incline
    Then Treadmill shows no errors

  @D197
  Scenario: An outdoor run shows its pace worked out from distance and duration
    Given I have added "Outdoor Run"
    Then Outdoor Run shows the worked-out "Pace —"
    When I log Outdoor Run: 0:25:30, 5 km
    Then Outdoor Run shows the worked-out "Pace 5:06/km"

  @D197
  Scenario: A treadmill shows its speed worked out from distance and duration
    Given I have added "Treadmill"
    When I log Treadmill: 0:30:00, 5.2 km
    Then Treadmill shows the worked-out "Speed 10.4 km/h"
