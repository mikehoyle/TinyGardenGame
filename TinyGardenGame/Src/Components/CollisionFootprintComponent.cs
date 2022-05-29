using MonoGame.Extended;

namespace TinyGardenGame.Components {
  /**
   * Defines the collision area for an entity.
   * NOTE: This footprint corresponds to the map grid -- that is, before isometric projection.
   * As such, a rectangular footprint would have a practical diamond-shape in-game.
   *
   * The shape is assumed to have its origin at the center of the footprint
   */
  public class CollisionFootprintComponent {
    public RectangleF Footprint { get; set; }
  }
}