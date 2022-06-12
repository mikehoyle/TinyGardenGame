using System.Drawing;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.Systems;
using TinyGardenGame.Screens;
using static TinyGardenGame.MapPlacementHelper;
using Color = Microsoft.Xna.Framework.Color;

namespace TinyGardenGame.Plants {
  /**
   * Manages the place-ability of objects (such as plants) on the map.
   *
   * Also the display of placement overlay 
   */
  public class ObjectPlacementSystem : EntityUpdateSystem, IDrawSystem, IAttemptPlantPlacement {
    private readonly PrimaryGameplayScreen _gameScreen;
    private readonly GameMap _map;
    private readonly IIsSpaceOccupied _isSpaceOccupied;
    private readonly CameraSystem _cameraSystem;
    private readonly PlantEntityFactory _plantFactory;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private GhostPlant? _ghostPlant;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;

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
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
    }

    public override void Update(GameTime gameTime) {
      UpdateBuildGhost();

      if (_gameScreen.Game.Config.ShowBuildHints && HoveredPlant != null) {
        ForEachTileInBounds(_map, _cameraSystem.Camera.BoundingRectangle, (x, y, tile) => {
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

    public void Draw(GameTime gameTime) {
      // TODO draw overlay
    }
    
    public bool AttemptPlantPlacement(PlantType type, Vector2 location) {
      if (CanPlacePlant(type, location)) {
        _plantFactory.CreatePlant(type, location);
        return true;
      }
      return false;
    }

    private bool CanPlacePlant(PlantType type, Vector2 location) {
      var footprintSize = _plantFactory.GetPlantFootprintSize(type);
      var candidateFootprint = new RectangleF(
          location.X, location.Y, footprintSize.X, footprintSize.Y);
      if (_isSpaceOccupied.IsSpaceOccupied(candidateFootprint)) {
        return false;
      }

      foreach (var tile in GetIntersectingTiles(_map, candidateFootprint)) {
        if (tile.Tile.Has(TileFlags.ContainsWater)
            || tile.Tile.Has(TileFlags.IsNonTraversable)) {
          return false;
        }
      };
      return true;
    }

    private struct GhostPlant {
      public int EntityId;
      public PlantType PlantType;
    }
  }
}