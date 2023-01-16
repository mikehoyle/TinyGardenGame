namespace TinyGardenGame;

/// <summary>
/// Quick and dirty singleton that contains everything needed as globally accessible,
/// to avoid ridiculous dependency chains and constructor bloat.
/// Everything here is expected to be available for lifetime of the program.
/// </summary>
public static class Platform { 
  public static ContentManager Content { get; private set; }
  
  public static void Init(
      ContentManager contentManager) {
    Content = contentManager;
  }
}