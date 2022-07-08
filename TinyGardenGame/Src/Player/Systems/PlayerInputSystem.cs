using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input.InputListeners;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.Plants;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Screens;

namespace TinyGardenGame.Player.Systems {
  public class PlayerInputSystem : EntityUpdateSystem, IDisposable {
    // TODO: Base player input on configuration
    // TODO: Controller support
    private static readonly Dictionary<Keys, Vector2> MovementControls =
        new Dictionary<Keys, Vector2>() {
          {Keys.Down, Vector2.UnitY},
          {Keys.Up, Vector2.UnitY * -1},
          {Keys.Right, Vector2.UnitX},
          {Keys.Left, Vector2.UnitX * -1},
        };

    private readonly Dictionary<Keys, Action> _actionControls;


    private readonly MainGame _game;
    private readonly HeadsUpDisplay _hud;
    private readonly PrimaryGameplayScreen _screen;
    private readonly ObjectPlacementSystem _objectPlacementSystem;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;
    private readonly KeyboardListener _keyboardListener;

    public PlayerInputSystem(
        PrimaryGameplayScreen screen,
        HeadsUpDisplay hud,
        GameMap map,
        ObjectPlacementSystem objectPlacementSystem)
        : base(Aspect.All(
            typeof(PlayerInputComponent),
            typeof(MotionComponent),
            typeof(PositionComponent),
            typeof(SelectionComponent),
            typeof(CollisionFootprintComponent))) {
      _screen = screen;
      _hud = hud;
      _objectPlacementSystem = objectPlacementSystem;
      _keyboardListener = new KeyboardListener(new KeyboardListenerSettings() {
          RepeatPress = false,
      });
      _keyboardListener.KeyPressed += OnKeyPressed;
      screen.Console.MovePlayer += TeleportPlayer;
      _actionControls = new Dictionary<Keys, Action>() {
          {Keys.OemMinus, MoveSelectionLeft},
          {Keys.OemPlus, MoveSelectionRight},
          {Keys.Q, PlacePlant},
          {Keys.W, DigTrench},
          {Keys.A, ToggleHoverPlant},
      };
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
    }

    public override void Update(GameTime gameTime) {
      // Motion controls use input directly, while more concise button-press actions use
      // input listeners.
      var movementDirection = GetMovementDirection();
      foreach (var entity in ActiveEntities) {
        var motionComponent = _motionComponentMapper.Get(entity);
        motionComponent.SetMotionFromCardinalVector(
            GetMovementVector(gameTime, movementDirection, motionComponent.SpeedTilesPerSec));
      }

      _keyboardListener.Update(gameTime);
    }

    private static Vector2 GetMovementVector(
        GameTime gameTime, Vector2 movementDirection, float speed) {
      var normalizedSpeed = speed * gameTime.GetElapsedSeconds();
      return movementDirection * normalizedSpeed;
    }
    
    private static Vector2 GetMovementDirection() {
      var state = Keyboard.GetState();
      var movementDirection = MovementControls.Keys.Where(key => state.IsKeyDown(key))
          .Aggregate(Vector2.Zero, (current, key) => current + MovementControls[key]);
      
      if (movementDirection != Vector2.Zero) {
        movementDirection.Normalize(); 
      }
    
      return movementDirection;
    }

    private void TeleportPlayer(int x, int y) {
      if (ActiveEntities.Count == 1) {
        _positionComponentMapper.Get(ActiveEntities[0])
            .Position = new Vector2(x, y);
      } else {
        _screen.Console.WriteLine(
            $"Need exactly one entity to move but found {ActiveEntities.Count}");
      }
    }
    
    private void OnKeyPressed(object sender, KeyboardEventArgs args) {
      if (_actionControls.TryGetValue(args.Key, out var action)) {
        action.Invoke();
      }
    }

    private void MoveSelectionLeft() {
      _hud.Inventory.CurrentlySelectedSlot--;
      _objectPlacementSystem.HoveredPlant = null;
    }
    
    private void MoveSelectionRight() {
      _hud.Inventory.CurrentlySelectedSlot++;
      _objectPlacementSystem.HoveredPlant = null;
    }

    private void PlacePlant() {
      var currentItem = _hud.Inventory.CurrentlySelectedItem; 
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
      var currentItem = _hud.Inventory.CurrentlySelectedItem;
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

    void IDisposable.Dispose() {
      if (_game != null) {
        _screen.Console.MovePlayer -= TeleportPlayer;
      }
      base.Dispose();
    }
  }
}