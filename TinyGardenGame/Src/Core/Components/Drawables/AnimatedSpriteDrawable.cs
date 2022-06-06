using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Graphics;

namespace TinyGardenGame.Core.Components.Drawables {
  public class AnimatedSpriteDrawable : BaseDrawable {
    private readonly AnimatedSprite _sprite;

    public AnimatedSpriteDrawable(AnimatedSprite sprite) {
      _sprite = sprite;
    }
    
    public override void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects) {
      _sprite.Position = position;
      _sprite.SpriteEffect = effects;
      _sprite.Render(spriteBatch);
    }

    public override void Update(GameTime gameTime) {
      _sprite.Update(gameTime);
    }

    public override void OnAnimationChange(string name) {
      _sprite.Play(name);
    }
  }
}