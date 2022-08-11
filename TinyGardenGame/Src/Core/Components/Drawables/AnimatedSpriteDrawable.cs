using System.Collections.Generic;
using System.Diagnostics;

namespace TinyGardenGame.Core.Components.Drawables {
  public class AnimatedSpriteDrawable : BaseDrawable {
    private readonly AsepriteAnimatedSprite _sprite;
    
    public sealed override HashSet<string> PossibleAnimations { get; }

    public AnimatedSpriteDrawable(AsepriteAnimatedSprite sprite) {
      _sprite = sprite;
      PossibleAnimations = new HashSet<string>();
      PossibleAnimations.UnionWith(sprite.Animations.Keys);
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects effects) {
      _sprite.Position = position;
      _sprite.SpriteEffect = effects;
      _sprite.Render(spriteBatch);
    }

    public override void Update(GameTime gameTime) {
      _sprite.Update(gameTime);
    }

    public override void OnAnimationChange(string name, bool loop) {
      if (!PossibleAnimations.Contains(name)) {
        Debug.WriteLine($"Attempted to animate with '{name}' but drawable can't use that string");
        return;
      }
      var animation = _sprite.Animations[name];
      animation.IsOneShot = !loop;
      _sprite.Animations[name] = animation;
      _sprite.Play(name);
    }
  }
}