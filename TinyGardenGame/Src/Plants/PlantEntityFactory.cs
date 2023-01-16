#nullable enable
using System;
using System.Collections.Generic;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Config;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.Plants.Components;
using static TinyGardenGame.Plants.PlantType;

namespace TinyGardenGame.Plants;

public enum PlantType {
  GreatOak,
  Marigold,
  Reeds,
}

public class PlantEntityFactory {
  public delegate bool CanGrowOn(MapTile tile);
  private readonly Func<Entity> _createEntity;
  private readonly Dictionary<PlantType, PlantMetadata> _plantAssets;

  public PlantEntityFactory(
      ContentManager content,
      Func<Entity> createEntity) {
    _createEntity = createEntity;
    _plantAssets = new Dictionary<PlantType, PlantMetadata> {
        [Marigold] = new() {
            Sprite = () => content.LoadAnimated(Vars.Sprite.Type.Marigold),
            GrowthTimeSecs = 45,
            GrowthCondition = WaterProximityGrowthCondition(3),
            //CollisionFootprint = new RectangleF(0.25f, 0.25f, 0.5f, 0.5f),
        },
        [Reeds] = new() {
            Sprite = () => content.LoadAnimated(Vars.Sprite.Type.Reeds),
            GrowthTimeSecs = 30,
            GrowthCondition = WaterProximityGrowthCondition(1),
        },
        [GreatOak] = new() {
            Sprite = () => content.LoadAnimated(Vars.Sprite.Type.GreatTree),
            GrowthTimeSecs = 0,
            GrowthCondition = WaterProximityGrowthCondition(8),
            //CollisionFootprint = new RectangleF(0f, 0f, 3f, 3f),
            FootprintSize = new Vector2(3f, 3f),
            CustomSetup = entity => entity.Attach(new GreatTreeComponent()),
        },
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
    var entity = _createEntity()
        .AttachAnd(drawable)
        .AttachAnd(new PositionComponent(position, footprintSize: metadata.FootprintSize))
        .AttachAnd(new CollisionFootprintComponent(metadata.CollisionFootprint));
    if (metadata.GrowthTimeSecs > 0) {
      var growth = new GrowthComponent(
          TimeSpan.FromSeconds(metadata.GrowthTimeSecs),
          metadata.GrowthStages);
      drawable.SetAnimation(growth.CurrentGrowthAnimationName());
      entity.Attach(growth);
    }

    metadata.CustomSetup(entity);
  }

  /**
   * Creates a no-collision ghost plant for use as a visual indicator.
   * @returns entity id of the new entity.
   */
  public int CreateGhostPlant(PlantType type, Vector2 position, Entity? existingEntity = null) {
    var originalSprite = _plantAssets[type].Sprite();
    var sprite = new Sprite(
        new TextureRegion2D(originalSprite.Texture, originalSprite.Frames[0].Bounds)) {
        Origin = originalSprite.Origin,
        Alpha = GameConfig.Config.BuildGhostOpacity,
    };

    existingEntity ??= _createEntity();
    return existingEntity.AttachAnd(new DrawableComponent(sprite))
        .AttachAnd(new PositionComponent(position, footprintSize: _plantAssets[type].FootprintSize))
        .Id;
  }

  private static CanGrowOn WaterProximityGrowthCondition(int proximity) {
    return tile => !tile.ContainsWater &&
                   !tile.IsNonTraversable &&
                   tile.WaterProximity != 0 &&
                   tile.WaterProximity <= proximity;
  }

  private static int GetGrowthStages(AsepriteAnimatedSprite sprite) {
    var stages = 1;
    while (sprite.Animations.ContainsKey($"{GrowthComponent.GrowthAnimationPrefix}{stages + 1}"))
      stages++;

    return stages;
  }

  private class PlantMetadata {
    private int _growthStages;
    public Func<AsepriteAnimatedSprite> Sprite { get; init; }
    public Vector2 FootprintSize { get; init; } = Vector2.One;
    public RectangleF CollisionFootprint { get; } = new(0, 0, 0, 0);
    public double GrowthTimeSecs { get; init; }

    public CanGrowOn GrowthCondition { get; init; }

    public Action<Entity> CustomSetup { get; init; } = (_) => { };

    public int GrowthStages {
      get {
        if (_growthStages == 0) _growthStages = GetGrowthStages(Sprite());

        return _growthStages;
      }
    }
  }
}
