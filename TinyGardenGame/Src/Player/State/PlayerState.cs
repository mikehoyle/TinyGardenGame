﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
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

    public PlayerState(Config.Config config) {
      Inventory = new PlayerInventory();
      Tools = new PlayerTools();
      Hp = new ResourceMeter(config.DefaultMaxHp, config.DefaultMinHp);
      Energy = new ResourceMeter(config.DefaultMaxEnergy, config.DefaultMinEnergy);
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
      var playerSprite = game.Content.LoadAnimated(SpriteName.Player);
      PlayerEntity = world.CreateEntity()
          .AttachAnd(new DrawableComponent(new AnimatedSpriteDrawable(playerSprite)))
          .AttachAnd(new CameraFollowComponent())
          .AttachAnd(new MotionComponent(game.Config.PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PositionComponent(MapPlacementHelper.CenterOfMapTile(0, 0)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(-0.3f, -0.3f, 0.6f, 0.6f)))
          .AttachAnd(new DamageRecipientComponent(
              Hp.CurrentValue, new System.Drawing.RectangleF(-0.3f, -0.3f, 0.6f, 0.6f)))
          .AttachAnd(new SelectionComponent());
    }

    public void Update(GameTime gameTime, HashSet<PlayerAction> triggeredActions) {
      var newState = State?.Update(gameTime, triggeredActions);
      if (newState != null && _states[newState].MeetsEntryCondition()) {
        State.Exit();
        State = _states[newState];
        State.Enter();
      }
    }
  }
}