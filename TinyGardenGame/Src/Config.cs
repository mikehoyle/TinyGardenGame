namespace TinyGardenGame {
  /**
   * Encapsulates readonly configuration, meant to be initialized once
   * during game start.
   * TODO: enable initializing from serialized file.
   */
  public class Config {
    public readonly float PlayerSpeed = 3.0f;
    public readonly int FpsCap = 144;
    public readonly DebugConfig Debug = new DebugConfig();
  }

  /**
   * Debug-only configuration
   */
  public class DebugConfig {
    public readonly bool ShowSelectionIndicator = true;
    public readonly bool EnableConsole = true;
    public readonly bool EnableFpsDisplay = true;
  }
}