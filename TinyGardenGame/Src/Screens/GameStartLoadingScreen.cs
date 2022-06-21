#nullable enable
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using MonoGame.Extended.Screens;
using TinyGardenGame.Core;

namespace TinyGardenGame.Screens {
  /** Currently, just load all textures with no UI */
  public class GameStartLoadingScreen: GameScreen {
    private readonly MainGame _game;
    private Task? _loadingTask;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private Stopwatch _loadTimer;

    public GameStartLoadingScreen(MainGame game) : base(game) {
      _game = game;
      _loadTimer = new Stopwatch();
    }

    public override void Initialize() {
      _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
    }

    public override void LoadContent() {
      _font = _game.Content.Load<SpriteFont>(Assets.ConsoleFont);
    }
    
    public override void Update(GameTime gameTime) {
      // OPTIMIZE: Load on background thread while updating UI to not block main thread
      if (_loadingTask == null) {
        _loadingTask = AssetLoading.LoadAllAssets(Content);
        _loadTimer.Start();
        return;
      }

      if (_loadingTask.IsCompleted) {
        _loadTimer.Stop();
        _game.LoadScreen((game) => new PrimaryGameplayScreen(game));
      }
    }

    public override void Draw(GameTime gameTime) {
      _game.GraphicsDevice.Clear(Color.Cornsilk);
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp);
      _spriteBatch.DrawString(
          _font, $"Loading... {_loadTimer.Elapsed}", new Vector2(5, 5), Color.Red);
      _spriteBatch.End();
    }
  }
}