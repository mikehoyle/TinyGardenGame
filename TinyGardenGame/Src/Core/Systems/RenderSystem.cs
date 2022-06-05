using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.MapGeneration;
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
  public class RenderSystem : EntityDrawSystem {
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CameraSystem _cameraSystem;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private readonly MapRenderer _mapRenderer;

    public RenderSystem(
        MainGame game, GraphicsDevice graphicsDevice, CameraSystem cameraSystem, GameMap map)
        : base(Aspect.All(typeof(DrawableComponent), typeof(PositionComponent))) {
      _graphicsDevice = graphicsDevice;
      _cameraSystem = cameraSystem;
      _spriteBatch = new SpriteBatch(graphicsDevice);
      _mapRenderer = new MapRenderer(game, _spriteBatch, map);
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
    }

    public override void Draw(GameTime gameTime) {
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp,
          transformMatrix: _cameraSystem.Camera.GetViewMatrix());
      
      DrawMap(gameTime);
      DrawSprites(gameTime);

      _spriteBatch.End();
    }

    private void DrawMap(GameTime gameTime) {
      _graphicsDevice.Clear(Color.CornflowerBlue);
      _mapRenderer.Draw(_cameraSystem.Camera.BoundingRectangle);
    }

    private void DrawSprites(GameTime gameTime) {
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
        var depth1 = _positionComponentMapper.Get(entity1).EffectiveRenderDepth;
        var depth2 = _positionComponentMapper.Get(entity2).Position;
        if (depth1 == depth2) {
          return _positionComponentMapper.Get(entity2).FootprintSizeInTiles != Vector2.Zero
              ? -1 : 0;
        }
        return ((depth1.X >= depth2.X) && (depth1.Y >= depth2.Y)) ? 1 : -1;
      });
      
      // And now draw in order
      foreach (var entity in entities) {
        var drawable = _drawableComponentMapper.Get(entity);
        var absolutePosition = _positionComponentMapper.Get(entity).AbsolutePosition;
        drawable.Drawable.Draw(_spriteBatch, absolutePosition);
      }
    }
  }
}