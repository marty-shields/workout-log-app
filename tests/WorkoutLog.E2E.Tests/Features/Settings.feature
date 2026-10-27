@FR-36 @FR-50
Feature: Settings
  The gear on the Dashboard opens Settings.

  @smoke
  Scenario: The gear on the Dashboard opens Settings
    When I tap Settings on the Dashboard
    Then I am on Settings

  Scenario Outline: Back returns to the Dashboard
    Given I have opened Settings
    When I go back with <how>
    Then I am on the Dashboard
    Examples:
      | how            |
      | the back arrow |
      | Android back   |
