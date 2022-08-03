// Monogame global usings
global using Microsoft.Xna.Framework.Audio;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Content;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Media;
global using Microsoft.Xna.Framework.Input;
global using Microsoft.Xna.Framework.Input.Touch;
global using MonoGame.Framework.Utilities;

// Project global usings and aliases
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