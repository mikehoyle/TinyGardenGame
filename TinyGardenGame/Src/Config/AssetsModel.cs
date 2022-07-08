using System;
using System.Collections.Generic;
using System.IO;//In InventoryInventoryReedSeeds,Reeds,
using Microsoft.Xna.Framework;
using TinyGardenGame.Core;
using Tomlyn;

namespace TinyGardenGame.Config {
  public class AssetsModel {
    public List<Asset> Assets { get; set; }
    public List<FontAsset> Fonts { get; set; }

    public static AssetsModel Load(string configPath) {
      var tomlText = File.ReadAllText(configPath);
      var options = new TomlModelOptions {
          ConvertPropertyName = (str) => str,
          ConvertToModel = (field, type) => {
            if (field is string s && type == typeof(SpriteName)) {
              return Enum.Parse(typeof(SpriteName), s, ignoreCase: true);
            }

            return null;
          },
      };
      return Toml.ToModel<AssetsModel>(
          tomlText,
          typeof(AssetsModel).Assembly.Location,
          options);
    }
  }
  
  public class Asset {
    public SpriteName Name { get; set; }
    public string Path { get; set; }

    public RectData AtlasRect { get; set; } = new RectData();

    public bool HasAtlasRect => AtlasRect.Height != -1 && AtlasRect.Width != -1;

    public OriginData Origin { get; set; } = new OriginData();

    public class OriginData {
      public int X { get; set; } = 0;
      public int Y { get; set; } = 0;

      public Vector2 ToVec() {
        return new Vector2(X, Y);
      }
    }

    public class RectData {
      public int X { get; set; } = 0;
      public int Y { get; set; } = 0;
      public int Width { get; set; } = -1;
      public int Height { get; set; } = -1;

      public Rectangle? ToRect() {
        if (Width == -1 || Height == -1) {
          return null;
        }
        
        return new Rectangle(X, Y, Width, Height);
      }
    }
  }

  public class FontAsset {
    public SpriteName Name { get; set; }
    public string Path { get; set; }
  } 
}