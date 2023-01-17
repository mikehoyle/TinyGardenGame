using System.Text;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Core;
using TinyGardenGame.GameState;

namespace TinyGardenGame.Hud {
  // TODO Display in a more appealing/graphical way. This whole class is pretty hacked up knowing
  //     it will change very soon (famous last words...)
  public class TimeDisplay {
    private readonly ScalingViewportAdapter _hudScale;
    private readonly InGameClock _clock;
    private readonly BitmapFont _font;

    public TimeDisplay(ScalingViewportAdapter hudScale, InGameClock gameClock) {
      _hudScale = hudScale;
      _clock = gameClock;
      _font = Platform.Content.LoadBmpFont(Vars.BmpFont.Type.CourierFont);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      var displayString = BuildTimeString();
      var position = new Vector2(
          _hudScale.VirtualWidth - HeadsUpDisplay.PaddingPx - 35,
          HeadsUpDisplay.PaddingPx);
      spriteBatch.DrawString(
          _font,
          BuildTimeString(),
          position,
          Color.White,
          rotation: 0,
          origin: Vector2.Zero,
          scale: 0.5f,
          SpriteEffects.None,
          layerDepth: 0);
    }

    private string BuildTimeString() {
      var result = new StringBuilder();
      result.Append(_clock.IsNight ? "N" : "D");
      result.Append($" [{_clock.Day}] ");
      result.Append($"{(int)_clock.TimeOfDay}:{(int)((_clock.TimeOfDay % 1) * 60):00}");
      return result.ToString();
    }
  }
}