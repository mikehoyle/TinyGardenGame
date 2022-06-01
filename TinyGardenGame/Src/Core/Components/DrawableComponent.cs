using MonoGame.Extended.Sprites;
using TinyGardenGame.Core.Systems;

namespace TinyGardenGame.Core.Components {
  public class DrawableComponent {
    public Sprite Sprite { get; }
    public RenderLayer RenderLayer { get; }

    public DrawableComponent(Sprite sprite, RenderLayer renderLayer = RenderLayer.GameObject) {
      Sprite = sprite;
      RenderLayer = renderLayer;
    }
  }
}