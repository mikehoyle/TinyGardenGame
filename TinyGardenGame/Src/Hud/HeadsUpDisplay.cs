using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Player.State;

namespace TinyGardenGame.Hud {
  public class HeadsUpDisplay {
    public const int PaddingPx = 2;

    private readonly ScalingViewportAdapter _hudScale;
    private readonly DebugOverlay _debugOverlay;

    private readonly InventoryDisplay _inventoryDisplay;
    private readonly ToolDisplay _toolDisplay;
    private readonly ResourcesDisplay _resourcesDisplay;
    private readonly TimeDisplay _timeDisplay;

    public HeadsUpDisplay(
        MainGame game,
        PlayerState playerState,
        GameState.GameState gameState,
        GraphicsDevice graphicsDevice,
        int renderWidth,
        int renderHeight) {
      _hudScale = new ScalingViewportAdapter(graphicsDevice, renderWidth, renderHeight);
      _inventoryDisplay = new InventoryDisplay(_hudScale, playerState.Inventory);
      _toolDisplay = new ToolDisplay(_hudScale, playerState.Tools);
      _resourcesDisplay = new ResourcesDisplay(_hudScale, playerState);
      _timeDisplay = new TimeDisplay(_hudScale, gameState.Clock);
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
      _timeDisplay.Draw(spriteBatch, gameTime);
      _debugOverlay.Draw(spriteBatch, gameTime);

      spriteBatch.End();
    }
  }
}