using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Core;

namespace TinyGardenGame.Hud {
  public class DebugOverlay {
    private const int PaddingTopPx = 2;
    private const int PaddingLeftPx = 2;

    private readonly MainGame _game;
    private readonly ScalingViewportAdapter _hudScale;
    private readonly SpriteFont _font;

    public DebugOverlay(MainGame mainGame, ScalingViewportAdapter hudScale) {
      _game = mainGame;
      _hudScale = hudScale;
      _font = mainGame.Content.LoadFont(SpriteName.ConsoleFont);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      if (_game.Config.Debug.EnableFpsDisplay) {
        DisplayFps(spriteBatch, gameTime);
      }
    }

    private void DisplayFps(SpriteBatch spriteBatch, GameTime gameTime) {
      var fps = (int)(1d / gameTime.ElapsedGameTime.TotalSeconds);
      spriteBatch.DrawString(
          _font,
          fps.ToString(),
          new Vector2(PaddingLeftPx, PaddingTopPx),
          Color.Red,
          rotation: 0,
          origin: Vector2.Zero,
          scale: 0.5f,
          SpriteEffects.None,
          layerDepth: 0);
    }
  }
}