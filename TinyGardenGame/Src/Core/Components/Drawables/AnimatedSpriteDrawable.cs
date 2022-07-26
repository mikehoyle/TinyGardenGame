namespace TinyGardenGame.Core.Components.Drawables {
  public class AnimatedSpriteDrawable : BaseDrawable {
    private readonly AsepriteAnimatedSprite _sprite;

    public AnimatedSpriteDrawable(AsepriteAnimatedSprite sprite) {
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

    public override void OnAnimationChange(string name, bool loop) {
      var animation = _sprite.Animations[name];
      animation.IsOneShot = !loop;
      _sprite.Animations[name] = animation;
      _sprite.Play(name);
    }
  }
}