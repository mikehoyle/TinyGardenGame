using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input.InputListeners;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.State;
using TinyGardenGame.Screens;
using static TinyGardenGame.MapPlacementHelper.Direction;

namespace TinyGardenGame.Player.Systems {
  public class PlayerInputSystem : EntityUpdateSystem {
    private readonly Dictionary<Keys, Action> _actionControls;


    private readonly MainGame _game;
    private readonly HeadsUpDisplay _hud;
    private readonly PrimaryGameplayScreen _screen;
    private readonly PlayerState _playerState;
    private readonly ObjectPlacementSystem _objectPlacementSystem;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private readonly ControlsMapper<PlayerAction> _controlsMapper;

    public PlayerInputSystem(
        PrimaryGameplayScreen screen,
        PlayerState playerState,
        GameMap map,
        ObjectPlacementSystem objectPlacementSystem)
        : base(Aspect.All(
            typeof(PlayerInputComponent),
            typeof(MotionComponent),
            typeof(PositionComponent),
            typeof(SelectionComponent),
            typeof(CollisionFootprintComponent))) {
      _screen = screen;
      _playerState = playerState;
      _objectPlacementSystem = objectPlacementSystem;
      // TODO: import these from a separate config, don't hardcode
      _controlsMapper = new ControlsMapper<PlayerAction>()
          .Register(new KeyHeldCondition(Keys.Up), PlayerAction.MoveUp)
          .Register(new KeyHeldCondition(Keys.Down), PlayerAction.MoveDown)
          .Register(new KeyHeldCondition(Keys.Left), PlayerAction.MoveLeft)
          .Register(new KeyHeldCondition(Keys.Right), PlayerAction.MoveRight)
          .Register(new KeyPressedCondition(Keys.Space), PlayerAction.Attack)
          .Register(new KeyPressedCondition(Keys.Q), PlayerAction.PlacePlant)
          .Register(new KeyPressedCondition(Keys.A), PlayerAction.ToggleHoverPlant)
          .Register(
              new KeyPressedCondition(Keys.OemOpenBrackets), PlayerAction.InventorySelectionLeft)
          .Register(
              new KeyPressedCondition(Keys.OemCloseBrackets), PlayerAction.InventorySelectionRight);
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
    }

    public override void Update(GameTime gameTime) {
      var triggeredActions = _controlsMapper.GetTriggeredActions();
      _playerState.Update(gameTime, triggeredActions);
    }

    private void OnKeyPressed(object sender, KeyboardEventArgs args) {
      if (_actionControls.TryGetValue(args.Key, out var action)) {
        action.Invoke();
      }
    }

    private void MoveSelectionLeft() {
      _playerState.Inventory.CurrentlySelectedSlot--;
      _objectPlacementSystem.HoveredPlant = null;
    }
    
    private void MoveSelectionRight() {
      _playerState.Inventory.CurrentlySelectedSlot++;
      _objectPlacementSystem.HoveredPlant = null;
    }

    private void PlacePlant() {
      var currentItem = _playerState.Inventory.CurrentlySelectedItem; 
      if (currentItem == null) {
        return;
      }
      
      var placement = _selectionComponentMapper.Get(ActiveEntities[0]).SelectedSquare;
      if (_objectPlacementSystem.AttemptPlantPlacement(currentItem.PlantType(), placement)) {
        currentItem.Expend(1);
        _objectPlacementSystem.HoveredPlant = null;
      }
    }

    private void ToggleHoverPlant() {
      var currentItem = _playerState.Inventory.CurrentlySelectedItem;
      if (_objectPlacementSystem.HoveredPlant == null && currentItem != null) {
        _objectPlacementSystem.HoveredPlant = currentItem.PlantType();
      } else {
        _objectPlacementSystem.HoveredPlant = null;
      }
    }
    
    private void DigTrench() {
      var placement = _selectionComponentMapper.Get(ActiveEntities[0]).SelectedSquare;
      _objectPlacementSystem.AttemptDigTrench((int)placement.X, (int)placement.Y);
    }

    private void Attack() {
      // TODO: Add cooldaown / animation state to prevent spam, cancellation, and looping
      // TODO: Also fix animation to work at all and not get pre-empted by idle animation
      // TODO: Add damage component
      var position = _positionComponentMapper.Get(_playerState.PlayerEntity);
      var drawable = _drawableComponentMapper.Get(_playerState.PlayerEntity);
      switch (MapPlacementHelper.AngleToDirection(position.Rotation)) {
        case SouthEast:
          drawable.SetAnimation($"attack_down");
          break;
        case NorthWest:
          drawable.SetAnimation($"attack_up");
          break;
        case South:
        case SouthWest:
        case West:
          drawable.SetAnimation($"attack_left");
          break;
        case North:
        case NorthEast:
        case East:
          drawable.SpriteEffects = SpriteEffects.FlipHorizontally;
          drawable.SetAnimation($"attack_left");
          break;
      }
    }
  }
}