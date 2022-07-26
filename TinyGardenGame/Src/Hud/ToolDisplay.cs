using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Player.State.Tools;

namespace TinyGardenGame.Hud {
  public class ToolDisplay {
    private const int MarginPx = 2;

    private readonly ContentManager _content;
    private readonly ScalingViewportAdapter _hudScale;
    private readonly PlayerTools _tools;

    public ToolDisplay(ContentManager content, ScalingViewportAdapter hudScale, PlayerTools tools) {
      _content = content;
      _hudScale = hudScale;
      _tools = tools;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      var sprite = _tools.CurrentlySelectedTool.GetSprite(_content);
      var position =
          new Vector2(MarginPx, _hudScale.VirtualHeight - sprite.TextureRegion.Height - MarginPx);
      sprite.Draw(
          spriteBatch,
          position,
          rotation: 0f,
          scale: Vector2.One);
    }
  }
}