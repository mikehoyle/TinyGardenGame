using MonoGame.Extended.Sprites;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;

namespace TinyGardenGame.Core.Components {
  public class DrawableComponent {
    public IRenderSystemDrawable Drawable { get; }
    public RenderLayer RenderLayer { get; }

    public DrawableComponent(Sprite sprite, RenderLayer renderLayer = RenderLayer.GameObject) {
      Drawable = new SpriteDrawable(sprite);
      RenderLayer = renderLayer;
    }
    
    public DrawableComponent(
        IRenderSystemDrawable drawable, RenderLayer renderLayer = RenderLayer.GameObject) {
      Drawable = drawable;
      RenderLayer = renderLayer;
    }
  }
}