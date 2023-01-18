using System;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Config;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.Plants;
using TinyGardenGame.Plants.Components;

namespace TinyGardenGame.Vars;

/// <summary>
/// Extends generated var
/// </summary>
public partial class Plant {
  private int _growthStages;
  public delegate bool CanGrowOn(MapTile tile);
  public Vector2 FootprintVec => new(FootprintSize.Width, FootprintSize.Height);
  public RectangleF CollisionRect => new(
      CollisionFootprint.X, CollisionFootprint.Y, CollisionFootprint.Width, CollisionFootprint.Height);
  public CanGrowOn GrowthCondition {
    get {
      return Id switch {
          Type.Marigold => WaterProximityGrowthCondition(3),
          Type.Reeds => WaterProximityGrowthCondition(1),
          Type.GreatOak => WaterProximityGrowthCondition(8),
          _ => WaterProximityGrowthCondition(3),
      };
    }
  }
  public int GrowthStages {
    get {
      if (_growthStages == 0) {
        _growthStages = GetGrowthStages(LoadSprite());
      }

      return _growthStages;
    }
  }

  public void Instantiate(Vector2 position, Func<Entity> createEntity) {
    var drawable = new DrawableComponent(new AnimatedSpriteDrawable(LoadSprite()));
    var entity = createEntity()
        .AttachAnd(drawable)
        .AttachAnd(new PositionComponent(position, footprintSize: FootprintVec));
    if ((CollisionFootprint.Height > 0) && (CollisionFootprint.Width > 0)) {
      entity.Attach(new CollisionFootprintComponent(CollisionRect));
    }
    
    if (GrowthTimeSecs > 0) {
      var growth = new GrowthComponent(
          TimeSpan.FromSeconds(GrowthTimeSecs), GrowthStages);
      drawable.SetAnimation(growth.CurrentGrowthAnimationName());
      entity.Attach(growth);
    }

    if (Tag != null) {
      entity.SetTag(Tag);
    }
  }

  public int CreateGhost(Vector2 position, Entity existingEntity) {
    var originalSprite = LoadSprite();
    var sprite = new MonoGame.Extended.Sprites.Sprite(
        new TextureRegion2D(originalSprite.Texture, originalSprite.Frames[0].Bounds)) {
        Origin = originalSprite.Origin,
        Alpha = GameConfig.Config.BuildGhostOpacity,
    };

    return existingEntity.AttachAnd(new DrawableComponent(sprite))
        .AttachAnd(new PositionComponent(position, footprintSize: FootprintVec))
        .Id;
  }

  private AsepriteAnimatedSprite LoadSprite() {
    return Platform.Content.LoadAnimated(Sprite.Id);
  }

  private static CanGrowOn WaterProximityGrowthCondition(int proximity) {
    return tile => !tile.ContainsWater &&
        !tile.IsNonTraversable &&
        (tile.WaterProximity != 0) &&
        (tile.WaterProximity <= proximity);
  }

  private static int GetGrowthStages(AsepriteAnimatedSprite sprite) {
    var stages = 1;
    while (sprite.Animations.ContainsKey($"{GrowthComponent.GrowthAnimationPrefix}{stages + 1}")) {
      stages++;
    }

    return stages;
  }
}