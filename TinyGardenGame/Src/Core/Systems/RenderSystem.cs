using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Player.Systems;

namespace TinyGardenGame.Core.Systems {
  public enum RenderLayer {
    Map = 1,
    GameObject = 2,
    Overlay = 3,
    Menu = 4,
  }
  
  /**
   * Render in ordered stages:
   * 1. Map
   * 2. Sprites (in order, back to front)
   * 3. Overlay components
   * 4. Game GUI
   */
  public class RenderSystem : EntityDrawSystem, IUpdateSystem {
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CameraSystem _cameraSystem;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private ComponentMapper<PlacementComponent> _placementComponentMapper;
    private readonly TiledMapRenderer _tiledMapRenderer;

    public RenderSystem(GraphicsDevice graphicsDevice, CameraSystem cameraSystem, TiledMap map)
        : base(Aspect.All(typeof(DrawableComponent), typeof(PlacementComponent))) {
      _graphicsDevice = graphicsDevice;
      _cameraSystem = cameraSystem;
      // TODO probably remove or rework this clumsy dependency on Tiled.
      _tiledMapRenderer = new TiledMapRenderer(graphicsDevice, map);
      _spriteBatch = new SpriteBatch(graphicsDevice);
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      _placementComponentMapper = mapperService.GetMapper<PlacementComponent>();
    }

    public void Update(GameTime gameTime) {
      _tiledMapRenderer.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
      DrawMap(gameTime);
      DrawSprites(gameTime);
    }

    private void DrawMap(GameTime gameTime) {
      _graphicsDevice.Clear(Color.CornflowerBlue);
      _tiledMapRenderer.Draw(_cameraSystem.ViewMatrix);
    }

    private void DrawSprites(GameTime gameTime) {
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp,
          transformMatrix: _cameraSystem.Camera.GetViewMatrix());

      // Sort the entities by depth
      var entities = ActiveEntities.ToList();
      entities.Sort((entity1, entity2) => {
        // TODO: These lookups should be fast (O(1)), but may still be an efficiency issue
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
        var depth1 = _placementComponentMapper.Get(entity1).EffectiveRenderDepth;
        var depth2 = _placementComponentMapper.Get(entity2).Position;
        if (depth1 == depth2) {
          return _placementComponentMapper.Get(entity2).FootprintSizeInTiles != Vector2.Zero
              ? -1 : 0;
        }
        return ((depth1.X >= depth2.X) && (depth1.Y >= depth2.Y)) ? 1 : -1;
      });
      
      // And now draw in order
      foreach (var entity in entities) {
        var drawable = _drawableComponentMapper.Get(entity);
        var absolutePosition = _placementComponentMapper.Get(entity).AbsolutePosition;
        drawable.Drawable.Draw(_spriteBatch, absolutePosition);
      }

      _spriteBatch.End();
    }
  }
}