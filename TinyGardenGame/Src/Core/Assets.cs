using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Config;
using AnimatedSprite = MonoGame.Aseprite.Graphics.AnimatedSprite;

namespace TinyGardenGame.Core {
  // TODO: separate out this massive struct into a few themed structs?
  // Next id: 24
  public enum SpriteName {
    // Player
    Player,
    
    // Overlay
    ValidTile,
    SelectedTileOverlay,
    LoadingBarEmpty,
    LoadingBarFull,
    
    // HUD
    InventoryContainer,
    InventorySelected,
    
    // Inventory
    InventoryReedSeeds,

    // Plants: Player Made
    Marigold,
    Reeds,
    
    // Plants: Weeds
    Dandelion,
    Shrub,

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
    
    // Fonts
    ConsoleFont,
  }

  // TODO: This special static class is error prone
  public static class AssetLoading {
    public static KeyedCollection<SpriteName, Asset> Assets =
        new KeyedCollection<SpriteName, Asset>(x => x.Name);
    public static KeyedCollection<SpriteName, FontAsset> Fonts =
        new KeyedCollection<SpriteName, FontAsset>(x => x.Name);
    public static Task LoadAllAssets(ContentManager contentManager, string assetConfigPath) {
      // OPTIMIZE: This in theory duplicates a lot. Even though contentManager caches, it could
      // cause churn on the heap.
      return Task.Run(() => {
        var assetsModel = AssetsModel.Load(assetConfigPath);
        foreach (var asset in assetsModel.Assets) {
          contentManager.Load<AsepriteDocument>(asset.Path);
          Assets.Add(asset);
        }
      
        foreach (var asset in assetsModel.Fonts) {
          contentManager.Load<SpriteFont>(asset.Path);
          Fonts.Add(asset);
        }
      });
    }
  }
  
  public static class ContentManagerExtensions {
    public static AnimatedSprite LoadAnimated(this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      var sprite = new AnimatedSprite(
          content.Load<AsepriteDocument>(item.Path)) {
          Origin = item.Origin.ToVec(),
      };
      if (item.HasAtlasRect) {
        sprite.SourceRectangle = item.AtlasRect.ToRect().Value;
      }

      return sprite;
    }
    
    public static Sprite LoadSprite(this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      var sprite = item.HasAtlasRect
          ? new Sprite(new TextureRegion2D(
              content.Load<AsepriteDocument>(item.Path).Texture, item.AtlasRect.ToRect().Value))
          : new Sprite(content.Load<AsepriteDocument>(item.Path).Texture);
      sprite.Origin = item.Origin.ToVec();
      return sprite;
    }
    
    public static Texture2D LoadTexture(this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      return content.Load<AsepriteDocument>(item.Path).Texture;
    }
    
    public static SpriteFont LoadFont(this ContentManager content, SpriteName spriteName) {
      return content.Load<SpriteFont>(AssetLoading.Fonts[spriteName].Path);
    }
  }
}