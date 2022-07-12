using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Player.State;

namespace TinyGardenGame.Hud {
  public class HeadsUpDisplay : IUpdate {
    private readonly ScalingViewportAdapter _hudScale;
    private readonly DebugOverlay _debugOverlay;

    private readonly InventoryDisplay _inventoryDisplay;
    private readonly ToolDisplay _toolDisplay;
    private readonly ResourcesDisplay _resourcesDisplay;

    public HeadsUpDisplay(
        MainGame game,
        PlayerState playerState,
        GraphicsDevice graphicsDevice,
        int renderWidth,
        int renderHeight) {
      _hudScale = new ScalingViewportAdapter(graphicsDevice, renderWidth, renderHeight);
      _inventoryDisplay = new InventoryDisplay(game.Content, _hudScale, playerState.Inventory);
      _toolDisplay = new ToolDisplay(game.Content, _hudScale, playerState.Tools);
      _resourcesDisplay = new ResourcesDisplay(game.Content, _hudScale, playerState);
      _debugOverlay = new DebugOverlay(game, _hudScale);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      spriteBatch.Begin(
          transformMatrix: _hudScale.GetScaleMatrix(),
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp);
      
      _inventoryDisplay.Draw(spriteBatch, gameTime);
      _toolDisplay.Draw(spriteBatch, gameTime);
      _resourcesDisplay.Draw(spriteBatch, gameTime);
      _debugOverlay.Draw(spriteBatch, gameTime);
      
      spriteBatch.End();
    }

    public void Update(GameTime gameTime) {
      
    }
  }
}