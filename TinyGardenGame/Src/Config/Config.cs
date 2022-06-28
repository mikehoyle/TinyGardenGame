namespace TinyGardenGame.Config {
  /**
   * Encapsulates readonly configuration, meant to be initialized once
   * during game start.
   * TODO: enable initializing from serialized file.
   */
  public class Config {
    public readonly float PlayerSpeed = 3.0f;
    public readonly int FpsCap = 144;
    public readonly bool ShowBuildHints = true;
    public readonly bool ShowBuildGhost = true;
    public readonly float BuildGhostOpacity = 0.45f;
    public readonly DebugConfig Debug = new DebugConfig();
    public readonly string AssetsConfigPath = "Src/Config/Assets.toml";

    // Map Generation
    public readonly int MapWidth = 1000;
    public readonly int MapHeight = 1000;
    public readonly int MapGenerationSeed = 42;
    public readonly int OceanBorderWidth = 3;
    public readonly int StartingAreaRadius = 6;
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