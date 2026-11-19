Feature: Trends
  Trends shows a week, a month or a year, stepped with the arrows. A day, a week or a month with workouts opens them in Workouts, and back returns to the same period.

  Scenario: Trends opens on the current week
    When I open Trends
    Then Trends is on the Week view
    And Trends is on the current period

  Scenario: The arrows step back and forward
    Given I have opened Trends
    And I have noted the period shown on Trends
    When I step back 1 period on Trends
    Then Trends is on a different period than the one I noted
    And Trends is on an earlier period
    When I step forward on Trends
    Then Trends is on the period I noted
    And Trends is on the current period

  Scenario Outline: Each view can be chosen
    Given I have opened Trends
    When I choose the <view> view on Trends
    Then Trends is on the <view> view
    And Trends is on the current period
    Examples:
      | view  |
      | Month |
      | Year  |
      | Week  |

  @today
  Scenario: The tiles count the period's workouts
    Given these saved workouts:
      | started     | exercises             |
      | today 00:00 | Push-up: 10 reps 0 kg |
      | today 00:02 | Push-up: 10 reps 0 kg |
    When I open Trends
    Then Trends counts 2 workouts
    When I choose the Month view on Trends
    Then Trends counts 2 workouts

  @today
  Scenario: Only days with a workout can be opened
    Given these saved workouts:
      | started     | exercises             |
      | today 00:00 | Push-up: 10 reps 0 kg |
    And I have opened Trends
    When I choose the Month view on Trends
    Then the Trends calendar lets me open only the days today

  @today
  Scenario Outline: A week row, a day and a day's bars open their workouts, and back returns to the same period
    Given these saved workouts:
      | started     | exercises             |
      | today 00:00 | Push-up: 10 reps 0 kg |
      | today 00:02 | Push-up: 12 reps 0 kg |
    And I have opened Trends
    When I <open>
    Then I am on Workouts
    And the rows read, in order:
      | today 00:02 |
      | today 00:00 |
    When I go back from Workouts with <how>
    Then I am on Trends
    And Trends is on the <view> view
    And Trends is on the current period
    Examples:
      | open                                                                | how            | view  |
      | open the workouts of today from the Trends week view                | the back arrow | Week  |
      | open the workouts of today from the Trends calendar                 | Android back   | Month |
      | open the workouts of the week of today from the Trends weeks        | the back arrow | Month |
      | open the workouts of the month of today from the Trends year view   | Android back   | Year  |

  @today
  Scenario: Opening an earlier month's day steps back to it, and back shows that month again
    Given these saved workouts:
      | started           | exercises             |
      | 40 days ago 10:00 | Push-up: 10 reps 0 kg |
      | today 00:00       | Push-up: 10 reps 0 kg |
    And I have opened Trends
    When I show the month of 40 days ago on Trends
    And I note the period shown on Trends
    And I open the workouts of 40 days ago from the Trends calendar
    Then the rows read, in order:
      | 40 days ago 10:00 |
    When I go back from Workouts with the back arrow
    Then I am on Trends
    And Trends is on the Month view
    And Trends is on the period I noted

  @today
  Scenario: An exercise opens from Most trained, and back returns to the same month
    Given these saved workouts:
      | started     | exercises                        |
      | today 00:00 | Barbell Back Squat: 5 reps 60 kg |
    And I have opened Trends
    When I show the month of today on Trends
    And I note the period shown on Trends
    And I tap "Barbell Back Squat" in Most trained on Trends
    Then the Exercise page is titled "Barbell Back Squat" with "Quadriceps · Barbell"
    When I go back with the back arrow
    Then I am on Trends
    And Trends is on the Month view
    And Trends is on the period I noted

  Scenario Outline: Settings opened from Trends returns to Trends
    Given I have opened Trends
    And I have chosen the Month view on Trends
    And I have noted the period shown on Trends
    When I tap Settings on Trends
    Then I am on Settings
    When I go back with <how>
    Then I am on Trends
    And Trends is on the Month view
    And Trends is on the period I noted
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
