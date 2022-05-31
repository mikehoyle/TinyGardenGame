using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Player.Components;
using static TinyGardenGame.MapPlacementHelper;

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

    private readonly MainGame _game;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private ComponentMapper<PlacementComponent> _placementComponentMapper;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;

    public PlayerInputSystem(MainGame game)
        : base(Aspect.All(
            typeof(PlayerInputComponent),
            typeof(MotionComponent),
            typeof(PlacementComponent),
            typeof(SelectionComponent),
            typeof(CollisionFootprintComponent))) {
      _game = game;
      _game.Console.MovePlayer += TeleportPlayer;
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
      _placementComponentMapper = mapperService.GetMapper<PlacementComponent>();
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
    }

    public override void Update(GameTime gameTime) {
      var movementDirection = GetMovementDirection();
      foreach (var entity in ActiveEntities) {
        var motionComponent = _motionComponentMapper.Get(entity);
        motionComponent.SetMotionFromCardinalVector(
            GetMovementVector(gameTime, movementDirection, motionComponent.SpeedTilesPerSec));
      }
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
        _placementComponentMapper.Get(ActiveEntities[0])
            .Position = new Vector2(x, y);
      } else {
        _game.Console.WriteLine(
            $"Need exactly one entity to move but found {ActiveEntities.Count}");
      }
    }

    void IDisposable.Dispose() {
      if (_game != null) {
        _game.Console.MovePlayer -= TeleportPlayer;
      }
      base.Dispose();
    }
  }
}