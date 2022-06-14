using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;

namespace TinyGardenGame.Core.Components.Drawables {
  public class SpriteDrawable : BaseDrawable {
    public Sprite Sprite { get; }

    public SpriteDrawable(Sprite sprite) {
      Sprite = sprite;
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects) {
      Sprite.Effect = effects;
      spriteBatch.Draw(Sprite, position);
    }
  }
}