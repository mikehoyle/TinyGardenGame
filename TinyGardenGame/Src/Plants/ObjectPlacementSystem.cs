using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.Systems;
using TinyGardenGame.Screens;
using Color = Microsoft.Xna.Framework.Color;

namespace TinyGardenGame.Plants {
  /**
   * Manages the place-ability of objects (such as plants) on the map.
   *
   * Also the display of placement overlay 
   */
  public class ObjectPlacementSystem : EntityUpdateSystem, IAttemptPlantPlacement {
    private readonly PrimaryGameplayScreen _gameScreen;
    private readonly GameMap _map;
    private readonly IIsSpaceOccupied _isSpaceOccupied;
    private readonly CameraSystem _cameraSystem;
    private readonly PlantEntityFactory _plantFactory;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private GhostPlant? _ghostPlant;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private readonly Sprite _validSquareSprite;

    // i.e. hovered in inventory, attempting to build
    public PlantType? HoveredPlant { get; set; }

    public ObjectPlacementSystem(
        PrimaryGameplayScreen gameScreen,
        GameMap map,
        IIsSpaceOccupied isSpaceOccupied, 
        CameraSystem cameraSystem) : base(Aspect.All()){
      _gameScreen = gameScreen;
      _map = map;
      _isSpaceOccupied = isSpaceOccupied;
      _cameraSystem = cameraSystem;
      _plantFactory = new PlantEntityFactory(
          gameScreen.Game.Config, gameScreen.Game.Content, CreateEntity);
      _validSquareSprite = gameScreen.Game.Content.LoadSprite(
          SpriteName.ValidTileSprite);
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
    }

    public override void Update(GameTime gameTime) {
      UpdateBuildGhost();

      if (_gameScreen.Game.Config.ShowBuildHints && HoveredPlant != null) {
        _map.ForEachTileInBounds(_cameraSystem.Camera.BoundingRectangle, (x, y, tile) => {
          // TODO update cache
        });
      }
    }

    private void UpdateBuildGhost() {
      if (_gameScreen.Game.Config.ShowBuildGhost
          && HoveredPlant.HasValue
          && _selectionComponentMapper.Components.Count > 0) {
        if (_ghostPlant.HasValue && _ghostPlant.Value.PlantType != HoveredPlant) {
          DestroyEntity(_ghostPlant.Value.EntityId);
        }
        var placement = _selectionComponentMapper.Get(_gameScreen.PlayerEntity.Id)
            .SelectedSquare;
        _ghostPlant ??= new GhostPlant {
            EntityId = _plantFactory.CreateGhostPlant(HoveredPlant.Value, placement),
            PlantType = HoveredPlant.Value,
        };
        
        _positionComponentMapper.Get(_ghostPlant.Value.EntityId).Position = placement;
        var sprite = (SpriteDrawable)_drawableComponentMapper.Get(_ghostPlant.Value.EntityId)
            .Drawable;
        if (CanPlacePlant(_ghostPlant.Value.PlantType, placement)) {
          sprite.Sprite.Color = Color.White;
        }
        else {
          sprite.Sprite.Color = Color.Red;
        }
      }

      if (!HoveredPlant.HasValue && _ghostPlant.HasValue) {
        DestroyEntity(_ghostPlant.Value.EntityId);
        _ghostPlant = null;
      }
    }

    // TODO Cache these values on update to prevent draw lag
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      if (!HoveredPlant.HasValue || !_gameScreen.Game.Config.ShowBuildHints) {
        return;
      }
      var growthCondition = _plantFactory.GetPlantGrowthCondition(HoveredPlant.Value);
      _map.ForEachTileInBounds(_cameraSystem.Camera.BoundingRectangle, (x, y, tile) => {
        if (growthCondition(tile)) {
          spriteBatch.Draw(
              _validSquareSprite.TextureRegion.Texture,
              MapPlacementHelper.MapCoordToAbsoluteCoord(new Vector2(x, y)),
              _validSquareSprite.TextureRegion.Bounds,
              Color.White,
              rotation: 0f,
              _validSquareSprite.Origin,
              scale: Vector2.One,
              SpriteEffects.None,
              0f);
        }
      });
    }
    
    public bool AttemptPlantPlacement(PlantType type, Vector2 location) {
      if (CanPlacePlant(type, location)) {
        _plantFactory.CreatePlant(type, location);
        return true;
      }
      return false;
    }

    public void AttemptDigTrench(int x, int y) {
      if (!CanDigTrench(x, y)) {
        return;
      }
      
      if (_map.TryGet(x, y, out var t)) {
        if (!t.ContainsWater && t.CanContainWater) {
          var hasAdjacentWater = false;
          _map.ForEachAdjacentTile(x, y, (_, adjX, adjY, adjTile) => {
            if (adjTile.ContainsWater) {
              hasAdjacentWater = true;
            }
          });

          if (hasAdjacentWater) {
            t.ContainsWater = true;
            _map.MarkTileDirty(x, y);
          }
        }
      }
    }

    private bool CanPlacePlant(PlantType type, Vector2 location) {
      var footprintSize = _plantFactory.GetPlantFootprintSize(type);
      var candidateFootprint = new RectangleF(
          location.X, location.Y, footprintSize.X, footprintSize.Y);
      if (_isSpaceOccupied.IsSpaceOccupied(candidateFootprint)) {
        return false;
      }

      var growthCondition = _plantFactory.GetPlantGrowthCondition(type);
      foreach (var tile in _map.GetIntersectingTiles(candidateFootprint)) {
        if (!growthCondition(tile.Tile)) {
          return false;
        }
      };
      return true;
    }

    private bool CanDigTrench(int x, int y) {
      return !_isSpaceOccupied.IsSpaceOccupied(new RectangleF(x, y, 1, 1));
    }

    private struct GhostPlant {
      public int EntityId;
      public PlantType PlantType;
    }
  }
}