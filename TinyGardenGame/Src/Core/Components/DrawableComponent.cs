using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;

namespace TinyGardenGame.Core.Components {
  public class DrawableComponent : IUpdate {
    public BaseDrawable Drawable { get; }
    public RenderLayer RenderLayer { get; }

    public SpriteEffects SpriteEffects { get; set; }

    public DrawableComponent(Sprite sprite, RenderLayer renderLayer = RenderLayer.GameObject)
        : this(new SpriteDrawable(sprite), renderLayer) { }

    public DrawableComponent(
        BaseDrawable drawable, RenderLayer renderLayer = RenderLayer.GameObject) {
      Drawable = drawable;
      RenderLayer = renderLayer;
      SpriteEffects = SpriteEffects.None;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position) {
      Drawable.Draw(spriteBatch, position, SpriteEffects);
    }

    /**
     * Note that this will be called in the Draw loop rather than the update loop,
     * so updates should be strictly related to drawing (like animation).
     */
    public void Update(GameTime gameTime) {
      Drawable.Update(gameTime);
    }

    public void SetAnimation(string animationName, bool loop = true) {
      Drawable.OnAnimationChange(animationName, loop);
    }
  }
}