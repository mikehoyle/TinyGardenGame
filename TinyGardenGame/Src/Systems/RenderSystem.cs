using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Components;

namespace TinyGardenGame.Systems {
  public class RenderSystem : EntityDrawSystem {
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CameraSystem _cameraSystem;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<Sprite> _spriteMapper;
    private ComponentMapper<PlacementComponent> _positionComponentMapper;

    public RenderSystem(GraphicsDevice graphicsDevice, CameraSystem cameraSystem)
        : base(Aspect.All(typeof(Sprite), typeof(PlacementComponent))) {
      _graphicsDevice = graphicsDevice;
      _cameraSystem = cameraSystem;
      _spriteBatch = new SpriteBatch(graphicsDevice);
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _spriteMapper = mapperService.GetMapper<Sprite>();
      _positionComponentMapper = mapperService.GetMapper<PlacementComponent>();
    }

    public override void Draw(GameTime gameTime) {
      _spriteBatch.Begin(
          samplerState: SamplerState.PointClamp,
          transformMatrix: _cameraSystem.Camera.GetViewMatrix());

      foreach (var entity in ActiveEntities) {
        var sprite = _spriteMapper.Get(entity);
        var position = _positionComponentMapper.Get(entity);
        _spriteBatch.Draw(sprite, position.AbsolutePosition);
      }

      _spriteBatch.End();
    }
  }
}