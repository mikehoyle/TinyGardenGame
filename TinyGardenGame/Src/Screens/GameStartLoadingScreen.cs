using Microsoft.Xna.Framework;
using MonoGame.Extended.Content;
using MonoGame.Extended.Screens;
using TinyGardenGame.Core;

namespace TinyGardenGame.Screens {
  /** Currently, just load all textures with no UI */
  public class GameStartLoadingScreen: GameScreen {
    private readonly MainGame _game;

    public GameStartLoadingScreen(MainGame game) : base(game) {
      _game = game;
    }

    public override void LoadContent() {
      AssetLoading.LoadAllAssets(Content);
      _game.LoadScreen((game) => new PrimaryGameplayScreen(game));
    }
    
    public override void Update(GameTime gameTime) {
      // Do nothing (for now)
    }

    public override void Draw(GameTime gameTime) {
      // Draw Nothing (for now)
    }
  }
}