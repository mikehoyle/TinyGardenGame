using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using static TinyGardenGame.Core.Assets;
using static TinyGardenGame.Core.SpriteName;
using static TinyGardenGame.MapPlacementHelper;
using AnimatedSprite = MonoGame.Aseprite.Graphics.AnimatedSprite;

namespace TinyGardenGame.Core {
  public enum SpriteName {
    // Player
    PlayerSprite,
    
    // Overlay
    ValidTileSprite,
    SelectedTileOverlay,
    
    // Plants
    MarigoldSprite,
    
    // Tiles: Water
    Water0,
    Water1,
    Water2Nw,
    Water2Sw,
    Water2Ew,
    Water3,
    Water4,
    
    // Tiles: Base
    Weeds1,
    Weeds2,
    Weeds3,
    
    Sand1,
    Sand2,
    Sand3,
    
    Rocks1,
    Rocks2,
    Rocks3,
  }

  public class AtlasItem {
    // Region within the texture. If null, is whole texture
    public string Path { get; }
    public readonly Rectangle? Region;
    public readonly Vector2 Origin;

    public AtlasItem(string path, float originX, float originY, Rectangle? region = null) {
      Path = path;
      Region = region;
      Origin = new Vector2(originX, originY);
    }
  }

  public class TileAtlasItem : AtlasItem {
    public TileAtlasItem(string path, Rectangle region) : base(
        path,
        HalfTileWidthPixels,
        region.Height - TileHeightPixels,
        region) {}
  }

  public class Assets {
    public static AtlasItem GetItem(SpriteName spriteName) {
      // OPTIMIZE: Cache these
      switch (spriteName) {
        // Player
        case PlayerSprite:
          return new AtlasItem(PlayerAsset, 10, 28);
        
        // Overlay
        case ValidTileSprite:
          return new AtlasItem(ValidSquare, 16, 0);
        case SelectedTileOverlay:
          return new TileAtlasItem(TileSprites, new Rectangle(0, 0, 32, 16));
        
        // Plants
        case MarigoldSprite:
          return new AtlasItem(SmallFlowersAsset, 16, 32, new Rectangle(0, 0, 32, 48));
        
        // Tiles: Water
        case Water0:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 2, 0, TileWidthPixels, TileHeightPixels));
        case Water1:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 3, 0, TileWidthPixels, TileHeightPixels));
        case Water2Nw:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 4, 0, TileWidthPixels, TileHeightPixels));
        case Water2Sw:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 5, 0, TileWidthPixels, TileHeightPixels));
        case Water2Ew:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 6, 0, TileWidthPixels, TileHeightPixels));
        case Water3:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 7, 0, TileWidthPixels, TileHeightPixels));
        case Water4:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 8, 0, TileWidthPixels, TileHeightPixels));
        
        // Tiles: Base
        case Weeds1:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 0, 16, TileWidthPixels, 24));
        case Weeds2:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 1, 16, TileWidthPixels, 24));
        case Weeds3:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 2, 16, TileWidthPixels, 24));
        case Sand1:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 3, 16, TileWidthPixels, 24));
        case Sand2:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 4, 16, TileWidthPixels, 24));
        case Sand3:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 5, 16, TileWidthPixels, 24));
        case Rocks1:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 6, 16, TileWidthPixels, 24));
        case Rocks2:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 7, 16, TileWidthPixels, 24));
        case Rocks3:
          return new TileAtlasItem(
              TileSprites,
              new Rectangle(TileWidthPixels * 8, 16, TileWidthPixels, 24));
        default:
          // TODO: handle this better with "Unknown" sprite to render?
          throw new Exception($"Unknown Sprite: {spriteName}");
      }
    }

    // Sprites
    public const string TestPlantSprites = "sprites/test_plant_sprites";
    
    // Aseprite: Tiles / Map
    public const string TileSprites = "tilesets/tile_sprites";
    
    // Aseprite spritesheets
    public const string PlayerAsset = "sprites/ffa_test_spritesheet";
    public const string ValidSquare = "sprites/valid_square";
    public const string InventoryContainer = "sprites/inventory_border";
    public const string InventorySelected = "sprites/inventory_selected";
    
    
    // Aseprite: Plants
    public static readonly string SmallFlowersAsset = "sprites/first_flower";
      
    // Fonts
    public const string ConsoleFont = "ConsoleFont";
  }

  public static class ContentManagerExtensions {
    public static AnimatedSprite LoadAnimated(
        this ContentManager content, SpriteName spriteName) {
      var item = Assets.GetItem(spriteName);
      return new AnimatedSprite(
          content.Load<AsepriteDocument>(item.Path)) {
          Origin = item.Origin,
          SourceRectangle = item.Region.GetValueOrDefault(),
      };
    }
    
    public static MonoGame.Extended.Sprites.Sprite LoadSprite(
        this ContentManager content, SpriteName spriteName) {
      var item = Assets.GetItem(spriteName);
      return new Sprite(
          new TextureRegion2D(
              content.Load<AsepriteDocument>(item.Path).Texture, item.Region.GetValueOrDefault())) {
          Origin = item.Origin,
      };
    }
  }

  public static class AssetLoading {
    private static readonly List<string> Texture2dAssets = new List<string> {
            TestPlantSprites, 
    };
    
    private static readonly List<string> AsepriteAssets = new List<string> {
        InventoryContainer, InventorySelected, PlayerAsset, ValidSquare, SmallFlowersAsset,
        TileSprites,
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