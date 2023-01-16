using System;
using System.Linq;
using System.Threading.Tasks;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using NLog;

namespace TinyGardenGame.Core {
  // TODO: This special static class is error prone
  public static class AssetLoading {

    public static Task LoadAllAssets(ContentManager contentManager, string assetConfigPath) {
      // OPTIMIZE: This in theory duplicates a lot. Even though contentManager caches, it could
      // cause churn on the heap.
      return Task.Run(() => {
        var logger = LogManager.GetLogger("LoadAllAssets_closure");
        foreach (var sprite in Vars.Sprite.Items) {
          contentManager.Load<AsepriteDocument>(sprite.Value.Path);
        }

        foreach (var font in Vars.SpriteFont.Items) {
          contentManager.Load<SpriteFont>(font.Value.Path);
        }
        
        foreach (var font in Vars.BmpFont.Items) {
          contentManager.Load<BitmapFont>(font.Value.Path);
        }
        
        logger.Info("Successfully loaded all assets");
      });
    }
  }

  public static class ContentManagerExtensions {
    public static AsepriteAnimatedSprite LoadAnimated(
        this ContentManager content, Vars.Sprite.Type spriteName) {
      var item = Vars.Sprite.Items[spriteName];
      var asepriteDocument = content.Load<AsepriteDocument>(item.Path);
      var sprite = new AsepriteAnimatedSprite(asepriteDocument) {
          Origin = GetOrigin(item, asepriteDocument),
      };
      if (item.HasAtlasRect) {
        sprite.SourceRectangle = item.AtlasRect.ToRect().Value;
      }

      return sprite;
    }

    public static Sprite LoadSprite(this ContentManager content, Vars.Sprite.Type spriteName) {
      var item = Vars.Sprite.Items[spriteName];
      var asepriteDocument = content.Load<AsepriteDocument>(item.Path);
      var sprite = item.HasAtlasRect
          ? new Sprite(new TextureRegion2D(asepriteDocument.Texture, item.AtlasRect.ToRect().Value))
          : new Sprite(asepriteDocument.Texture);
      sprite.Origin = GetOrigin(item, asepriteDocument);
      return sprite;
    }

    public static Texture2D LoadTexture(this ContentManager content, Vars.Sprite.Type spriteName) {
      var item = Vars.Sprite.Items[spriteName];
      return content.Load<AsepriteDocument>(item.Path).Texture;
    }

    public static NinePatchRegion2D LoadNinepatch(
        this ContentManager content, Vars.Sprite.Type spriteName) {
      var item = Vars.Sprite.Items[spriteName];
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

    public static SpriteFont LoadFont(this ContentManager content, Vars.SpriteFont.Type spriteName) {
      return content.Load<SpriteFont>(Vars.SpriteFont.Items[spriteName].Path);
    }

    public static BitmapFont LoadBmpFont(this ContentManager content, Vars.BmpFont.Type spriteName) {
      return content.Load<BitmapFont>(Vars.BmpFont.Items[spriteName].Path);
    }

    // TODO: Support slices for atlas rect as well
    private static Vector2 GetOrigin(Vars.Sprite item, AsepriteDocument document) {
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