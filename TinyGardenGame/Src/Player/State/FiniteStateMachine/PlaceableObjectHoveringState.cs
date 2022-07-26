#nullable enable
using System;
using System.Collections.Generic;
using MonoGame.Extended.Entities;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants;
using TinyGardenGame.Player.Components;

namespace TinyGardenGame.Player.State.FiniteStateMachine {
  public class PlaceableObjectHoveringState : MovablePlayerState {
    private readonly IIsSpaceOccupied _isSpaceOccupied;
    private readonly GameMap _map;
    private PlantType _hoveredPlaceableType;
    private readonly PlantEntityFactory _plantFactory;
    private readonly Config.Config _config;
    private readonly Entity _placementGhostEntity;
    private readonly MapProcessor _mapProcessor;

    public PlaceableObjectHoveringState(
        PlayerState playerState,
        World world,
        MainGame game,
        IIsSpaceOccupied isSpaceOccupied,
        GameMap map,
        MapProcessor mapProcessor) : base(playerState, isSpaceOccupied, map) {
      _isSpaceOccupied = isSpaceOccupied;
      _map = map;
      _plantFactory = new PlantEntityFactory(game.Config, game.Content, world.CreateEntity);
      _config = game.Config;
      _placementGhostEntity = world.CreateEntity();
      _plantFactory = new PlantEntityFactory(game.Config, game.Content, world.CreateEntity);
      _mapProcessor = mapProcessor;
    }

    public override bool MeetsEntryCondition() {
      return PlayerState.Inventory.CurrentlySelectedItem != null;
    }

    public override void Enter() {
      var currentItem = PlayerState.Inventory.CurrentlySelectedItem;
      var selectionPosition = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;

      _hoveredPlaceableType = currentItem.PlantType();
      _plantFactory.CreateGhostPlant(
          _hoveredPlaceableType, selectionPosition, _placementGhostEntity);
      _mapProcessor.TileHighlightCondition =
          _plantFactory.GetPlantGrowthCondition(_hoveredPlaceableType);
    }

    public override Type? Update(GameTime gameTime, HashSet<PlayerAction> actions) {
      UpdateBuildGhost();
      if (actions.Contains(PlayerAction.PlacePlant)) {
        // TODO: place-plant state transition to accomodate animation
        var placement = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;
        if (AttemptPlantPlacement(_hoveredPlaceableType, placement)) {
          PlayerState.Inventory.CurrentlySelectedItem.Expend(1);

          MaybeMove(gameTime, actions);
          return typeof(MovablePlayerState);
        }
      }

      if (actions.Contains(PlayerAction.ToggleHoverPlant)) {
        MaybeMove(gameTime, actions);
        return typeof(MovablePlayerState);
      }

      return base.Update(gameTime, actions);
    }

    public override void Exit() {
      // Keep the ghost entity to prevent churn, but remove its components
      _placementGhostEntity.Detach<DrawableComponent>();
      _placementGhostEntity.Detach<PositionComponent>();
      _mapProcessor.TileHighlightCondition = null;
    }

    private bool AttemptPlantPlacement(PlantType type, Vector2 location) {
      if (CanPlacePlant(type, location)) {
        _plantFactory.CreatePlant(type, location);
        return true;
      }

      return false;
    }

    private void UpdateBuildGhost() {
      if (_config.ShowBuildGhost) {
        var placement = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;

        _placementGhostEntity.Get<PositionComponent>().Position = placement;
        var sprite = (SpriteDrawable)_placementGhostEntity.Get<DrawableComponent>().Drawable;
        if (CanPlacePlant(_hoveredPlaceableType, placement)) {
          sprite.Sprite.Color = Color.White;
        }
        else {
          sprite.Sprite.Color = Color.Red;
        }
      }
    }

    private bool CanPlacePlant(PlantType type, Vector2 location) {
      var footprintSize = _plantFactory.GetPlantFootprintSize(type);
      var candidateFootprint = new SysRectangleF(
          location.X, location.Y, footprintSize.X, footprintSize.Y);
      if (_isSpaceOccupied.IsSpaceOccupied(candidateFootprint)) {
        return false;
      }

      var growthCondition = _plantFactory.GetPlantGrowthCondition(type);
      foreach (var tile in _map.GetIntersectingTiles(candidateFootprint)) {
        if (!growthCondition(tile.Tile)) {
          return false;
        }
      }

      return true;
    }
  }
}