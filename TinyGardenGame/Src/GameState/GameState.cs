using TinyGardenGame.Config;

namespace TinyGardenGame.GameState {
  /**
   * Holds global game state associated with a single play-through
   */
  public class GameState {
    public InGameClock Clock { get; }

    public GameState() {
      Clock = new InGameClock();
    }

    public void Update(GameTime gameTime) {
      Clock.Update(gameTime);
    }
  }
}