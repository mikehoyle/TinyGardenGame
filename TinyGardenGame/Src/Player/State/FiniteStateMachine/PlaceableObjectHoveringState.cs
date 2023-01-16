#nullable enable
using System;
using System.Collections.Generic;
using MonoGame.Extended.Entities;
using TinyGardenGame.Config;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Vars;

namespace TinyGardenGame.Player.State.FiniteStateMachine {
  public class PlaceableObjectHoveringState : MovablePlayerState {
    private readonly IIsSpaceOccupied _isSpaceOccupied;
    private readonly GameMap _map;
    private Plant _hoveredPlaceableItem;
    private readonly Entity _placementGhostEntity;
    private readonly MapProcessor _mapProcessor;
    private readonly Func<Entity> _createEntity;

    public PlaceableObjectHoveringState(
        PlayerState playerState,
        World world,
        MainGame game,
        IIsSpaceOccupied isSpaceOccupied,
        GameMap map,
        MapProcessor mapProcessor) : base(playerState, isSpaceOccupied, map) {
      _isSpaceOccupied = isSpaceOccupied;
      _map = map;
      _createEntity = world.CreateEntity;
      _placementGhostEntity = world.CreateEntity();
      _mapProcessor = mapProcessor;
    }

    public override bool MeetsEntryCondition() {
      return PlayerState.Inventory.CurrentlySelectedItem != null;
    }

    public override void Enter() {
      var currentItem = PlayerState.Inventory.CurrentlySelectedItem;
      var selectionPosition = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;

      _hoveredPlaceableItem = currentItem.PlantType();
      _hoveredPlaceableItem.CreateGhost(selectionPosition, _placementGhostEntity);
      _mapProcessor.TileHighlightCondition =
          _hoveredPlaceableItem.GrowthCondition;
    }

    public override Type? Update(GameTime gameTime, HashSet<PlayerAction> actions) {
      UpdateBuildGhost();
      if (actions.Contains(PlayerAction.PlacePlant)) {
        // TODO: place-plant state transition to accomodate animation
        var placement = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;
        if (AttemptPlantPlacement(_hoveredPlaceableItem, placement)) {
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

    private bool AttemptPlantPlacement(Plant plant, Vector2 location) {
      if (CanPlacePlant(plant, location)) {
        plant.Instantiate(location, _createEntity);
        return true;
      }

      return false;
    }

    private void UpdateBuildGhost() {
      if (GameConfig.Config.ShowBuildGhost) {
        var placement = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;

        _placementGhostEntity.Get<PositionComponent>().Position = placement;
        var sprite = (SpriteDrawable)_placementGhostEntity.Get<DrawableComponent>().Drawable;
        if (CanPlacePlant(_hoveredPlaceableItem, placement)) {
          sprite.Sprite.Color = Color.White;
        }
        else {
          sprite.Sprite.Color = Color.Red;
        }
      }
    }

    private bool CanPlacePlant(Plant plant, Vector2 location) {
      var footprintSize = plant.FootprintVec;
      var candidateFootprint = new SysRectangleF(
          location.X, location.Y, footprintSize.X, footprintSize.Y);
      if (_isSpaceOccupied.IsSpaceOccupied(candidateFootprint)) {
        return false;
      }
      
      foreach (var tile in _map.GetIntersectingTiles(candidateFootprint)) {
        if (!plant.GrowthCondition(tile.Tile)) {
          return false;
        }
      }

      return true;
    }
  }
}