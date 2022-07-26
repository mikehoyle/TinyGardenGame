global using static TinyGardenGame.MapPlacementHelper;
global using static TinyGardenGame.MapPlacementHelper.Direction;
global using SysRectangleF = System.Drawing.RectangleF;
global using AsepriteSprite = MonoGame.Aseprite.Graphics.Sprite;
global using AsepriteAnimatedSprite = MonoGame.Aseprite.Graphics.AnimatedSprite;
using System;

namespace TinyGardenGame {
  public static class Program {
    [STAThread]
    static void Main() {
      using (var game = new MainGame())
        game.Run();
    }
  }
}