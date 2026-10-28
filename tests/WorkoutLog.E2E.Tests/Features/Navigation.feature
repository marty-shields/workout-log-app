@US-28
Feature: Back navigation
  Android back does what the on-screen back or Cancel does.

  Scenario Outline: Android back from each screen
    Given I am on <screen>
    When I press Android back
    Then <result>
    Examples:
      | screen                               | result                                         |
      | the add-exercise list on Edit Workout | the add-exercise list is closed on Edit Workout |
      | the Dashboard                        | the app is in the background                   |

  Scenario: Leaving with back and coming back shows the Dashboard
    Given I have pressed Android back on the Dashboard
    When I open the app again
    Then I am on the Dashboard
