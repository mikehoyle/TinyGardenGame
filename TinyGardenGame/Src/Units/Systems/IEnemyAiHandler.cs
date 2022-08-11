using MonoGame.Extended.Entities;

namespace TinyGardenGame.Units.Systems; 

public interface IEnemyAiHandler {
  void Initialize(IComponentMapperService mapperService);
  void Handle(GameTime gameTime, int entity);
}
