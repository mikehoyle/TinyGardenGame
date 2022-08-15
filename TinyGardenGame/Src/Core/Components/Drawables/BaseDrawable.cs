using System.Collections.Generic;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components.Drawables {
  public abstract class BaseDrawable : IUpdate {
    public virtual HashSet<string> PossibleAnimations => new();

    public abstract void Draw(
        SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects);

    public virtual void Update(GameTime gameTime) { }
    public virtual void OnAnimationChange(string name, bool loop) { }
  }
}