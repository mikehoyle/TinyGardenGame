using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using static TinyGardenGame.Core.Assets;

namespace TinyGardenGame.Core {
  public readonly struct Assets {
    // Tiles / Map
    public const string TestTiledMap = "maps/test-map";
    public const string TileSprites = "tilesets/tile_sprites";
      
    // Sprites
    public const string TestPlantSprites = "sprites/test_plant_sprites";
    public const string TestPlayerSprite = "sprites/Old hero";
      
    // Fonts
    public const string ConsoleFont = "ConsoleFont";
  }

  public static class AssetLoading {
    
    
    private static readonly List<string> Texture2dAssets = new List<string> {
            TileSprites, TestPlantSprites, TestPlayerSprite};

    private static readonly List<string> TiledMapAssets = new List<string> { TestTiledMap };

    private static readonly List<string> SpriteFontAssets = new List<string> { ConsoleFont };

    public static void LoadAllAssets(ContentManager contentManager) {
      foreach (var asset in Texture2dAssets) {
        contentManager.Load<Texture2D>(asset);
      }
      
      foreach (var asset in TiledMapAssets) {
        contentManager.Load<TiledMap>(asset);
      }
      
      foreach (var asset in SpriteFontAssets) {
        contentManager.Load<SpriteFont>(asset);
      }
    }
  }
}