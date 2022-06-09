using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace TinyGardenGame.Hud {
  public class HeadsUpDisplay : IUpdate {
    private readonly ScalingViewportAdapter _hudScale;
    
    public Inventory Inventory { get; }

    public HeadsUpDisplay(
        ContentManager contentManager,
        GraphicsDevice graphicsDevice,
        int renderWidth,
        int renderHeight) {
      _hudScale = new ScalingViewportAdapter(graphicsDevice, renderWidth, renderHeight);
      Inventory = new Inventory(contentManager, _hudScale);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      spriteBatch.Begin(
          transformMatrix: _hudScale.GetScaleMatrix(),
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp);
      
      Inventory.Draw(spriteBatch, gameTime);
      
      spriteBatch.End();
    }

    public void Update(GameTime gameTime) {
      
    }
  }
}