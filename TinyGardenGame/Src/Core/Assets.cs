using System;
using System.Linq;
using System.Threading.Tasks;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Config;

namespace TinyGardenGame.Core {
  // TODO: separate out this massive enum into a few themed enums?
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
    ProgressBarFillEmpty,
    ProgressBarFillHp,
    ProgressBarFillEnergy,

    // Inventory
    InventoryReedSeeds,
    InventoryGreatAcorn,

    // Plants: Player Made
    GreatTree,
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

    DryDirt1,
    DryDirt2,
    DryDirt3,

    FlowerPatch1,
    FlowerPatch2,
    FlowerPatch3,

    // Tools
    HandTool,
    TillerTool,
    ShovelTool,

    // Fonts
    ConsoleFont,

    // Bmp Fonts
    CourierFont,
  }

  // TODO: This special static class is error prone
  public static class AssetLoading {
    public static KeyedCollection<SpriteName, Asset> Assets = new(x => x.Name);

    public static KeyedCollection<SpriteName, FontAsset> Fonts = new(x => x.Name);

    public static KeyedCollection<SpriteName, FontAsset> BmpFonts = new(x => x.Name);

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

        foreach (var asset in assetsModel.BmpFonts) {
          contentManager.Load<BitmapFont>(asset.Path);
          BmpFonts.Add(asset);
        }
      });
    }
  }

  public static class ContentManagerExtensions {
    public static AsepriteAnimatedSprite LoadAnimated(
        this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      var asepriteDocument = content.Load<AsepriteDocument>(item.Path);
      var sprite = new AsepriteAnimatedSprite(asepriteDocument) {
          Origin = GetOrigin(item, asepriteDocument),
      };
      if (item.HasAtlasRect) {
        sprite.SourceRectangle = item.AtlasRect.ToRect().Value;
      }

      return sprite;
    }

    public static Sprite LoadSprite(this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      var asepriteDocument = content.Load<AsepriteDocument>(item.Path);
      var sprite = item.HasAtlasRect
          ? new Sprite(new TextureRegion2D(asepriteDocument.Texture, item.AtlasRect.ToRect().Value))
          : new Sprite(asepriteDocument.Texture);
      sprite.Origin = GetOrigin(item, asepriteDocument);
      return sprite;
    }

    public static Texture2D LoadTexture(this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      return content.Load<AsepriteDocument>(item.Path).Texture;
    }

    public static NinePatchRegion2D LoadNinepatch(
        this ContentManager content, SpriteName spriteName) {
      var item = AssetLoading.Assets[spriteName];
      var document = content.Load<AsepriteDocument>(item.Path);
      if (document.Slices.TryGetValue("nine_patch", out var slice)) {
        foreach (var key in slice.SliceKeys.Values) {
          if (key.HasNinePatch) {
            var leftPadding = key.CenterX - key.X;
            var topPadding = key.CenterY - key.Y;
            var texture = content.LoadSprite(spriteName).TextureRegion;
            return new NinePatchRegion2D(
                texture,
                leftPadding,
                topPadding,
                rightPadding: texture.Width - key.Width - leftPadding,
                bottomPadding: texture.Height - key.Height - topPadding);
          }
        }
      }

      // TODO: handle this more cleanly
      throw new Exception($"{spriteName} doesn't have a nine_patch slice");
    }

    public static SpriteFont LoadFont(this ContentManager content, SpriteName spriteName) {
      return content.Load<SpriteFont>(AssetLoading.Fonts[spriteName].Path);
    }

    public static BitmapFont LoadBmpFont(this ContentManager content, SpriteName spriteName) {
      return content.Load<BitmapFont>(AssetLoading.BmpFonts[spriteName].Path);
    }

    // TODO: Support slices for atlas rect as well
    private static Vector2 GetOrigin(Asset item, AsepriteDocument document) {
      if (document.Slices.Count == 1) {
        foreach (var sliceKey in document.Slices.Values.First().SliceKeys.Values) {
          if (sliceKey.HasPivot) {
            return new Vector2(sliceKey.PivotX, sliceKey.PivotY);
          }
        }
      }
      
      return item.Origin.ToVec();
    }
  }
}