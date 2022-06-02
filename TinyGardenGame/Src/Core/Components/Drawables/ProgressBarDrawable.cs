using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core.Systems;

namespace TinyGardenGame.Core.Components.Drawables {
  public class ProgressBarDrawable : IRenderSystemDrawable {
    private readonly TextureRegion2D _emptyTexture;
    private readonly TextureRegion2D _fullTexture;
    private double _progressPercentage;

    public int BorderSizePx { get; set; } = 1;
    public double ProgressPercentage {
      get => _progressPercentage;
      set => _progressPercentage = Math.Clamp(value, 0, 1);
    }

    public ProgressBarDrawable(
        TextureRegion2D emptyTexture, TextureRegion2D fullTexture, double currentProgress = 0d) {
      if (emptyTexture.Width != fullTexture.Width || emptyTexture.Height != fullTexture.Height) {
        throw new Exception("Invalid progress bar textures");
      }

      ProgressPercentage = currentProgress;
      _emptyTexture = emptyTexture;
      _fullTexture = fullTexture;
    }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 position) {
      var fullBounds = _fullTexture.Bounds;
      fullBounds.Width = (int)(_progressPercentage * (_emptyTexture.Width - (BorderSizePx * 2)))
                   + BorderSizePx;
      SpriteBatchDraw(spriteBatch, _emptyTexture.Texture, position, _emptyTexture.Bounds);
      SpriteBatchDraw(spriteBatch, _fullTexture.Texture, position, fullBounds);
    }
    
    private void SpriteBatchDraw(
        SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle bounds) {
      spriteBatch.Draw(
          texture,
          position,
          bounds,
          Color.White,
          rotation: 0f,
          origin: new Vector2(_emptyTexture.Width / 2f, 0),
          scale: Vector2.One,
          SpriteEffects.None,
          0f);
    }
  }
}