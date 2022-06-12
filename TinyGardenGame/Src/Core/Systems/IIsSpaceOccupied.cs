using System.Drawing;

namespace TinyGardenGame.Core.Systems {
  public interface IIsSpaceOccupied {
    public bool IsSpaceOccupied(RectangleF target);
  }
}