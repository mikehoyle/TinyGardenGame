using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.Plants.Components;
using static TinyGardenGame.Plants.PlantType;

namespace TinyGardenGame.Plants {
  public enum PlantType {
    Marigold,
    Reeds,
  }
  
  public class PlantEntityFactory {
    
    private class PlantMetadata {
      private int _growthStages;
      public Func<AnimatedSprite> Sprite { get; set; }
      public Vector2 FootprintSize { get; set; } = Vector2.One;
      public RectangleF CollisionFootprint { get; set; } = new RectangleF(0, 0, 0, 0);
      public double GrowthTimeSecs { get; set; }
      
      public CanGrowOn GrowthCondition { get; set; }

      public int GrowthStages {
        get {
          if (_growthStages == 0) {
            _growthStages = GetGrowthStages(Sprite());
          }
          return _growthStages;
        }
      }
    }

    private readonly Dictionary<PlantType, PlantMetadata> _plantAssets;
    private readonly Config.Config _config;
    private readonly Func<Entity> _createEntity;

    public delegate bool CanGrowOn(MapTile tile);

    public PlantEntityFactory(
        Config.Config config,
        ContentManager content,
        Func<Entity> createEntity) {
      _config = config;
      _createEntity = createEntity;
      _plantAssets = new Dictionary<PlantType, PlantMetadata> {
          [Marigold] = new PlantMetadata {
              Sprite = () => content.LoadAnimated(SpriteName.Marigold),
              GrowthTimeSecs = 45,
              GrowthCondition = WaterProximityGrowthCondition(3),
              CollisionFootprint = new RectangleF(0.25f, 0.25f, 0.5f, 0.5f),
          },
          [Reeds] = new PlantMetadata {
              Sprite = () => content.LoadAnimated(SpriteName.Reeds),
              GrowthTimeSecs = 30,
              GrowthCondition = WaterProximityGrowthCondition(1),
          }
      };
    }

    public Vector2 GetPlantFootprintSize(PlantType type) {
      return _plantAssets[type].FootprintSize;
    }

    public CanGrowOn GetPlantGrowthCondition(PlantType type) {
      return _plantAssets[type].GrowthCondition;
    }
    
    public void CreatePlant(PlantType type, Vector2 position) {
      var metadata = _plantAssets[type];
      var drawable = new DrawableComponent(new AnimatedSpriteDrawable(metadata.Sprite()));
      var growth = new GrowthComponent(
          TimeSpan.FromSeconds(metadata.GrowthTimeSecs), metadata.GrowthStages);
      drawable.SetAnimation(growth.CurrentGrowthAnimationName());
      _createEntity()
          .AttachAnd(drawable)
          .AttachAnd(growth)
          .AttachAnd(new PositionComponent(position, footprintSize: metadata.FootprintSize))
          .AttachAnd(new CollisionFootprintComponent(metadata.CollisionFootprint));
    }

    /**
     * Creates a no-collision ghost plant for use as a visual indicator.
     * @returns entity id of the new entity.
     */
    public int CreateGhostPlant(PlantType type, Vector2 position) {
      var originalSprite = _plantAssets[type].Sprite();
      var sprite = new MonoGame.Extended.Sprites.Sprite(
          new TextureRegion2D(originalSprite.Texture, originalSprite.Frames[0].Bounds)) {
          Origin = originalSprite.Origin,
          Alpha = _config.BuildGhostOpacity,
      };
      return _createEntity()
          .AttachAnd(new DrawableComponent(sprite))
          .AttachAnd(new PositionComponent(
              position, footprintSize: _plantAssets[type].FootprintSize))
          .Id;
    }

    private static CanGrowOn WaterProximityGrowthCondition(int proximity) {
      return tile => !tile.ContainsWater
                     && !tile.IsNonTraversable
                     && tile.WaterProximity != 0
                     && tile.WaterProximity <= proximity;
    }

    private static int GetGrowthStages(AnimatedSprite sprite) {
      int stages = 1;
      while (sprite.Animations.ContainsKey(
                 $"{GrowthComponent.GrowthAnimationPrefix}{stages + 1}")) {
        stages++;
      }

      return stages;
    }
  }
}