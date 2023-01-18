using System;
using System.Collections.Generic;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using TinyGardenGame.Config;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.State.FiniteStateMachine;
using TinyGardenGame.Player.State.Inventory;
using TinyGardenGame.Player.State.Tools;

namespace TinyGardenGame.Player.State {
  /**
   * Encapsulate all state of Player:
   * - ECS Entity
   * - Inventory
   * - Tools
   * - More?
   */
  public class PlayerState {
    private KeyedCollection<Type, BasePlayerState> _states;

    public Entity PlayerEntity { get; private set; }
    public PlayerInventory Inventory { get; }
    public PlayerTools Tools { get; }
    public ResourceMeter Hp { get; }
    public ResourceMeter Energy { get; }

    public BasePlayerState State { get; private set; }

    public PlayerState() {
      Inventory = new PlayerInventory();
      Tools = new PlayerTools();
      Hp = new ResourceMeter(GameConfig.Config.DefaultMaxHp, GameConfig.Config.DefaultMinHp);
      Energy = new ResourceMeter(
          GameConfig.Config.DefaultMaxEnergy, GameConfig.Config.DefaultMinEnergy);
      _states = new KeyedCollection<Type, BasePlayerState>(state => state.GetType());
    }

    public void Initialize(
        World world,
        MainGame game,
        IIsSpaceOccupied isSpaceOccupied,
        GameMap map,
        MapProcessor mapProcessor) {
      _states.Add(new MovablePlayerState(this, isSpaceOccupied, map));
      _states.Add(new BasicAttackingPlayerState(this, isSpaceOccupied, map));
      _states.Add(new PlaceableObjectHoveringState(
          this, world, game, isSpaceOccupied, map, mapProcessor));

      State = _states[typeof(MovablePlayerState)];
      var playerSprite = game.Content.LoadAnimated(Vars.Sprite.Type.Player);
      PlayerEntity = world.CreateEntity()
          .AttachAnd(new DrawableComponent(new AnimatedSpriteDrawable(playerSprite)))
          .AttachAnd(new CameraFollowComponent())
          .AttachAnd(new MotionComponent(GameConfig.Config.PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PositionComponent(CenterOfMapTile(0, 0)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(-0.3f, -0.3f, 0.6f, 0.6f)))
          .AttachAnd(new DamageRecipientComponent(
              Hp.CurrentValue,
              new SysRectangleF(-0.3f, -0.3f, 0.6f, 0.6f),
              DamageRecipientComponent.Category.Friendly))
          .AttachAnd(new SelectionComponent());
      PlayerEntity.SetTag(Tags.Player);
    }

    public void Update(GameTime gameTime, HashSet<PlayerAction> triggeredActions) {
      SyncResources();
      var newState = State?.Update(gameTime, triggeredActions);
      if (newState != null && _states[newState].MeetsEntryCondition()) {
        State.Exit();
        State = _states[newState];
        State.Enter();
      }
    }

    // TODO: The need for this sync is error-prone. The meters should use the source components
    //     directly
    private void SyncResources() {
      var hpComponent = PlayerEntity.Get<DamageRecipientComponent>();
      Hp.MaxValue = hpComponent.MaxHp;
      Hp.CurrentValue = hpComponent.CurrentHp;
    }
  }
}