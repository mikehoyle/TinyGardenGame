﻿using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Player.Systems;

namespace TinyGardenGame.Core.Systems {
  public enum RenderLayer {
    GameObject = 1,
    Overlay = 2,
    Menu = 3,
  }
  
  /**
   * Render in ordered stages:
   * - Map
   * - Map overlay
   * - Sprites (in order, back to front)
   * - Overlay components
   * - HUD
   * - Menus
   */
  public class RenderSystem : EntityDrawSystem, IUpdateSystem {
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CameraSystem _cameraSystem;
    private readonly GameState.GameState _gameState;
    private readonly RenderSystemDraw _drawMapOverlay;
    private readonly RenderSystemDraw _drawHud;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private readonly MapProcessor _mapProcessor;
    private readonly Effect _nightEffect;

    public delegate void RenderSystemDraw(SpriteBatch spriteBatch, GameTime gameTime);

    public RenderSystem(
        MainGame game,
        GraphicsDevice graphicsDevice,
        CameraSystem cameraSystem,
        GameState.GameState gameState,
        GameMap map, 
        RenderSystemDraw drawMapOverlay,
        RenderSystemDraw drawHud)
        : base(Aspect.All(typeof(DrawableComponent), typeof(PositionComponent))) {
      _graphicsDevice = graphicsDevice;
      _cameraSystem = cameraSystem;
      _gameState = gameState;
      _drawMapOverlay = drawMapOverlay;
      _nightEffect = game.Content.Load<Effect>("shaders/night_shader");
      _drawHud = drawHud;
      _spriteBatch = new SpriteBatch(graphicsDevice);
      _mapProcessor = new MapProcessor(game, map);
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
    }

    public override void Draw(GameTime gameTime) {
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp,
          transformMatrix: _cameraSystem.ViewMatrix,
          effect: _gameState.Clock.IsNight ? _nightEffect : null);
      DrawMap();
      _drawMapOverlay(_spriteBatch, gameTime);
      DrawSprites(gameTime);
      _spriteBatch.End();
      // Hud uses its own batch
      _drawHud(_spriteBatch, gameTime);
    }

    private void DrawMap() {
      _graphicsDevice.Clear(Color.CornflowerBlue);
      _mapProcessor.Draw(_spriteBatch, _cameraSystem.Camera.BoundingRectangle);
    }

    private void DrawSprites(GameTime gameTime) {
      // Sort the entities by depth
      var entities = ActiveEntities.ToList();
      // TODO: Fix this inconsistent sort
      entities.Sort((entity1, entity2) => {
        // OPTIMIZE: These lookups should be fast (O(1)), but may still be an efficiency issue
        // This is also far more continued sorting than is really necessary, as many of
        // the game components will never move.
        // In short, look here if optimization is needed
        var layer1 = _drawableComponentMapper.Get(entity1).RenderLayer;
        var layer2 = _drawableComponentMapper.Get(entity2).RenderLayer;
        if (layer1 != layer2) {
          return layer1 - layer2;
        }
        
        // Sprite1 is in front of Sprite2 if its SE-most point is greater (X&Y) than
        // the NW origin of Sprite2.
        // This is only sound given some assumptions about the entities
        // (that they don't overlap, and take up about a tile). Those may break in the future
        // we shall see.
        var pos1 = _positionComponentMapper.Get(entity1).Position;
        var depth1 = _positionComponentMapper.Get(entity1).EffectiveRenderDepth;
        var pos2 = _positionComponentMapper.Get(entity2).Position;
        if (pos1 == pos2) {
          return 0;
        }
        if (depth1.X == pos2.X || depth1.Y == pos2.Y) {
          return _positionComponentMapper.Get(entity2).FootprintSizeInTiles != Vector2.Zero
              ? -1 : 0;
        }
        return ((depth1.X > pos2.X) && (depth1.Y > pos2.Y)) ? 1 : -1;
      });
      
      // And now draw in order
      foreach (var entity in entities) {
        var drawable = _drawableComponentMapper.Get(entity);
        var absolutePosition = _positionComponentMapper.Get(entity).AbsolutePosition;
        drawable.Update(gameTime);
        drawable.Draw(_spriteBatch, absolutePosition);
      }
    }

    public void Update(GameTime gameTime) {
      _mapProcessor.Update(gameTime);
    }
  }
}