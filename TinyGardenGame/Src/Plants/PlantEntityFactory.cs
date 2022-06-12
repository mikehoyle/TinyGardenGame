using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Plants.Components;
using TinyGardenGame.Player.Systems;
using static TinyGardenGame.Plants.PlantType;

namespace TinyGardenGame.Plants {
  public enum PlantType {
    WideTestPlant,
    TallTestPlant,
  }
  
  public class PlantEntityFactory {
    private class PlantMetadata {
      public Sprite Sprite { get; set; }
      public Vector2 FootprintSize { get; set; } = Vector2.One;
      public RectangleF CollisionFootprint { get; set; } = new RectangleF(0, 0, 1, 1);
      public double GrowthTimeSecs { get; set; }
    }

    private readonly Dictionary<PlantType, PlantMetadata> _plantAssets;
    private readonly Config _config;
    private readonly Func<Entity> _createEntity;

    public PlantEntityFactory(
        Config config,
        ContentManager contentManager,
        Func<Entity> createEntity) {
      _config = config;
      _createEntity = createEntity;
      var spriteSheet = contentManager.Load<Texture2D>(Assets.TestPlantSprites);
      _plantAssets = new Dictionary<PlantType, PlantMetadata> {
          [WideTestPlant] = new PlantMetadata {
              Sprite = new Sprite(
                  new TextureRegion2D(spriteSheet, 0, 0, 64, 52)) {
                  // TODO create helpers for these magic numbers 
                  // This is the NW corner of the would-be north-west square
                  Origin = new Vector2(32, 20),
              },
              FootprintSize = new Vector2(2, 2),
              CollisionFootprint = new RectangleF(0, 0, 2, 2),
              GrowthTimeSecs = 20,
          },
          [TallTestPlant] = new PlantMetadata {
              Sprite = new Sprite(
                  new TextureRegion2D(spriteSheet, 96, 0, 32, 56)) {
                  Origin = new Vector2(16, 40),
              },
              GrowthTimeSecs = 25,
          },
      };
    }

    public Vector2 GetPlantFootprintSize(PlantType type) {
      return _plantAssets[type].FootprintSize;
    }
    
    public void CreatePlant(PlantType type, Vector2 position) {
      var metadata = _plantAssets[type];
      _createEntity()
          .AttachAnd(new DrawableComponent(metadata.Sprite))
          .AttachAnd(new PositionComponent(position, footprintSize: metadata.FootprintSize))
          .AttachAnd(new CollisionFootprintComponent(metadata.CollisionFootprint))
          .AttachAnd(new GrowthComponent(TimeSpan.FromSeconds(metadata.GrowthTimeSecs)));
    }

    /**
     * Creates a no-collision ghost plant for use as a visual indicator.
     * @returns entity id of the new entity.
     */
    public int CreateGhostPlant(PlantType type, Vector2 position) {
      var metadata = _plantAssets[type];
      var sprite = new Sprite(metadata.Sprite.TextureRegion) {
          Origin = metadata.Sprite.Origin,
          Alpha = _config.BuildGhostOpacity,
      };
      return _createEntity()
          .AttachAnd(new DrawableComponent(sprite))
          .AttachAnd(new PositionComponent(position, footprintSize: metadata.FootprintSize))
          .Id;
    }
  }
}