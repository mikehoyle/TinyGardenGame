using MonoGame.Extended.TextureAtlases;

namespace TinyGardenGame.Core.Components; 

/**
 * Creates a visible meter over the entity, assuming that entity has a position.
 */
public class VisibleMeterComponent {
  private const int BorderSizePx = 1;

  public double CurrentFillPercentage { get; set; } = 0;
  // Top-center offset of sprite from position
  public Vector2 Offset { get; set; } = Vector2.Zero;
  
  public void Draw(
      SpriteBatch spriteBatch,
      Vector2 position,
      TextureRegion2D fullTexture,
      TextureRegion2D emptyTexture) {
    var textureOrigin = new Vector2(emptyTexture.Width / 2f, 0);
    var fullBounds = fullTexture.Bounds;
    fullBounds.Width = (int)(CurrentFillPercentage * (emptyTexture.Width - (BorderSizePx * 2)))
                       + BorderSizePx;
    position += Offset;
    
    SpriteBatchDraw(
        spriteBatch,
        emptyTexture.Texture,
        position,
        textureOrigin,
        emptyTexture.Bounds);
    SpriteBatchDraw(spriteBatch, fullTexture.Texture, position, textureOrigin, fullBounds);
  }
  
  private void SpriteBatchDraw(
      SpriteBatch spriteBatch,
      Texture2D texture,
      Vector2 position,
      Vector2 origin,
      Rectangle? bounds = null) {
    spriteBatch.Draw(
        texture,
        position,
        bounds,
        Color.White,
        rotation: 0f,
        origin,
        scale: Vector2.One,
        SpriteEffects.None,
        0f);
  }
}
