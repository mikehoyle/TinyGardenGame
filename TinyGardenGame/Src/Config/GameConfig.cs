using System.IO;
using Tomlyn;

namespace TinyGardenGame.Config {
  /**
   * Encapsulates configuration, meant to be initialized once during game start.
   * Is a non-threadsafe singleton.
   */
  public record GameConfig {    
    // TODO: CLI flag is ideal for this, doesn't really matter at the moment though
    private const string GameConfigPath = "Config/GameConfig.toml";
    
    // Player
    public float PlayerSpeed { get; init; }
    public int DefaultMaxHp { get; init; }
    public int DefaultMinHp { get; init; }
    public int DefaultMaxEnergy { get; init; }
    public int DefaultMinEnergy { get; init; }

    // Global
    public int FpsCap { get; init; }
    public string AssetsConfigPath { get; init; }

    // Game
    public DebugConfig Debug { get; init; }
    public bool ShowBuildHints { get; init; }
    public bool ShowBuildGhost { get; init; }
    public float BuildGhostOpacity { get; init; }

    // Game: Time
    public int TotalHoursInADay { get; init; }
    public int HoursOfNight { get; init; }
    public int HourLengthInSeconds { get; init; }

    // Map Generation
    public int MapWidth { get; init; }
    public int MapHeight { get; init; }
    public int MapGenerationSeed { get; init; }
    public int OceanBorderWidth { get; init; }
    public int StartingAreaRadius { get; init; }
    public int BiomeSize { get; init; }
    public int LakeMinRadius { get; init; }
    public int LakeMaxRadius { get; init; }
    public float LakeProbabilityPerBiomeInstance { get; init; }
    public float LakeMinAngleStepRadians { get; init; }
    public float LakeMaxAngleStepRadians { get; init; }
    public float LakeVertexVariabilityPercent { get; init; }
    
    // AI
    public float AiRoamVariationRadiansPerSec { get; init; }
    
    // Singleton/Loading stuff
    public static GameConfig Config { get; private set; }

    public static void Init() {
      var tomlText = File.ReadAllText(GameConfigPath);
      var options = new TomlModelOptions {
          ConvertPropertyName = (str) => str,
      };
      Config = Toml.ToModel<GameConfig>(
          tomlText,
          typeof(GameConfig).Assembly.Location,
          options);
    }
  }

  /**
   * Debug-only configuration.
   */
  public record DebugConfig {
    public bool ShowSelectionIndicator { get; init; }
    public bool EnableConsole { get; init; }
    public bool EnableFpsDisplay { get; init; }
  }
}