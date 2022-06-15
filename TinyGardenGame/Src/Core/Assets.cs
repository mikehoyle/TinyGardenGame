using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using MonoGame.Aseprite.Graphics;
using static TinyGardenGame.Core.Assets;
using static TinyGardenGame.Core.SpriteName;

namespace TinyGardenGame.Core {
  public enum SpriteName {
    // Player
    PlayerSprite,
    
    // Overlay
    ValidTileSprite,
    
    // Plants
    MarigoldSprite,
  }
  
  public readonly struct Asset {
    public readonly string Path;
    private readonly Dictionary<SpriteName, AtlasItem> _atlas;

    public AtlasItem this[SpriteName sprite] => _atlas[sprite]; 

    public Asset(string path, Dictionary<SpriteName, AtlasItem> atlas) {
      Path = path;
      _atlas = atlas;
    }
  }

  public readonly struct AtlasItem {
    // Region within the texture. If null, is whole texture
    public readonly Rectangle? Region;
    public readonly Vector2 Origin;

    public AtlasItem(float originX, float originY, Rectangle? region = null) {
      Region = region;
      Origin = new Vector2(originX, originY);
    }
  }

  public readonly struct Assets {
    // Tiles / Map
    public const string TileSprites = "tilesets/tile_sprites";
      
    // Sprites
    public const string TestPlantSprites = "sprites/test_plant_sprites";
    
    // Aseprite spritesheets
    public static readonly Asset PlayerAsset = new Asset(
        "sprites/ffa_test_spritesheet",
        new Dictionary<SpriteName, AtlasItem> {
            { PlayerSprite, new AtlasItem(10, 28) }
        });
    public static readonly Asset ValidSquare = new Asset(
        "sprites/valid_square",
        new Dictionary<SpriteName, AtlasItem> {
            { ValidTileSprite, new AtlasItem(16, 0) }
        });
    public const string InventoryContainer = "sprites/inventory_border";
    public const string InventorySelected = "sprites/inventory_selected";
    
    
    // Aseprite spritesheets: Plants
    public static readonly Asset SmallFlowersAsset = new Asset(
        "sprites/first_flower",
        new Dictionary<SpriteName, AtlasItem> {
            { MarigoldSprite, new AtlasItem(16, 32, new Rectangle(0, 0, 32, 48)) }
        });
      
    // Fonts
    public const string ConsoleFont = "ConsoleFont";
    
    
    // Helper functions
    public static AnimatedSprite Load(ContentManager content, Asset asset, SpriteName spriteName) {
      return new AnimatedSprite(
          content.Load<AsepriteDocument>(asset.Path)) {
          Origin = asset[spriteName].Origin,
          SourceRectangle = asset[spriteName].Region.GetValueOrDefault(),
      };
    }
  }

  public static class AssetLoading {
    private static readonly List<string> Texture2dAssets = new List<string> {
            TileSprites, TestPlantSprites, 
    };
    
    private static readonly List<string> AsepriteAssets = new List<string> {
        InventoryContainer, InventorySelected,
    };
    private static readonly List<Asset> AsepriteAtlasAssets = new List<Asset> {
        PlayerAsset, ValidSquare, SmallFlowersAsset,
    };

    private static readonly List<string> SpriteFontAssets = new List<string> { ConsoleFont };

    public static void LoadAllAssets(ContentManager contentManager) {
      foreach (var asset in Texture2dAssets) {
        contentManager.Load<Texture2D>(asset);
      }

      foreach (var asset in AsepriteAssets) {
        contentManager.Load<AsepriteDocument>(asset);
      }
      
      foreach (var asset in AsepriteAtlasAssets) {
        contentManager.Load<AsepriteDocument>(asset.Path);
      }
      
      foreach (var asset in SpriteFontAssets) {
        contentManager.Load<SpriteFont>(asset);
      }
    }
  }
}