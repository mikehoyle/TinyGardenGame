using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core.Systems;

namespace TinyGardenGame.Core.Components.Drawables {
  public class SpriteDrawable : BaseDrawable {
    private readonly Sprite _sprite;

    public SpriteDrawable(Sprite sprite) {
      _sprite = sprite;
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects) {
      _sprite.Effect = effects;
      spriteBatch.Draw(_sprite, position);
    }
  }
}