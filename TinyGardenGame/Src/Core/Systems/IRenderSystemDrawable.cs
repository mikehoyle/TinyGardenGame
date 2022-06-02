using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TinyGardenGame.Core.Systems {
  public interface IRenderSystemDrawable {
    void Draw(SpriteBatch spriteBatch, Vector2 position);
  }
}