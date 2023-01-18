using MonoGame.Extended.Entities;

namespace TinyGardenGame.Units.Systems; 

public interface IEnemyAiHandler {
  void Handle(GameTime gameTime, int entity);
}
