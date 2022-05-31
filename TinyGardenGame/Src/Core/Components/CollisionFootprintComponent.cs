using MonoGame.Extended;

namespace TinyGardenGame.Core.Components {
  /**
   * Defines the collision area for an entity.
   * NOTE: This footprint corresponds to the map grid -- that is, before isometric projection.
   * As such, a rectangular footprint would have a practical diamond-shape in-game.
   *
   * The shape is assumed to have its origin at the center of the footprint
   */
  public class CollisionFootprintComponent {
    /**
     * Because we index to the NW, a RectangleF with top-left corner + H + W would actually
     * extend to the NE instead of the intuitive SE. As such, the footprint should actually
     * originate from the SW corner to accomodate.
     * TODO: Add a helper method for the above.
     */
    public RectangleF Footprint { get; set; }
  }
}