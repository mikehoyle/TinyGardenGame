using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using static TinyGardenGame.Core.Assets;

namespace TinyGardenGame.Core {
  // TODO: Extend this idea to create TextureAtlas's for spritesheets
  public readonly struct Assets {
    // Tiles / Map
    public const string TileSprites = "tilesets/tile_sprites";
      
    // Sprites
    public const string TestPlantSprites = "sprites/test_plant_sprites";
    
    // Aseprite spritesheets
    public const string TestAnimatedPlayerSprite = "sprites/ffa_test_spritesheet";
    public const string InventoryContainer = "sprites/inventory_border";
    public const string InventorySelected = "sprites/inventory_selected";
    public const string ValidSquare = "sprites/valid_square";
    
    // Aseprite spritesheets: Plants
    public const string FirstFlower = "sprites/first_flower";
      
    // Fonts
    public const string ConsoleFont = "ConsoleFont";
  }

  public static class AssetLoading {
    
    
    private static readonly List<string> Texture2dAssets = new List<string> {
            TileSprites, TestPlantSprites, 
    };
    
    private static readonly List<string> AsepriteAssets = new List<string> {
        TestAnimatedPlayerSprite, InventoryContainer, InventorySelected, ValidSquare, FirstFlower,
    };

    private static readonly List<string> SpriteFontAssets = new List<string> { ConsoleFont };

    public static void LoadAllAssets(ContentManager contentManager) {
      foreach (var asset in Texture2dAssets) {
        contentManager.Load<Texture2D>(asset);
      }

      foreach (var asset in AsepriteAssets) {
        contentManager.Load<AsepriteDocument>(asset);
      }
      
      foreach (var asset in SpriteFontAssets) {
        contentManager.Load<SpriteFont>(asset);
      }
    }
  }
}