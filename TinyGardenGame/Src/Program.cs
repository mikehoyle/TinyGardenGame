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
using NLog;

namespace TinyGardenGame {
  public static class Program {
    [STAThread]
    static void Main() {
      ConfigureLogging();
      using (var game = new MainGame()) {
        game.Run();
      }
    }

    private static void ConfigureLogging() {
      var config = new NLog.Config.LoggingConfiguration();

      // Targets where to log to: File and Console
      var logFile = new NLog.Targets.FileTarget("logfile") { FileName = "logs.txt" };
      var logConsole = new NLog.Targets.ConsoleTarget("logconsole");
      
      // Rules for mapping loggers to targets            
      config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
      config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
      
      LogManager.Configuration = config;
    }
  }
}