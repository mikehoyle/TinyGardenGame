using Microsoft.Xna.Framework;

namespace TinyGardenGame.GameState {
  /**
   * Holds global game state associated with a single play-through
   */
  public class GameState {
    public InGameClock Clock { get; }

    public GameState(Config.Config config) {
      Clock = new InGameClock(config);
    }

    public void Update(GameTime gameTime) {
      Clock.Update(gameTime);
    }
  }
}