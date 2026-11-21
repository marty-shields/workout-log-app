Feature: Settings
  The gear on This week opens Settings; back returns to This week. The status bar keeps light icons over the top bar in every theme.

  @smoke @release
  Scenario: The gear on This week opens Settings
    When I tap Settings on This week
    Then I am on Settings

  Scenario: Settings shows the app version
    Given I have opened Settings
    Then the app version is listed on Settings

  Scenario Outline: Back returns to This week
    Given I have opened Settings
    When I go back with <how>
    Then I am on This week
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

  Scenario Outline: A theme saved by an earlier version still applies
    Given the phone's dark theme is <phone>
    And an earlier version saved the <app> theme
    When I close the app and open it again
    Then the <app> theme is applied
    And the status bar icons are light
    When I tap Settings on This week
    Then the <app> theme is selected in Settings
    Examples:
      | phone | app   |
      | off   | Dark  |
      | on    | Light |
