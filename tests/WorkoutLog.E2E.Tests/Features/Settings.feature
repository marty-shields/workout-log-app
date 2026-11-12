Feature: Settings
  The gear on the Dashboard opens Settings; back returns to the Dashboard. The status bar keeps light icons over the top bar in every theme.

  @smoke @release
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

  Scenario Outline: The status bar icons stay light on the dark top bar
    Given the phone's dark theme is <phone>
    And I have opened Settings
    When I choose the <app> theme
    Then the status bar icons are light
    Examples:
      | phone | app    |
      | off   | System |
      | off   | Light  |
      | off   | Dark   |
      | on    | System |
      | on    | Light  |
      | on    | Dark   |
