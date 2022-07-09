using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Player.State;

namespace TinyGardenGame.Hud {
  public class HeadsUpDisplay : IUpdate {
    private readonly ScalingViewportAdapter _hudScale;
    private readonly DebugOverlay _debugOverlay;

    public Inventory Inventory { get; }

    public HeadsUpDisplay(
        MainGame game,
        PlayerState playerState,
        GraphicsDevice graphicsDevice,
        int renderWidth,
        int renderHeight) {
      _hudScale = new ScalingViewportAdapter(graphicsDevice, renderWidth, renderHeight);
      Inventory = new Inventory(game.Content, _hudScale, playerState.Inventory);
      _debugOverlay = new DebugOverlay(game, _hudScale);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      spriteBatch.Begin(
          transformMatrix: _hudScale.GetScaleMatrix(),
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp);
      
      Inventory.Draw(spriteBatch, gameTime);
      _debugOverlay.Draw(spriteBatch, gameTime);
      
      spriteBatch.End();
    }

    public void Update(GameTime gameTime) {
      
    }
  }
}