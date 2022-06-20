﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Player.Components;

namespace TinyGardenGame.Core.Systems {
  /**
   * Encapsulates debug functionality as much as possible.
   */
  public class DebugSystem : EntityUpdateSystem {
    private readonly MainGame _game;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponent;
    private ComponentMapper<MotionComponent> _motionComponent;
    private ComponentMapper<PositionComponent> _positionComponent;
    private ComponentMapper<Sprite> _spriteComponent;
    private ComponentMapper<SelectionComponent> _selectionComponent;
    private Entity _selectionIndicatorEntity;
    
    public Entity PlayerEntity { private get; set; } 

    public DebugSystem(MainGame game) : base(Aspect.One()) {
      _game = game;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _collisionComponent = mapperService.GetMapper<CollisionFootprintComponent>();
      _motionComponent = mapperService.GetMapper<MotionComponent>();
      _positionComponent = mapperService.GetMapper<PositionComponent>();
      _spriteComponent = mapperService.GetMapper<Sprite>();
      _selectionComponent = mapperService.GetMapper<SelectionComponent>();
      
      _selectionIndicatorEntity = _game.Config.Debug.ShowSelectionIndicator ? CreateEntity() : null;
    }

    public void LoadContent() {
      LoadSelectionIndicator();
    }

    public override void Update(GameTime gameTime) {
      UpdateSelectionIndicator();
    }

    private void LoadSelectionIndicator() {
      if (_game.Config.Debug.ShowSelectionIndicator && PlayerEntity != null) {
        var playerSelection = _selectionComponent.Get(PlayerEntity);
        var sprite = _game.Content.LoadSprite(SpriteName.SelectedTileOverlay);
        _selectionIndicatorEntity
            .AttachAnd(new DrawableComponent(sprite, RenderLayer.Overlay))
            .Attach(new PositionComponent(playerSelection.SelectedSquare));
      }
    }

    private void UpdateSelectionIndicator() {
      if (PlayerEntity != null && _selectionIndicatorEntity != null) {
        var playerSelection = _selectionComponent.Get(PlayerEntity);
        var indicatorPlacement = _positionComponent.Get(_selectionIndicatorEntity);
        indicatorPlacement.Position = playerSelection.SelectedSquare;
      }
    }
  }
}