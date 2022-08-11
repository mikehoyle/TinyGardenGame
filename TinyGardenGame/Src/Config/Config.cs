namespace TinyGardenGame.Config {
  /**
   * Encapsulates readonly configuration, meant to be initialized once
   * during game start.
   * TODO: enable initializing from serialized file.
   */
  public class Config {
    // Player
    public readonly float PlayerSpeed = 3.0f;
    public readonly int DefaultMaxHp = 100;
    public readonly int DefaultMinHp = 0;
    public readonly int DefaultMaxEnergy = 100;
    public readonly int DefaultMinEnergy = 0;

    // Global
    public readonly int FpsCap = 144;
    public readonly string AssetsConfigPath = "Src/Config/Assets.toml";

    // Game
    public readonly DebugConfig Debug = new DebugConfig();
    public readonly bool ShowBuildHints = true;
    public readonly bool ShowBuildGhost = true;
    public readonly float BuildGhostOpacity = 0.45f;

    // Game: Time
    public readonly int TotalHoursInADay = 12;
    public readonly int HoursOfNight = 4;
    public readonly int HourLengthInSeconds = 20;

    // Map Generation
    public readonly int MapWidth = 500;
    public readonly int MapHeight = 500;
    public readonly int MapGenerationSeed = 42;
    public readonly int OceanBorderWidth = 3;
    public readonly int StartingAreaRadius = 6;
    public readonly int BiomeSize = 30;
    public readonly int LakeMinRadius = 4;
    public readonly int LakeMaxRadius = 8;
    public readonly float LakeProbabilityPerBiomeInstance = 0.75f;
    public readonly float LakeMinAngleStepRadians = 0.4f;
    public readonly float LakeMaxAngleStepRadians = 1.2f;
    public readonly float LakeVertexVariabilityPercent = 0.65f;
    
    // AI
    public readonly float AiRoamVariationRadiansPerSec = 6f;
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